# Import needed libraries
from esdbclient import EventStoreDBClient, NewEvent
import json
import config
import time
import traceback

# Print some messages for the user
print("\n\n***** LoanDecider *****\n")

if config.DEBUG:
    print("Initializing...")
    print('  Connecting to ESDB...')

# Create a connection to ESDB
while True:
    try:
        # Connect with the configured URL
        esdb = EventStoreDBClient(uri=config.ESDB_URL)
        
        if config.DEBUG:
            print('  Connection succeeded!')
        
        # If the connection was successful, break from the loop
        break
    # If the connection fails, try again in 10 seconds
    except:
        print('Connection to ESDB failed, retrying in 10 seconds...')
        traceback.print_exc()
        time.sleep(10)

# Create the stream name to that we will subscribe for new events
_credit_check_stream_name=config.STREAM_ET_PREFIX+config.EVENT_TYPE_CREDIT_CHECKED

# Get the checkpoint
_checkpoint = None
f = None

# Open the checkpoint file, read the checkpoint value, convert it to an integer, and close the file
try:
    f = open(config.LOANDECIDER_CHECKPOINT_FILENAME)
    _checkpoint = int(f.read())
    f.close()

    if config.DEBUG:
        print('  Checkpoint: ' + str(_checkpoint))
# If we get an exception, start processing from the beginning of the stream
except:
    print('No checkpoint file. Starting from the beginning of the subscription')
    traceback.print_exc()

# Subscribe to the stream and wait for events to arrive
try:
    catchup_subscription = esdb.subscribe_to_stream(stream_name=_credit_check_stream_name, resolve_links=True, stream_position=_checkpoint)
# If we fail to subscribe, instead start at the beginning of the stream
except:
    print('Bad checkpoint! Starting at the beginning of the stream...')
    catchup_subscription = esdb.subscribe_to_stream(stream_name=_credit_check_stream_name, resolve_links=True)
    traceback.print_exc()

print('Waiting for events')

# For each event in the subscription
for credit_check_event in catchup_subscription:
    # Introduce some delay
    time.sleep(5)
    
    # Get the current commit version for the stream to which we will append
    COMMIT_VERSION = esdb.get_current_version(stream_name=credit_check_event.stream_name)
    
    if config.DEBUG:
        print('  Received event: id=' + str(credit_check_event.id) + '; type=' + credit_check_event.type + '; stream_name=' + str(credit_check_event.stream_name) + '; data=\'' +  str(credit_check_event.data) + '\'')

    # We need to reconstruct state so that we can decide whether to automatically or manually process the loan, and push it onwards for further processing
    # Read the stream for this loan request
    state_stream = esdb.read_stream(credit_check_event.stream_name)
    # Create a data structure to hold the state
    _state_data = {}

    if config.DEBUG:
        print('  Reconstructing state...')
    # Read the events in the stream and reconstruct the state
    for state_event in state_stream:
        if config.DEBUG:
            print('    Reading event: id=' + str(state_event.id) + '; type=' + state_event.type + '; stream_name=' + str(state_event.stream_name) + '; data=\'' +  str(state_event.data) + '\'')
        # Append the event data to left-fold the event into the state
        _state_data = _state_data | json.loads(state_event.data)

    if config.DEBUG:
        print('  State ready: ' + str(_state_data))
    
    # Create a placeholder for our approval decision event type
    _decision_event_type = ''

    # If the credit score is 7 or above, automatically approve it
    if _state_data.get("Score") >= 7:
        _decision_event_type = config.EVENT_TYPE_LOAN_AUTO_APPROVED
    # If the credit score is 5 - 6, send it to underwriting for manual decision
    elif _state_data.get("Score") >= 5:
        _decision_event_type = config.EVENT_TYPE_LOAN_APPROVAL_NEEDED
    # Otherwise, automatically deny the loan
    else:
        _decision_event_type = config.EVENT_TYPE_LOAN_AUTO_DENIED

    # Create a dictionary holding the elements of the loan decision
    _decision_event_data = {"LoanRequestID": _state_data.get("LoanRequestID")}
    # Create a decision event
    decision_event = NewEvent(type=_decision_event_type, data=bytes(json.dumps(_decision_event_data), 'utf-8'))

    if config.DEBUG:
        print('  Processing loan decision - ' + _decision_event_type + ': ' + str(_decision_event_data) + '\n')
    
    print('  Processing loan decision for ' + _state_data['User'] + ' with Score of ' + str(_state_data['Score']) + ' - ' + _decision_event_type + '\n  Appending to stream: ' + credit_check_event.stream_name + '\n')
    
    # Append the decision event to the stream
    CURRENT_POSITION = esdb.append_to_stream(stream_name=credit_check_event.stream_name, current_version=COMMIT_VERSION, events=[decision_event])

    # Open the checkpoint file and write the stream position of the event
    # In practice, we'd probably do this every so often, not for every event
    f = open(config.LOANDECIDER_CHECKPOINT_FILENAME, 'w')
    f.write(str(credit_check_event.link.stream_position))
    f.close()
    
    if config.DEBUG:
        print('  Checkpoint: ' + str(credit_check_event.link.stream_position) + '\n')

    # Introduce some delay
    time.sleep(5)

