# Import needed libraries
from esdbclient import EventStoreDBClient, NewEvent, StreamState
import json
import config
import time
import traceback
import uuid

# Print some information for the user
print("\n\n***** Loan Request Command Processor *****\n")

if config.DEBUG:
    print('Connecting to ESDB...')

# Create a connection to ESDB
while True:
    try:
        # Connect to ESDB using the URL in the config file
        esdb = EventStoreDBClient(uri=config.ESDB_URL)
        
        if config.DEBUG:
            print('  Connection succeeded!')
        
        # If the connection was successul, exit the loop
        break
    # If the connection fails, retry again in 10 seconds
    except:
        print('Connection to ESDB failed, retrying in 10 seconds...')
        traceback.print_exc()
        time.sleep(10)

# Let's create some event data that will be appended to ESDB streams, simulating loan approval requests
_loan_request_data = []
_loan_request_stream_name = []
_loan_request_uuid = []

# Simulate an automatically approved loan
_uuid = str(uuid.uuid4())
_loan_request_data.append({"User": "Yves", "NationalID": 12345, "Amount": 10000, "LoanRequestID": _uuid})
_loan_request_stream_name.append(config.STREAM_PREFIX + '-' + _uuid)
_loan_request_uuid.append(_uuid)
 
# Simulate a loan requiring manual approval / denial
_uuid = str(uuid.uuid4())
_loan_request_data.append({"User": "Tony", "NationalID": 54321, "Amount": 5000, "LoanRequestID": _uuid})
_loan_request_stream_name.append(config.STREAM_PREFIX + '-' + _uuid)
_loan_request_uuid.append(_uuid)

# Simulate another loan requiring manual approval / denial
_uuid = str(uuid.uuid4())
_loan_request_data.append({"User": "David", "NationalID": 43521, "Amount": 5000, "LoanRequestID": _uuid})
_loan_request_stream_name.append(config.STREAM_PREFIX + '-' + _uuid)
_loan_request_uuid.append(_uuid)

# Simulate a loan that is automatically denied
_uuid = str(uuid.uuid4())
_loan_request_data.append({"User": "Rob", "NationalID": 34251, "Amount": 3000, "LoanRequestID": _uuid})
_loan_request_stream_name.append(config.STREAM_PREFIX + '-' + _uuid)
_loan_request_uuid.append(_uuid)

# Create a loop counter
loop_counter = 0
# Loop through the samples and append events into the ESDB streams
while loop_counter < len(_loan_request_data):
    # Create a loan request event
    loan_request_event = NewEvent(type=config.EVENT_TYPE_LOAN_REQUESTED, data=bytes(json.dumps(_loan_request_data[loop_counter]), 'utf-8'), id=_loan_request_uuid[loop_counter])
    
    print('Command Received - LoanRequested: ' + str(_loan_request_data[loop_counter]) + '\n  Appending to stream: ' + _loan_request_stream_name[loop_counter])
    
    # Append the event to the stream
    CURRENT_POSITION = esdb.append_to_stream(stream_name=_loan_request_stream_name[loop_counter], current_version=StreamState.ANY, events=[loan_request_event])

    # Wait a few seconds to let the event wind through the system
    time.sleep(10)

    # Increment the loop counter
    loop_counter+=1

