# Import needed libraries
from esdbclient import EventStoreDBClient, NewEvent
import json
import config
import time
import traceback

# Print some information messages to the user
print("\n\n***** CreditCheck *****\n")

if config.DEBUG:
    print("Initializing...")
    print('  Connecting to ESDB...')

# Create a connection to ESDB
while True:
    try:
        # Connect to ESDB using the config parameter
        esdb = EventStoreDBClient(uri=config.ESDB_URL)
        
        if config.DEBUG:
            print('  Connection succeeded!')
        
        # If successful, break from the while loop
        break
    # If the connection fails, print the exception and try again in 10 seconds
    except:
        print('Connection to ESDB failed, retrying in 10 seconds...')
        traceback.print_exc()
        time.sleep(10)

# Set up the name of the Event Type stream to which we will listen for events to check credit
_loan_request_stream_name=config.STREAM_ET_PREFIX+config.EVENT_TYPE_LOAN_REQUESTED

# Get a list of the current persistent subscriptions to ESDB with the given stream name
available_subscriptions = esdb.list_subscriptions_to_stream(stream_name=_loan_request_stream_name)

# Check if the persistent subscription we want already exists
if len(available_subscriptions) == 0:
    if config.DEBUG:
        print('  Subscription doesn\'t exist, creating...')
    # If not, then create the persistent subscription
    esdb.create_subscription_to_stream(group_name=config.GROUP_NAME, stream_name=_loan_request_stream_name, resolve_links=True, message_timeout=600)
else:
    # if it does, then do nothing
    if config.DEBUG:
        print('  Subscription already exists. Skipping...')

# Start reading the persistent subscription
print('Waiting for LoanRequest events')
persistent_subscription = esdb.read_subscription_to_stream(group_name=config.GROUP_NAME, stream_name=_loan_request_stream_name)

# For each eent received
for loan_request_event in persistent_subscription:
    # Introduce some delay
    time.sleep(5)

    if config.DEBUG:
        print('  Received event: id=' + str(loan_request_event.id) + '; type=' + loan_request_event.type + '; stream_name=' + str(loan_request_event.stream_name) + '; data=\'' +  str(loan_request_event.data) + '\'')

    # Get the event data out of JSON format and into a python dictionary so we can use it
    _loan_request_data = json.loads(loan_request_event.data)

    # Create a data structure to hold the credit score that we will return
    _credit_score = -1

    # Map the requestor name to a credit score
    # In practice this portion of code would call out to some credit clearing house for an actual credit score, or apply other business rules
    if _loan_request_data.get("User") == "Yves":
        _credit_score = 9
    elif _loan_request_data.get("User") == "Tony":
        _credit_score = 5
    elif _loan_request_data.get("User") == "David":
        _credit_score = 6
    elif _loan_request_data.get("User") == "Rob":
        _credit_score = 1
    # If we can't map the user name to one of the above, send for manual processing
    else:
        _credit_score = 5

    # Create a dictionary with the credit check data
    _credit_checked_event_data = {"Score": _credit_score, "NationalID": _loan_request_data.get("NationalID")}
    # Create a credit checked event with the event data
    credit_checked_event = NewEvent(type=config.EVENT_TYPE_CREDIT_CHECKED, data=bytes(json.dumps(_credit_checked_event_data), 'utf-8'))
    
    if config.DEBUG:
        print('  Processing credit check - CreditChecked: ' + str(_credit_checked_event_data) + '\n')
    
    print('  Processing credit check - CreditChecked for NationalID ' + str(_credit_checked_event_data["NationalID"]) + ' with a Score of ' + str(_credit_checked_event_data["Score"]))

    # Get the current commit version of the loan request stream for this loan request
    COMMIT_VERSION = esdb.get_current_version(stream_name=loan_request_event.stream_name)
    # Append the event to the stream
    CURRENT_POSITION = esdb.append_to_stream(stream_name=loan_request_event.stream_name, current_version=COMMIT_VERSION, events=[credit_checked_event])

    # Acknowledge the original event
    persistent_subscription.ack(loan_request_event.ack_id)

    # Introduce some delay
    time.sleep(5)

