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

# Enable DEBUG mode to see debug messages printed onto the console
DEBUG = True

# Connection URL for ESDB
ESDB_URL = "esdb://localhost:2113?Tls=false"

# Name of checkpoint files, which are written into the working directory
UNDERWRITING_CHECKPOINT_FILENAME = 'underwriting.checkpoint'
LOANDECIDER_CHECKPOINT_FILENAME = 'loandecider.checkpoint'
LOANREQUESTOR_CHECKPOINT_FILENAME = 'loanrequestor.checkpoint'