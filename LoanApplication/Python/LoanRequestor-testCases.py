# Import needed libraries
from esdbclient import EventStoreDBClient, NewEvent, StreamState
import json
import config
import time
import traceback
import uuid
import datetime
import utils
import logging

log = logging.getLogger("loanrequestor")

# Print some information for the user
log.info("***** LoanRequestor *****")

if config.DEBUG:
    log.info("Initializing...")
    log.info('Connecting to ESDB...')

# Create a connection to ESDB
while True:
    try:
        esdb = utils.create_db_client()
        if config.DEBUG:
            log.info('Connection succeeded!')

        # If the connection was successful, exit the loop
        break
    # If the connection failed, try again in 10 seconds
    except:
        log.error('Connection to ESDB failed, retrying in 10 seconds...')
        traceback.print_exc()
        time.sleep(10)

# Let's create some event data that will be appended to ESDB streams, simulating loan approval requests
_loan_request_data = []
_loan_request_metadata = []
_loan_request_stream_name = []
_loan_request_uuid = []

# Simulate an automatically approved loan
_uuid = str(uuid.uuid4())
_ts = str(datetime.datetime.now())

# Create a dictionary for the event data, and compose the stream name to which we will append it
_loan_request_dict = { 'Address': { 'City': 'Somecity',
            'Country': 'Somecountry',
            'PostalCode': 'ABC123',
            'Region': 'Someregion',
            'Street': '123 Outta My Way'},
        'Age': 35,
        'Amount': 10000,
        'Dependents': 2,
        'EmployerName': 'Acme Co',
        'Gender': 'Male',
        'Income': 100000,
        'JobSeniority': 2.5,
        'JobTitle': 'Operator',
        'LevelOfEducation': 'High School Diploma',
        'LoanRequestID': _uuid,
        'LoanRequestedTimestamp': _ts,
        'MaritalStatus': 'Married',
        'Name': 'Yves',
        'NationalID': 12345}

_loan_request_data.append(_loan_request_dict)
_loan_request_metadata.append({"$correlationId": _uuid, "$causationId": _uuid, "transactionTimestamp": _ts})
_loan_request_stream_name.append(config.STREAM_PREFIX + '-' + _uuid)
_loan_request_uuid.append(_uuid)
 
# Simulate a loan requiring manual approval / denial
_uuid = str(uuid.uuid4())
_ts = str(datetime.datetime.now())

# Create a dictionary for the event data, and compose the stream name to which we will append it
_loan_request_dict = { 'Address': { 'City': 'Mycity',
            'Country': 'Mycountry',
            'PostalCode': 'DEF456',
            'Region': 'Myregion',
            'Street': '456 Some Street'},
        'Age': 35,
        'Amount': 5000,
        'Dependents': 2,
        'EmployerName': 'Acme Co',
        'Gender': 'Male',
        'Income': 50000,
        'JobSeniority': 2.5,
        'JobTitle': 'Operator',
        'LevelOfEducation': 'High School Diploma',
        'LoanRequestID': _uuid,
        'LoanRequestedTimestamp': _ts,
        'MaritalStatus': 'Married',
        'Name': 'Tony',
        'NationalID': 54321}

_loan_request_data.append(_loan_request_dict)
_loan_request_metadata.append({"$correlationId": _uuid, "$causationId": _uuid, "transactionTimestamp": _ts})
_loan_request_stream_name.append(config.STREAM_PREFIX + '-' + _uuid)
_loan_request_uuid.append(_uuid)

# Simulate another loan requiring manual approval / denial
_uuid = str(uuid.uuid4())
_ts = str(datetime.datetime.now())

# Create a dictionary for the event data, and compose the stream name to which we will append it
_loan_request_dict = { 'Address': { 'City': 'Smalltown',
            'Country': 'Smallcountry',
            'PostalCode': 'HIJ789',
            'Region': 'Smallregion',
            'Street': '789 Back Lane'},
        'Age': 35,
        'Amount': 5000,
        'Dependents': 2,
        'EmployerName': 'Acme Co',
        'Gender': 'Male',
        'Income': 25000,
        'JobSeniority': 2.5,
        'JobTitle': 'Operator',
        'LevelOfEducation': 'High School Diploma',
        'LoanRequestID': _uuid,
        'LoanRequestedTimestamp': _ts,
        'MaritalStatus': 'Married',
        'Name': 'David',
        'NationalID': 43521}

_loan_request_data.append(_loan_request_dict)
_loan_request_metadata.append({"$correlationId": _uuid, "$causationId": _uuid, "transactionTimestamp": _ts})
_loan_request_stream_name.append(config.STREAM_PREFIX + '-' + _uuid)
_loan_request_uuid.append(_uuid)

# Simulate a loan that is automatically denied
_uuid = str(uuid.uuid4())
_ts = str(datetime.datetime.now())

# Create a dictionary for the event data, and compose the stream name to which we will append it
_loan_request_dict = { 'Address': { 'City': 'Fakecity',
            'Country': 'Fakeland',
            'PostalCode': 'KLM012',
            'Region': 'Fakeregion',
            'Street': '742 Evergreen Terrace'},
        'Age': 35,
        'Amount': 3000,
        'Dependents': 2,
        'EmployerName': 'Acme Co',
        'Gender': 'Male',
        'Income': 10000,
        'JobSeniority': 2.5,
        'JobTitle': 'Operator',
        'LevelOfEducation': 'High School Diploma',
        'LoanRequestID': _uuid,
        'LoanRequestedTimestamp': _ts,
        'MaritalStatus': 'Married',
        'Name': 'Rob',
        'NationalID': 34251}

_loan_request_data.append(_loan_request_dict)
_loan_request_metadata.append({"$correlationId": _uuid, "$causationId": _uuid, "transactionTimestamp": _ts})
_loan_request_stream_name.append(config.STREAM_PREFIX + '-' + _uuid)
_loan_request_uuid.append(_uuid)

# Create a loop counter
loop_counter = 0
# Loop through the samples and append events into the ESDB streams
while loop_counter < len(_loan_request_data):
    # Create a loan request event
    loan_request_event = NewEvent(type=config.EVENT_TYPE_LOAN_REQUESTED, metadata=bytes(json.dumps(_loan_request_metadata[loop_counter]), 'utf-8'), data=bytes(json.dumps(_loan_request_data[loop_counter]), 'utf-8'), id=_loan_request_uuid[loop_counter])
    
    log.info('Command Received - LoanRequested: ' + str(_loan_request_data[loop_counter]) + '\n  Appending to stream: ' + _loan_request_stream_name[loop_counter])
    
    # Append the event to the stream
    CURRENT_POSITION = esdb.append_to_stream(stream_name=_loan_request_stream_name[loop_counter], current_version=StreamState.ANY, events=[loan_request_event])

    # Wait a few seconds to let the event wind through the system
    time.sleep(20)

    # Increment the loop counter
    loop_counter+=1

