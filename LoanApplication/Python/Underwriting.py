# Import needed libraries
from esdbclient import NewEvent
import json
import config
import logging
import time
import traceback
import datetime
import random
import pprint
import utils

log = logging.getLogger("underwriting")

# Print some information for the user
log.info("***** Underwriting *****")

if config.DEBUG:
    log.info("Initializing...")
    log.info('Connecting to ESDB...')

esdb = ''

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
        log.debug('Checkpoint: ' + str(_checkpoint))
# If we couldn't open the file, start reading from the beginning of the stream
except:
    log.error('No checkpoint file. Starting from the beginning of the subscription')
    traceback.print_exc()

# Create a catchup subscription to the stream
try:
    catchup_subscription = esdb.subscribe_to_stream(stream_name=_subscription_stream_name, resolve_links=True, stream_position=_checkpoint)
# If we failed to create the catchup subscription, the checkpoint might be bad, so start from the beginning
except:
    log.error('Bad checkpoint! Starting at the beginning of the stream...')
    catchup_subscription = esdb.subscribe_to_stream(stream_name=_subscription_stream_name, resolve_links=True)
    traceback.print_exc()

log.info('Waiting for events')

# For each event received through the subscription (note: this is a blocking operation)
for decision_needed_event in catchup_subscription:
    # Introduce some delay
    time.sleep(config.CLIENT_PRE_WORK_DELAY)
    
    if config.DEBUG:
        log.debug('Received event: id=' + str(decision_needed_event.id) + '; type=' + decision_needed_event.type + '; stream_name=' + str(decision_needed_event.stream_name) + '; data=\'' +  str(decision_needed_event.data) + '\'')

    # Get the commit version to use for appending the decision event
    COMMIT_VERSION = esdb.get_current_version(stream_name=decision_needed_event.stream_name)
    
    # Get the metadata of the decision needed event, which will serve as the basis for the metadata of the decision event
    _decision_needed_metadata = json.loads(decision_needed_event.metadata)

    # We need to build a read model for the Underwriter. Get the stream of events from ESDB for the stream in which the Approval Request appeared
    state_stream = esdb.read_stream(decision_needed_event.stream_name)

    # Create a new dictionary to hold the folded state that will become our read model
    _state = {}

    if config.DEBUG:
        log.debug('Reconstructing state...')

    # For each event in the stream
    for state_event in state_stream:
        if config.DEBUG:
            log.debug('Reading event: id=' + str(state_event.id) + '; type=' + state_event.type + '; stream_name=' + str(state_event.stream_name) + '; data=\'' +  str(state_event.data) + '\'')
        
        # Append the data of the event to the state
        if state_event.type in (config.EVENT_TYPE_LOAN_REQUESTED, config.EVENT_TYPE_CREDIT_CHECKED):
            _state = _state | json.loads(state_event.data)

    if config.DEBUG:
        log.debug('State ready: ' + str(_state))
    
    # Create a placeholder for the Underwriter's decision
    _decision_event_type = ''
    _in_approver_name = ''
    _in_approve_deny = ''
    
    log.info("Underwriting needed:")

     # Display our read model for the Underwriter to make a decision
    _pp = pprint.PrettyPrinter(indent=2)
    _pp.pprint(_state)

    # If we're using the AI underwriting suggestions, get a recommendation from the engine
    if config.UNDERWRITING_AI_SUGGESTIONS:
        from openai import OpenAI
        log.info('Requesting a summary from AI...')

        fine_tuning = config.UNDERWRITING_BASE_FINE_TUNING

        fine_tuning = fine_tuning + [  
                {"role": "system", "content": str(_state)},
            ]
                        
        messages = fine_tuning + config.UNDERWRITING_AI_QUESTION
        
        if config.DEBUG:
            log.debug('messages = ' + str(messages))
        
        try:
            openai = OpenAI(base_url=config.LOCAL_AI_URL, api_key=config.OPENAI_API_KEY)

            completion = openai.chat.completions.create(
                model=config.LOCAL_AI_BASE_MODEL,
                messages=messages
            )

            _ts = str(datetime.datetime.now())
            _recommendedBy = config.LOCAL_AI_ENGINE + '/' + config.LOCAL_AI_BASE_MODEL

            _ai_event_data = json.loads(completion.choices[0].message.content.replace('```json\n','').replace('```',''))
            _ai_event_data = _ai_event_data | {"SummarizedBy": _recommendedBy,
                                                "SummarizedAt": _ts}
            _ai_event_metadata = {"$correlationId": _decision_needed_metadata.get("$correlationId"),
                                    "$causationId": str(decision_needed_event.id),
                                    "transactionTimestamp": _ts}

            # Create a new event for the Underwriting decision
            ai_event = NewEvent(type=config.EVENT_TYPE_AUTOMATED_RECOMMENDATION, metadata=bytes(json.dumps(_ai_event_metadata), 'utf-8'), data=bytes(json.dumps(_ai_event_data), 'utf-8'))
            
            if config.DEBUG:
                log.debug('Recording AI Summary - ' + config.EVENT_TYPE_AUTOMATED_RECOMMENDATION + ': ' + str(_ai_event_data) + '...')
            else:
                log.info('Recording AI Summary...')
        
        
            # Append the decision event
            CURRENT_POSITION = esdb.append_to_stream(stream_name=decision_needed_event.stream_name, current_version=COMMIT_VERSION, events=[ai_event])
            COMMIT_VERSION = COMMIT_VERSION + 1
            log.info('Summary Recorded!')

            log.info('\n-----AI\'s Summary-----\n')
        
            ai_recommendation = json.loads(completion.choices[0].message.content)
            # Display our read model for the Underwriter to make a decision
            for _ai_recommendation_key,_ai_recommendation_value in ai_recommendation.items():
                log.info("  " + _ai_recommendation_key + ": " + str(_ai_recommendation_value))

            log.info('\n-----AI\'s Summary-----')
        except:
            traceback.print_exc()
            log.error('Sorry, something went wrong, and we weren\'t able to complete the integration, or record it!')

    # If we're using the automated underwriting function, take a random underwriter name and decision
    if config.AUTOMATED_UNDERWRITING:
        _in_approver_name = random.choice(config.UNDERWRITING_USERS)
        _in_approve_deny = random.choice(config.UNDERWRITING_ANSWERS)

        log.info("What is the approver's name? " + _in_approver_name)
        log.info("Would " + _in_approver_name + " like to approve this loan (Y/N)? " + _in_approve_deny)
    else:
        # Otherwise, request the decision from the Underwriter as input from the keyboard
        _in_approver_name = input("\n  What is the approver's name? ")
        _in_approve_deny = input("\n  NWould " + _in_approver_name + " like to approve this loan (Y/N)? ")

    # If the Underwrter gives us a case-insensitive Y for yes
    if _in_approve_deny in ("Y","y"):
        # Manually approve the Loan Request by setting the event type
        _decision_event_type = config.EVENT_TYPE_LOAN_MANUAL_APPROVED
    else:
        # Manually decline the Loan Request by setting the event type
        _decision_event_type = config.EVENT_TYPE_LOAN_MANUAL_DENIED

    _ts = str(datetime.datetime.now())

    # Create a dictionary holding the LoanRequestID upon which the Underwriter acted
    _decision_event_data = {"LoanRequestID": _state.get("LoanRequestID"), "LoanManualDecisionTimestamp": _ts, "ApproverName": _in_approver_name}
    _decision_event_metadata = {"$correlationId": _decision_needed_metadata.get("$correlationId"), "$causationId": str(decision_needed_event.id), "transactionTimestamp": _ts}
    
    # Create a new event for the Underwriting decision
    decision_event = NewEvent(type=_decision_event_type, metadata=bytes(json.dumps(_decision_event_metadata), 'utf-8'), data=bytes(json.dumps(_decision_event_data), 'utf-8'))
    
    if config.DEBUG:
        log.debug('Processing loan decision - ' + _decision_event_type + ': ' + str(_decision_event_data) + '\n')
    else:
        log.info('  Processing loan decision - ' + _decision_event_type + '\n')
    
    # Append the decision event
    CURRENT_POSITION = esdb.append_to_stream(stream_name=decision_needed_event.stream_name, current_version=COMMIT_VERSION, events=[decision_event])

    # Open the checkpoint file and write the stream position of the event as our checkpoint
    # In practice, we'd likely do this every so often, not after every single event
    f = open(config.UNDERWRITING_CHECKPOINT_FILENAME, 'w')
    f.write(str(decision_needed_event.link.stream_position))
    f.close()
    
    if config.DEBUG:
        log.debug('Checkpoint: ' + str(decision_needed_event.link.stream_position) + '\n')

    # Introduce some delay
    time.sleep(config.CLIENT_POST_WORK_DELAY)
