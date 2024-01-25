# EventStoreDB samples

[EventStoreDB](https://www.eventstore.com/) is an industrial-strength database technology used for event-sourcing. It is open source and runs on most platforms or as SaaS in [Event Store Cloud](https://www.eventstore.com/event-store-cloud).

This repository provides practical samples that demonstrate features of [EventStoreDB](https://www.eventstore.com/) and its client SDKs.

Common operations and samples are in the [client repositories](https://github.com/EventStore?q=EventStore+Client)

## Contributing

Feel free to [create a GitHub issue](https://github.com/EventStore/samples/issues/new) if you have any questions or request for more explanation or samples.

We're open to any contribution! If you noticed some inconsistency, missing piece, or you'd like to extend existing samples - we're happy to [get your Pull Request](https://github.com/EventStore/samples/compare).

Read more in the [Contribution Guidelines](./CONTRIBUTING.md)

## Samples

Samples are organized by topics in dedicated directories by programming languages/environments.

### Quickstart

Quickstart guides have been created that show you how to stand up a sample Hello World application that appends to and reads from a stream in EventStoreDB.

Find the sample for your preferred client language below:

- .NET: [ASP.NET Core sample](/Quickstart/.NET/esdb-sample-dotnet)
- Go: [Gin sample](/Quickstart/Go/esdb-sample-go)
- Java: [Spring Boot sample](Quickstart/Java/esdb-sample-springboot)
- Node.js: [Express.js sample](/Quickstart/Nodejs/esdb-sample-nodejs)
- Python: [Flask sample](/Quickstart/Python/esdb-sample-python)
- Rust: [Rocket sample](/Quickstart/Rust/esdb-sample-rust)

### **[CQRS flow](./CQRS_Flow/)** 
- [.NET](./CQRS_Flow/.NET/)

  **Description**:
  - Demonstrates typical event sourcing with CQRS flow
  - Stores events in EventStoreDB
  - Shows how to handle the write model and read model
  - Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all)
  - Shows how to store read models as ElasticSearch documents
  - Shows how to write unit and integration tests

- [Java](./CQRS_Flow/Java/)

  **Description**:
  - Demonstrates typical event sourcing with CQRS flow
  - Stores events in EventStoreDB
  - Shows how to handle the write model and read model
  - Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all)
  - Shows how to store read models as Postgres documents
  - Shows how to write unit and integration tests

  The examples show 2 variations of handling business logic:
  - [Aggregate pattern](./CQRS_Flow/Java/event-sourcing-esdb-aggregates)
  - [Command handlers as pure functions](./CQRS_Flow/Java/event-sourcing-esdb-simple)


### **[Crypto Shredding](./Crypto_Shredding/)** 
- [.NET](./Crypto_Shredding/.NET/)

  **Description**:
  - Shows how to protect sensitive data (e.g. for [European General Data Protection Regulation](https://en.wikipedia.org/wiki/General_Data_Protection_Regulation)) in Event-Sourced Systems.
  - Shows how to use the .NET `System.Security.Cryptography` library with [AES](https://en.wikipedia.org/wiki/Advanced_Encryption_Standard) algorithm to encrypt and decrypt events' data.
  - Stores events in EventStoreDB

### **[Sending EventStoreDB logs to Elasticsearch](./Logging/Elastic/)**

**Description**

These samples show how to configure various ways of sending logs from EventStoreDB to Elasticsearch:
- [Logstash](./Logging/Elastic/Logstash/),
- [Filebeat](./Logging/Elastic/Filebeat/),
- [FilebeatWithLogstash](./Logging/Elastic/FilebeatWithLogstash/)

## Running samples locally

Check the `README.md` file in the specific sample folder for detailed instructions.

## Support

Information on EventStoreDB support: https://eventstore.com/support/.

EventStoreDB Documentation: https://developers.eventstore.com/



