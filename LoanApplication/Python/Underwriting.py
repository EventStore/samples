# Import needed libraries
from esdbclient import EventStoreDBClient, NewEvent
import json
import config
import time
import traceback

# Print some information for the user
print("\n\n***** Underwriting *****\n")

if config.DEBUG:
    print("Initializing...")
    print('  Connecting to ESDB...')

esdb = ''

# Create a connection to ESDB
while True:
    try:
        # Connect to ESDB using the configured URL
        esdb = EventStoreDBClient(uri=config.ESDB_URL)
        
        if config.DEBUG:
            print('  Connection succeeded!')
        
        # If the connection was successful, exit the loop
        break
    # If the connection failed, try again in 10 seconds
    except:
        print('Connection to ESDB failed, retrying in 10 seconds...')
        traceback.print_exc()
        time.sleep(10)

# Build the stream name to which we will subscribe
_subscription_stream_name=config.STREAM_ET_PREFIX+config.EVENT_TYPE_LOAN_APPROVAL_NEEDED

# Create variables for the checkpoint file and checkpoint value
_checkpoint = None
f = None

# Try opening the checkpoint file, read the checkpoint value, convert it to a number, and close the file
try:
    f = open(config.UNDERWRITING_CHECKPOINT_FILENAME)
    _checkpoint = int(f.read())
    f.close()

    if config.DEBUG:
        print('  Checkpoint: ' + str(_checkpoint))
# If we couldn't open the file, start reading from the beginning of the stream
except:
    print('No checkpoint file. Starting from the beginning of the subscription')
    traceback.print_exc()

# Create a catchup subscription to the stream
try:
    catchup_subscription = esdb.subscribe_to_stream(stream_name=_subscription_stream_name, resolve_links=True, stream_position=_checkpoint)
# If we failed to create the catchup subscription, the checkpoint might be bad, so start from the beginning
except:
    print('Bad checkpoint! Starting at the beginning of the stream...')
    catchup_subscription = esdb.subscribe_to_stream(stream_name=_subscription_stream_name, resolve_links=True)
    traceback.print_exc()

print('Waiting for events')

# For each event received through the subscription (note: this is a blocking operation)
for decision_needed_event in catchup_subscription:
    # Introduce some delay
    time.sleep(5)

    # Get the commit version to use for appending the decision event
    COMMIT_VERSION = esdb.get_current_version(stream_name=decision_needed_event.stream_name)

    if config.DEBUG:
        print('  Received event: id=' + str(decision_needed_event.id) + '; type=' + decision_needed_event.type + '; stream_name=' + str(decision_needed_event.stream_name) + '; data=\'' +  str(decision_needed_event.data) + '\'')

    # We need to build a read model for the Underwriter. Get the stream of events from ESDB for the stream in which the Approval Request appeared
    state_stream = esdb.read_stream(decision_needed_event.stream_name)

    # Create a new dictionary to hold the folded state that will become our read model
    _state = {}

    if config.DEBUG:
        print('  Reconstructing state...')

    # For each event in the stream
    for state_event in state_stream:
        if config.DEBUG:
            print('    Reading event: id=' + str(state_event.id) + '; type=' + state_event.type + '; stream_name=' + str(state_event.stream_name) + '; data=\'' +  str(state_event.data) + '\'')
        
        # Append the data of the event to the state
        _state = _state | json.loads(state_event.data)

    if config.DEBUG:
        print('  State ready: ' + str(_state))
    
    # Create a placeholder for the Underwriter's decision
    _decision_event_type = ''

    # Display our read model for the Underwriter to make a decision
    print("Underwriting needed:")
    for _state_key,_state_value in _state.items():
        print("  " + _state_key + ": " + str(_state_value))
    
    # Request the decision from the Underwriter as input from the keyboard
    keyboard_input = input("\n  NWould you like to approve this loan (Y/N)? ")

    # If the Underwrter gives us a case-insensitive Y for yes
    if keyboard_input in ("Y","y"):
        # Manually approve the Loan Request by setting the event type
        _decision_event_type = config.EVENT_TYPE_LOAN_MANUAL_APPROVED
    else:
        # Manually decline the Loan Request by setting the event type
        _decision_event_type = config.EVENT_TYPE_LOAN_MANUAL_DENIED

    # Create a dictionary holding the LoanRequestID upon which the Underwriter acted
    _decision_event_data = {"LoanRequestID": _state.get("LoanRequestID")}
    # Create a new event for the Underwriting decision
    decision_event = NewEvent(type=_decision_event_type, data=bytes(json.dumps(_decision_event_data), 'utf-8'))
    
    if config.DEBUG:
        print('  Processing loan decision - ' + _decision_event_type + ': ' + str(_decision_event_data) + '\n')
    else:
        print('  Processing loan decision - ' + _decision_event_type + '\n')
    
    # Append the decision event
    CURRENT_POSITION = esdb.append_to_stream(stream_name=decision_needed_event.stream_name, current_version=COMMIT_VERSION, events=[decision_event])

    # Open the checkpoint file and write the stream position of the event as our checkpoint
    # In practice, we'd likely do this every so often, not after every single event
    f = open(config.UNDERWRITING_CHECKPOINT_FILENAME, 'w')
    f.write(str(decision_needed_event.link.stream_position))
    f.close()
    
    if config.DEBUG:
        print('  Checkpoint: ' + str(decision_needed_event.link.stream_position) + '\n')

