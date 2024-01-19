import json
from waitress import serve
from flask import Flask, request
from dataclasses import dataclass
from esdbclient import EventStoreDBClient, NewEvent, StreamState

app = Flask(__name__)

event_store = EventStoreDBClient(
    uri='esdb://admin:changeit@esdb_local:2113?tls=false&tlsVerifyCert=false'
)

visitors_stream = 'visitors-stream'

@app.route('/')
def hello_world():
    @dataclass
    class VisitorGreeted:
        visitor: str
        
    visitor = request.args.get('visitor', 'Visitor')
    visitor_greeted = VisitorGreeted(visitor=visitor)

    event_data = NewEvent(
        type='VisitorGreeted',
        data=json.dumps(visitor_greeted.__dict__).encode('utf-8')
    )

    append_result = event_store.append_to_stream(
        stream_name=visitors_stream,
        current_version=StreamState.ANY,
        events=[event_data],
    )

    event_stream = event_store.get_stream(
        stream_name=visitors_stream,
        stream_position=0,
    )

    visitors_greeted = []
    for event in event_stream:
        visitors_greeted.append(VisitorGreeted(**json.loads(event.data)).visitor)

    return f"{len(visitors_greeted)} visitors have been greeted, they are: [{','.join(visitors_greeted)}]"

serve(app, host='0.0.0.0', port=8080)