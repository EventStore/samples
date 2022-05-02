using System.Text.Json;

using EventStore.Client;

// data elements of two events to be stored within EventStore DB
var events = new[]{
    new {
        Type = "NewUser",
        Metadata = new{
            Date = DateTime.Now
        },
        Body = new {
            Id = Guid.NewGuid(),
            FirstName = "George",
            LastName = "Washington"
        }
    },
    new {
        Type = "NewUser",
        Metadata = new{
            Date = DateTime.Now
        },
        Body = new {
            Id = Guid.NewGuid(),
            FirstName = "Samuel",
            LastName = "Adams"
        }
    }
};

// basic connection to EventStore DB
var settings = EventStoreClientSettings.Create("esdb://localhost:2113?tls=false");
settings.DefaultCredentials = new UserCredentials("admin", "changeit");
var client = new EventStoreClient(settings);
Console.WriteLine("EventStore DB Connection setup.");

// Serialization of the above data.
var serialized = events.Select(e => new EventData(Uuid.NewUuid(), e.Type, JsonSerializer.SerializeToUtf8Bytes(e.Body), JsonSerializer.SerializeToUtf8Bytes(e.Metadata))).ToArray();
Console.WriteLine("Events are serialized and ready for storage.");

// storing the serialized events within EventStore DB
await client.AppendToStreamAsync(
    streamName: "users", 
    expectedRevision: StreamRevision.None, 
    eventData: serialized);
Console.WriteLine("Events have been stored.");
