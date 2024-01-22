using System.Text.Json;
using EventStore.Client;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the EventStoreClient as a Singleton
builder.Services.AddSingleton(
    new EventStoreClient(EventStoreClientSettings.Create(
            "esdb://admin:changeit@esdblocal:2113?tls=false&tlsVerifyCert=false")));

var app = builder.Build();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

const string visitorsStream = "visitors-stream";

app.MapGet("/hello-world", async (
        [FromQuery] [DefaultValue("Visitor")] string visitor, 
        [FromServices] EventStoreClient eventStore,
        CancellationToken cancellationToken) =>
    {
        var visitorGreeted = new VisitorGreeted(visitor);

        var eventData = new EventData(
            Uuid.NewUuid(),
            nameof(VisitorGreeted),
            JsonSerializer.SerializeToUtf8Bytes(visitorGreeted));

        await eventStore.AppendToStreamAsync(
            visitorsStream,
            StreamState.Any,
            new[] { eventData },
            cancellationToken: cancellationToken);

        var readStreamResult = eventStore.ReadStreamAsync(
            Direction.Forwards, 
            visitorsStream, 
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        var eventStream = await readStreamResult.ToListAsync(cancellationToken);

        var visitorsGreeted = eventStream
            .Select(re => JsonSerializer.Deserialize<VisitorGreeted>(re.Event.Data.ToArray()))
            .Select(vg => vg!.Visitor)
            .ToArray();
        
        return Results.Ok($"{visitorsGreeted.Length} visitors have been greeted, they are: [{string.Join(',', visitorsGreeted)}]");
    })
    .WithName("HelloWorld")
    .WithOpenApi();

app.Run();

internal record VisitorGreeted(string Visitor);