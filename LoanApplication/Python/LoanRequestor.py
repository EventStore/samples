# Import needed libraries
import wx
import wx.grid
from esdbclient import EventStoreDBClient, NewEvent, StreamState
import json
import config
import time
import traceback
import uuid

# Create some variables to hold state with the GUI
esdb = None
timer = None
grid = None
requests_dict = None

# Create a GUI panel to hold our 'application form'
class LoanRequestorPanel(wx.Frame):
    def __init__(self):
        # Print some information for the user
        print("\n\n***** Loan Request Command Processor *****\n")

        if config.DEBUG:
            print('Connecting to ESDB...')

        # Create a connection to ESDB
        while True:
            try:
                # Connect to ESDB using the configured URL
                self.esdb = EventStoreDBClient(uri=config.ESDB_URL)
                
                if config.DEBUG:
                    print('  Connection succeeded!')
                
                # If the connection was successful, break from the loop
                break
            # if the connection failed, try again in 10 seconds
            except:
                print('Connection to ESDB failed, retrying in 10 seconds...')
                traceback.print_exc()
                time.sleep(10)

        # Create the GUI window
        super().__init__(parent=None, title='Loan Requestor')

        # Add the panel to the window, and create a grid to hold the labels, inputs, and button
        panel = wx.Panel(self)        
        field_grid = wx.GridSizer(rows=5, cols=2, hgap=5, vgap=5)  

        # Create fields and labels for the User, NationalID, and Amount data
        self.field_user = wx.TextCtrl(panel)
        self.label_user = wx.StaticText(panel, wx.ID_ANY, 'User')
        self.field_nationalid = wx.TextCtrl(panel)
        self.label_nationalid = wx.StaticText(panel, wx.ID_ANY, 'NationalID')
        self.field_amount = wx.TextCtrl(panel)
        self.label_amount = wx.StaticText(panel, wx.ID_ANY, 'Amount')

        # Create a read-only field and label for the LoanRequestID data, which we'll set to a UUID
        self.field_loanrequestid = wx.TextCtrl(panel, style=wx.TE_READONLY)
        self.label_loanrequestid = wx.StaticText(panel, wx.ID_ANY, 'LoanRequestID')
        
        # Add all the fields and labels to the grid
        field_grid.Add(self.label_user, 0, wx.ALL | wx.EXPAND)
        field_grid.Add(self.field_user, 0, wx.ALL | wx.EXPAND)
        field_grid.Add(self.label_nationalid, 0, wx.ALL | wx.EXPAND)
        field_grid.Add(self.field_nationalid, 0, wx.ALL | wx.EXPAND)
        field_grid.Add(self.label_amount, 0, wx.ALL | wx.EXPAND)
        field_grid.Add(self.field_amount, 0, wx.ALL | wx.EXPAND)
        field_grid.Add(self.label_loanrequestid, 0, wx.ALL | wx.EXPAND)
        field_grid.Add(self.field_loanrequestid, 0, wx.ALL | wx.EXPAND)
        
        # Create a button that can be clicked to trigger submission of a loan application
        submit_button = wx.Button(panel, label='Submit')
        submit_button.Bind(wx.EVT_BUTTON, self.on_press)

        # Add the button to the grid
        field_grid.Add(submit_button, 0, wx.ALL | wx.CENTER)

        # Generate a UUID that will be used as the LoanRequestID
        self.field_loanrequestid.SetValue(str(uuid.uuid4()))

        # Size the panel to the size of the grid, and show it
        panel.SetSizer(field_grid)     
        self.SetPosition(wx.Point(100,100))   
        self.Show() 

    # When the Submit button is pressed
    def on_press(self, event):
        # Create some placeholder variables for the loan request data and stream name
        _loan_request_data = None
        _loan_request_stream_name = None

        # Get the input field data
        _user = self.field_user.GetValue()
        _nationalid = self.field_nationalid.GetValue()
        _amount = self.field_amount.GetValue()
        _loanrequestid = self.field_loanrequestid.GetValue()

        # Create a dictionary for the event data, and compose the stream name to which we will append it
        _loan_request_data = {"User": _user, "NationalID": _nationalid, "Amount": _amount, "LoanRequestID": _loanrequestid}
        _loan_request_stream_name = config.STREAM_PREFIX + '-' + _loanrequestid

        # Create a loan request event
        loan_request_event = NewEvent(type=config.EVENT_TYPE_LOAN_REQUESTED, data=bytes(json.dumps(_loan_request_data), 'utf-8'), id=_loanrequestid)
        
        print('Command Received - LoanRequested: ' + str(_loan_request_data) + '\n  Appending to stream: ' + _loan_request_stream_name)
        
        # Append the event to the stream
        CURRENT_POSITION = self.esdb.append_to_stream(stream_name=_loan_request_stream_name, current_version=StreamState.ANY, events=[loan_request_event])

        # Clear the input fields and set the LoanRequestID to a new UUID
        self.field_user.SetValue('')
        self.field_nationalid.SetValue('')
        self.field_amount.SetValue('')
        self.field_loanrequestid.SetValue(str(uuid.uuid4()))

# Create a GUI panel to show submitted and approved / denied applications
class LoanStatusPanel(wx.Frame):
    def __init__(self):
        # Print some information for the user
        print("\n\n***** Loan Status Processor *****\n")

        if config.DEBUG:
            print('Connecting to ESDB...')

        # Create a connection to ESDB
        while True:
            try:
                # Connect to ESDB using the configured URL
                self.esdb = EventStoreDBClient(uri=config.ESDB_URL)
                
                if config.DEBUG:
                    print('  Connection succeeded!')
                
                # If the connection was successful, break from the loop
                break
            # if the connection failed, try again in 10 seconds
            except:
                print('Connection to ESDB failed, retrying in 10 seconds...')
                traceback.print_exc()
                time.sleep(10)

        self.requests_dict = {}

        # Create the GUI window
        super().__init__(parent=None, title='Loan Request Status')

        # Add the panel to the window, and create a grid to hold the labels, inputs, and button
        panel = wx.Panel(self)

        self.grid = wx.grid.Grid(self)
        self.grid.CreateGrid(0,2)
        self.grid.SetColLabelValue(0, "LoanRequestID")
        self.grid.SetColLabelValue(1, "Status")
        self.grid.AutoSize()

        sizer = wx.BoxSizer(wx.VERTICAL)
        sizer.Add(self.grid, 1, wx.EXPAND)
        self.SetSizer(sizer)

        self.SetPosition(wx.Point(900,100))
        self.timer = wx.Timer(self)
        self.Bind(wx.EVT_TIMER, self.on_timerEvent, self.timer)
        self.timer.Start(1000)
        self.Fit()
        self.Show()

    def on_timerEvent(self, event):
        # Set up the name of the stream to which we will listen for events
        _loan_request_stream_name=config.STREAM_CE_PREFIX+config.STREAM_PREFIX

        # Create variables for the checkpoint file and checkpoint value
        _checkpoint = -1
        f = None

        # Try opening the checkpoint file, read the checkpoint value, convert it to a number, and close the file
        try:
            f = open(config.LOANREQUESTOR_CHECKPOINT_FILENAME)
            _checkpoint = int(f.read())
            f.close()

            if config.DEBUG:
                print('  Checkpoint: ' + str(_checkpoint))
        # If we couldn't open the file, start reading from the beginning of the stream
        except:
            print('No checkpoint file. Starting from the beginning of the subscription')
            traceback.print_exc()

        # Create a catchup subscription to the stream
        try:
            _stream = self.esdb.read_stream(stream_name=_loan_request_stream_name, resolve_links=True, stream_position=_checkpoint+1)
        # If we failed to create the catchup subscription, the checkpoint might be bad, so start from the beginning
        except:
            print('Bad checkpoint! Starting at the beginning of the stream...')
            _stream = self.esdb.read_stream(resolve_links=True)
            traceback.print_exc()

        if config.DEBUG:
            print('Reading events')

        # For each eent received
        for _event in _stream:
            # Print some debug information, unpacking the event JSON into a dictionary
            if config.DEBUG:
                print('  Received event: id=' + str(_event.id) + '; type=' + _event.type + '; stream_name=' + str(_event.stream_name) + '; data=\'' +  
                      str(json.loads(_event.data)) + '\'')

            # Capture the loan request ID from the stream name
            _loan_request_id = _event.stream_name.replace(config.STREAM_PREFIX + '-', '')
            # Track the status of that loan request ID by capturing the event type
            self.requests_dict.update({_loan_request_id: _event.type})

            # Open the checkpoint file and write the stream position of the event as our checkpoint
            # In practice, we'd likely do this every so often, not after every single event
            f = open(config.LOANREQUESTOR_CHECKPOINT_FILENAME, 'w')
            f.write(str(_event.link.stream_position))
            f.close()
            
            if config.DEBUG:
                print('  Checkpoint: ' + str(_event.link.stream_position) + '\n')
        
        if self.grid.NumberRows > 0:
            self.grid.DeleteRows(pos=0, numRows=self.grid.NumberRows)

        for key, value in self.requests_dict.items():
            self.grid.AppendRows(numRows=1)
            self.grid.SetCellValue(self.grid.NumberRows-1,0,key)
            self.grid.SetCellValue(self.grid.NumberRows-1,1,value)

            if value == config.EVENT_TYPE_CREDIT_CHECKED:
                self.grid.SetCellTextColour(self.grid.NumberRows-1,1,'CYAN')
            elif value in (config.EVENT_TYPE_LOAN_AUTO_APPROVED, config.EVENT_TYPE_LOAN_MANUAL_APPROVED):
                self.grid.SetCellTextColour(self.grid.NumberRows-1,1,'GREEN')
            elif value == config.EVENT_TYPE_LOAN_APPROVAL_NEEDED:
                self.grid.SetCellTextColour(self.grid.NumberRows-1,1,'YELLOW')
            elif value in (config.EVENT_TYPE_LOAN_AUTO_DENIED, config.EVENT_TYPE_LOAN_MANUAL_DENIED):
                self.grid.SetCellTextColour(self.grid.NumberRows-1,1,'RED')

        self.grid.AutoSize()
        self.Fit()
        self.timer.Start(500)

# Launch the GUI
app = wx.App()
frame = LoanRequestorPanel()
frame2 = LoanStatusPanel()
app.MainLoop()