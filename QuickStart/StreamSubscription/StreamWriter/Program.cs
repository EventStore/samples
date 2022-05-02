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

int index = 0;

do {
    // adds a NewUser event to the users stream
    var user = new {
        Type = "NewUser",
        Metadata = new{
            Date = DateTime.Now
        },
        Body = new {
            Id = Guid.NewGuid(),
            FirstName = $"First Name #{index}",
            LastName = $"Washington {index}"
        }
    };

    var userEventData = new EventData(Uuid.NewUuid(), user.Type, JsonSerializer.SerializeToUtf8Bytes(user.Body), JsonSerializer.SerializeToUtf8Bytes(user.Metadata));

    await client.AppendToStreamAsync(
        streamName: "users",
        expectedState: StreamState.Any,
        eventData: new[] { userEventData });
    Console.WriteLine("New User added.");


    // adds a ClockedIn event into the timeclock stream.
    var timeClock = new {
        Type = "ClockedIn",
        Metadata = new {
            Date = DateTime.Now
        },
        Body = new {
            Id = Guid.NewGuid(),
            UserId = user.Body.Id,
            TimeIn = DateTime.Now
        }
    };

    var timeClockEventData = new EventData(Uuid.NewUuid(), timeClock.Type, JsonSerializer.SerializeToUtf8Bytes(timeClock.Body), JsonSerializer.SerializeToUtf8Bytes(timeClock.Metadata));

    await client.AppendToStreamAsync(
        streamName: "timeclock",
        expectedState: StreamState.Any,
        new[] { timeClockEventData }
    );
    Console.WriteLine("User just clocked-in.");

    await Task.Delay(250);
} while(true);