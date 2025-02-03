# Loans Application - Python Sample Code

Welcome to the Loans Application sample repo. This repo contains the python implementation of the Loans Project

## To get started

* Install python version 3.11 or greater
* Install the required packages:

```
pip install esdbclient
pip install wxPython
```

If the above fails, try:

```
pip3 install esdbclient
pip3 install wxPython
```

* Either clone the repository or download the code ZIP file
* Install VSCode: https://code.visualstudio.com/download
* Install Docker: https://www.docker.com/products/docker-desktop/
* Start the Docker image for your platform: https://www.eventstore.com/downloads/23.10
* In your web browser, navigate to http://localhost:2113/web/index.html#/projections
* Click the `Enable All` button to enable the system projections

That's it! You're ready to go!

## Basic usage

To start the project and see events move through the system, open four terminal windows. Run one of the following commands in each window:

```
python Underwriting.py
python LoanDecider.py
python CreditCheck.py
python LoanRequestor-testCases.py
```

If the above commands fail, try:

```
python3 Underwriting.py
python3 LoanDecider.py
python3 CreditCheck.py
python3 LoanRequestor-testCases.py
```

The last command, `LoanRequestor-testCases.py`, will inject some test loan requests into the system. You can instead use the command-line injector to create and append loan requests:

```
python LoanRequestor-commandLine.py
```

Or:

```
python3 LoanRequestor-commandLine.py
```

Check out our QuickStart video series on YouTube to see how everything works