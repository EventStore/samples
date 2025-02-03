#!/bin/bash

curl -X POST 'https://localhost:2113/projections/continuous?name=LoansApprovedDenied&emit=yes&checkpoints=yes&enabled=yes&trackemittedstreams=yes' \
     -v \
     -k \
     -i \
     -u "admin:changeit" \
     -H "Content-Type: application/json" \
     --data-binary "@Projection-LoansApprovedDenied.js"

curl -X POST 'https://localhost:2113/projections/continuous?name=LoansByCountryName&emit=yes&checkpoints=yes&enabled=yes&trackemittedstreams=yes' \
     -v \
     -k \
     -i \
     -u "admin:changeit" \
     -H "Content-Type: application/json" \
     --data-binary "@Projection-LoansByCountryName.js"
