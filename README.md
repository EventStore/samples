# EventStoreDB samples

[EventStoreDB](https://www.eventstore.com/) is an industrial-strength database technology used as the central data store for event-sourced systems. It is available open-source to run locally on most platforms or as SaaS through [Event Store Cloud](https://www.eventstore.com/event-store-cloud).

This repository provides samples demonstrating the functionalities of [EventStoreDB](https://www.eventstore.com/) and the client SDKs of EventStore and practical usage.

## Contributing

Feel free to [create a GitHub issue](https://github.com/EventStore/samples/issues/new) if you have any questions or request for more explanation or samples.

We're open for any contribution! If you noticed some inconsistency, missing piece, or you'd like to extend existing samples - we'll be happy to [get your Pull Request](https://github.com/EventStore/samples/compare).

Read more in the [Contribution Guidelines](./CONTRIBUTING.md)

## Samples

Samples are organised by the specific topic. By going to the folder, you can find dedicated folders for different programming languages/environments.

### **[CQRS flow](./CQRS_Flow/)** 
- [.NET](./CQRS_Flow/.NET/)

**Description**:
- typical Event Sourcing with CQRS flow.
- stores events to EventStoreDB.
- shows how to organise the write model and read model handling.
- Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all).
- Read models are stored as ElasticSearch documents.
- Shows how to unit and integration test solution.

## Running samples locally

Check the `README.md` file in the specific sample folder for the detailed run instructions.

## Support

Information on commercial support can be found here: https://eventstore.com/support/.

Documentation can be found here: https://developers.eventstore.com/

We invite you to join our community discussion space at [Discuss](https://discuss.eventstore.com/).
