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

# Loop through the samples and append events into the ESDB streams
while True:
    print("Requesting a loan:")
    # Get the input field data

    _name = input("  What is the applicant's full name? ")
    _nationalid = input("  What is the applicant's national ID? ")
    _age = input("  What is the applicant's age in years? ")
    _gender = input("  What is the applicant's gender? ")
    _maritalStatus = input("  What is the applicant's marital status? ")
    _dependents = input("  How many dependents does the applicant have? ")
    _address = input("  What is the applicant's street address (i.e. '123 Outta My Way')? ")
    _city = input("  What is the applicant's city? ")
    _region = input("  What is the applicant's region (i.e. 'Ontario')? ")
    _country = input("  What is the applicant's country? ")
    _postal = input("  What is the applicant's postal/zip code? ")
    _amount = input("  What is the loan amount the applicant is requesting? ")
    _levelOfEducation = input("  What is the applicant's level of education? ")
    _employerName = input("  What is the name of the applicant's employer? ")
    _income = input("  What is the applicant's yearly income? ")
    _jobTitle = input("  What is the applicant's job title? ")
    _jobSeniority = input("  What is the applicant's job seniority in years (i.e. '3.5')? ")
    _loanrequestid = str(uuid.uuid4())
    _ts = str(datetime.datetime.now())

    # Create a dictionary for the event data, and compose the stream name to which we will append it
    _loan_request_data = { 'Address': { 'City': _city,
               'Country': _country,
               'PostalCode': _postal,
               'Region': _region,
               'Street': _address},
            'Age': _age,
            'Amount': _amount,
            'Dependents': _dependents,
            'EmployerName': _employerName,
            'Gender': _gender,
            'Income': _income,
            'JobSeniority': _jobSeniority,
            'JobTitle': _jobTitle,
            'LevelOfEducation': _levelOfEducation,
            'LoanRequestID': _loanrequestid,
            'LoanRequestedTimestamp': _ts,
            'MaritalStatus': _maritalStatus,
            'Name': _name,
            'NationalID': _nationalid}
    _loan_request_metadata = {"$correlationId": _loanrequestid, "$causationId": _loanrequestid, "transactionTimestamp": _ts}
    _loan_request_stream_name = config.STREAM_PREFIX + '-' + _loanrequestid

    # Create a loan request event
    loan_request_event = NewEvent(type=config.EVENT_TYPE_LOAN_REQUESTED, metadata=bytes(json.dumps(_loan_request_metadata), 'utf-8'), data=bytes(json.dumps(_loan_request_data), 'utf-8'), id=_loanrequestid)
    
    log.info('Command Received - LoanRequested: ' + str(_loan_request_data) + '\n  Appending to stream: ' + _loan_request_stream_name + '\n\n')
    
    # Append the event to the stream
    CURRENT_POSITION = esdb.append_to_stream(stream_name=_loan_request_stream_name, current_version=StreamState.ANY, events=[loan_request_event])

    # Wait a few seconds to let the event wind through the system
    time.sleep(20)

