// basic connection to EventStore DB
using System.Text;

using EventStore.Client;

var settings = EventStoreClientSettings.Create("esdb://localhost:2113?tls=false");
settings.DefaultCredentials = new UserCredentials("admin", "changeit");
var client = new EventStoreClient(settings);
Console.WriteLine("EventStore DB Connection setup.");

var reader = client.ReadStreamAsync(
    direction: Direction.Forwards,
    streamName: "users",
    StreamPosition.Start
);

await foreach(var e in reader) {
    var metadata = Encoding.UTF8.GetString(e.Event.Metadata.ToArray());
    var data = Encoding.UTF8.GetString(e.Event.Data.ToArray());

    Console.WriteLine($"Event Position: {e.OriginalEventNumber}");
    Console.WriteLine($"Event Metadata: {metadata}");
    Console.WriteLine($"Event Data: {data}\n");
}

Console.WriteLine("Read completed.");
