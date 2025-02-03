options({
    $includeLinks:    true,
    reorderEvents:    false
})

fromCategory("loanRequest")
.when({
    "LoanRequested": function(s, e) {
        var country = e.data.Address.Country;
        linkTo("Loans-" + country, e, e.metadata);
    }
});