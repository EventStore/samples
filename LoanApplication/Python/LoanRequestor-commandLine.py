# Import needed libraries
from esdbclient import EventStoreDBClient, NewEvent, StreamState
import json
import config
import time
import traceback
import uuid
import datetime

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

# Loop through the samples and append events into the ESDB streams
while True:
    print("Requesting a loan:")
    # Get the input field data
    _user = input("  What is the applicant's name? ")
    _nationalid = input("  What is the applicant's national ID? ")
    _amount = input("  What is the amount requested? ")
    _address = input("  What is the applicant's street address? ")
    _city = input("  What is the applicant's city? ")
    _region = input("  What is the applicant's region? ")
    _country = input("  What is the applicant's country? ")
    _postal = input("  What is the applicant's postal code? ")
    _loanrequestid = str(uuid.uuid4())
    _ts = str(datetime.datetime.now())

    # Create a dictionary for the event data, and compose the stream name to which we will append it
    _loan_request_data = {"User": _user, "NationalID": _nationalid, "Amount": _amount, "LoanRequestID": _loanrequestid, "LoanRequestedTimestamp": _ts, "RequestorAddress": _address, "RequestorCity": _city, "RequestorRegion": _region, "RequestorCountry": _country, "RequestorPostalCode": _postal}
    _loan_request_metadata = {"$correlationId": _loanrequestid, "$causationId": _loanrequestid, "transactionTimestamp": _ts}
    _loan_request_stream_name = config.STREAM_PREFIX + '-' + _loanrequestid

    # Create a loan request event
    loan_request_event = NewEvent(type=config.EVENT_TYPE_LOAN_REQUESTED, metadata=bytes(json.dumps(_loan_request_metadata), 'utf-8'), data=bytes(json.dumps(_loan_request_data), 'utf-8'), id=_loanrequestid)
    
    print('Command Received - LoanRequested: ' + str(_loan_request_data) + '\n  Appending to stream: ' + _loan_request_stream_name + '\n\n')
    
    # Append the event to the stream
    CURRENT_POSITION = esdb.append_to_stream(stream_name=_loan_request_stream_name, current_version=StreamState.ANY, events=[loan_request_event])

    # Wait a few seconds to let the event wind through the system
    time.sleep(20)

