import os
import logging

logging.basicConfig(level=logging.INFO)

# AI key and endpoint configuration parameters
OPENAI_API_KEY = ''
try:
    OPENAI_API_KEY = os.environ['OPENAI_API_KEY']
except:
    OPENAI_API_KEY = 'NOT SET!'

OPENAI_URL = 'https://api.openai.com/v1'
#LOCAL_AI_URL = 'http://127.0.0.1:8000/v1'
LOCAL_AI_URL = 'https://api.openai.com/v1'

# These configuration parameters control the names of streams, events, and consumers
GROUP_NAME = 'LoanProcessing'
STREAM_PREFIX = 'loanRequest'
STREAM_CE_PREFIX = '$ce-'
STREAM_ET_PREFIX = '$et-'
EVENT_TYPE_LOAN_REQUESTED = 'LoanRequested'
EVENT_TYPE_CREDIT_CHECKED = "CreditChecked"
EVENT_TYPE_LOAN_AUTO_APPROVED = "LoanAutomaticallyApproved"
EVENT_TYPE_LOAN_AUTO_DENIED = "LoanAutomaticallyDenied"
EVENT_TYPE_LOAN_APPROVAL_NEEDED = "LoanApprovalNeeded"
EVENT_TYPE_LOAN_MANUAL_APPROVED = "LoanManuallyApproved"
EVENT_TYPE_LOAN_MANUAL_DENIED = "LoanManuallyDenied"
EVENT_TYPE_AUTOMATED_RECOMMENDATION = "AutomatedSummary"

# Enable DEBUG mode to see debug messages printed onto the console
DEBUG = False

# Delay pre and post work tell the scripts to pause for a certain amount of time so the activities aren't completed so quickly that the user can't see what's happening
CLIENT_PRE_WORK_DELAY = 5
CLIENT_POST_WORK_DELAY = 5

# These parameters control Automated Underwriting, allowing the Underwriting app to run headless for demo purposes
AUTOMATED_UNDERWRITING = True
UNDERWRITING_USERS = ['John','Jane']
UNDERWRITING_ANSWERS = ['Y','N']
UNDERWRITING_AI_SUGGESTIONS = False

# Connection URL for ESDB
ESDB_URL = os.getenv("ESDB_URL", "esdb://admin:changeit@localhost:2113?Tls=true")
ESDB_ROOT_CA_PATH = os.getenv("ESDB_ROOT_CA_PATH", '')

# Name of checkpoint files, which are written into the working directory
UNDERWRITING_CHECKPOINT_FILENAME = 'underwriting.checkpoint'
LOANDECIDER_CHECKPOINT_FILENAME = 'loandecider.checkpoint'
LOANREQUESTOR_CHECKPOINT_FILENAME = 'loanrequestor.checkpoint'

# These parameters control integration with OpenAI
OPENAI_BASE_MODEL = "gpt-3.5-turbo"

# These parameters control fine-tuning the OpenAI Chatbot example
CHATBOT_BASE_FINE_TUNING=[
    {"role": "system", "content": "You are a helpful virtual assistant working for a company providing bank loans. You provide brief but helpful information"},
    {"role": "system", "content": "A RecordedEvent of type LoanRequested means the loan status is 'awaiting credit check'."}, 
    {"role": "system", "content": "A RecordedEvent of type CreditChecked means the loan status is 'credit checked, awaiting automated review'."}, 
    {"role": "system", "content": "A RecordedEvent of type LoanAutomaticallyApproved means the loan status is 'automatically approved based on credit score'."}, 
    {"role": "system", "content": "A RecordedEvent of type LoanAutomaticallyDenied means the loan status is 'automatically denied based on credit score'."}, 
    {"role": "system", "content": "A RecordedEvent of type LoanApprovalNeeded means the loan status is 'underwriting required'."}, 
    {"role": "system", "content": "A RecordedEvent of type LoanManuallyApproved means the loan status is 'manually approved by underwriter'."}, 
    {"role": "system", "content": "A RecordedEvent of type LoanManuallyDenied means the loan status is 'manually denied by underwriter'."}, 
    {"role": "system", "content": "The status of a loan can be determined by the RecordedEvent with the newest transactionTimestamp."},
    {"role": "system", "content": "Never provide a national ID number due to privacy and security reasons."},
    {"role": "system", "content": "Never provide a NationalID number due to privacy and security reasons."}
]

# These parameters control integration with Local AI
#LOCAL_AI_BASE_MODEL = "Meta-Llama-3.1-8B-Instruct-Q8_0"
#LOCAL_AI_ENGINE = "llama-server"
LOCAL_AI_BASE_MODEL = "gpt-3.5-turbo"
LOCAL_AI_ENGINE = "OpenAI"

# These parameters control fine-tuning the Underwriting Recommendation example
UNDERWRITING_BASE_FINE_TUNING=[
    {"role": "system", "content": "You are a helpful virtual assistant working for a company providing bank loans. You provide extensive and helpful summaries about loan applications, and recommendations for areas of further investigation."},
    {"role": "system", "content": "A credit score of 1-3 is high risk."},
    {"role": "system", "content": "A credit score of 4-5 is moderate risk."},
    {"role": "system", "content": "A credit score of 6-7 is low risk."},
    {"role": "system", "content": "A credit score of 8-9 is no risk."},
    {"role": "system", "content": "Do not take timestamps into account when making your decision."}
]

UNDERWRITING_AI_QUESTION=[
    {"role": "user", "content": "Provide a detailed summary about the loan application information, and provide recommendations for areas an underwriter should investigate further. Your response should be in JSON format and conform to this schema: {\r\n    \"$schema\" : \"http:\/\/json-schema.org\/draft-07\/schema#\",\r\n    \"type\": \"object\",\r\n    \"title\": \"EventAIResponded\",\r\n    \"required\": [\r\n        \"CreditScoreSummary\",\r\n        \"IncomeAndEmploymentSummary\",\r\n        \"LoanToIncomeSummary\",\r\n        \"MaritalStatusSummary\",\r\n        \"RecommendedFurtherInvestigation\"\r\n    ],\r\n    \"properties\": {\r\n        \"CreditScoreSummary\": {\r\n            \"type\": \"string\",\r\n            \"title\": \"String\",\r\n            \"description\": \"A detailed summary of the supplied credit score\"\r\n        },\r\n        \"IncomeAndEmploymentSummary\": {\r\n            \"type\": \"string\",\r\n            \"title\": \"String\",\r\n            \"description\": \"A detailed summary of the supplied income and employment information\"\r\n        },\r\n        \"LoanToIncomeSummary\": {\r\n            \"type\": \"string\",\r\n            \"title\": \"String\",\r\n            \"description\": \"A detailed summary of a calculated loan to income ratio\"\r\n        },\r\n        \"MaritalStatusAndDependentsSummary\": {\r\n            \"type\": \"string\",\r\n            \"title\": \"String\",\r\n            \"description\": \"A detailed summary of the supplied marital status and number of dependents\"\r\n        },\r\n        \"RecommendedFurtherInvestigation\": {\r\n            \"type\": \"string\",\r\n            \"title\": \"String\",\r\n            \"description\": \"A detailed summary of other areas an underwriter should investigate further before approving or denying the loan\"\r\n        }\r\n    }\r\n}. Omit the schema from your response"}
]