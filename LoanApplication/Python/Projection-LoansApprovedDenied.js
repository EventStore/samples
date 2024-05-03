options({
    $includeLinks:    true,
    reorderEvents:    false
})

fromStreams(["$ce-loanRequest"])
.when({
    "LoanAutomaticallyApproved": function(s, e) {
        linkTo("ApprovedLoans", e, e.metadata);
    },
    "LoanManuallyApproved": function(s, e) {
        linkTo("ApprovedLoans", e, e.metadata);
    },
    "LoanAutomaticallyDenied": function(s, e) {
        linkTo("DeniedLoans", e, e.metadata);
    },
    "LoanManuallyDenied": function(s, e) {
        linkTo("DeniedLoans", e, e.metadata);
    }
});