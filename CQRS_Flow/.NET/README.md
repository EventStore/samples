![Github Actions](https://github.com/EventStore/samples/actions/workflows/build.cqrs_flow.dotnet.yml/badge.svg?branch=main)

# CQRS flow of Event Sourcing with EvenStoreDB

Sample is showing the typical flow of the Event Sourcing with [EventStoreDB](https://developers.eventstore.com) using CQRS. It uses E-Commerce shopping carts domain.

## Prerequisities

1. Install git - https://git-scm.com/downloads.
2. Install .NET Core 5.0 - https://dotnet.microsoft.com/download/dotnet/5.0.
3. Install Visual Studio 2019, Rider or VSCode.
4. Install docker - https://docs.docker.com/docker-for-windows/install/.
5. Open `ECommerce.sln` solution.

## Running

1. Run: `docker-compose up`.
2. Wait until all dockers got are downloaded and running.
3. You should automatically get:
    - ElasticSearch running at http://localhost:9200
    - Kibana - UI for ElasticSearch . Available at: http://localhost:5601
    - EventStoreDB UI: http://localhost:2113/
4. Open, build and run `ECommerce.sln` solution.
	- Swagger should be available at: http://localhost:5000/index.html


## Overview

It uses:
- Provides the example of the [Aggregate](./Carts/Carts/Carts/Cart.cs),
- Stores events to EventStoreDB,
- Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all).
- Read models are stored as [ElasticSearch](https://www.elastic.co/elasticsearch/) documents.
- CQRS with MediatR,
- App has Swagger and predefined [docker-compose](./docker-compose.yml) to run and play with samples.

## Write Model

- Provides the basic boilerplate together with Core projects,
- [EventStoreDBRepository](./Core/Core.EventStoreDB/Repository/EventStoreDBRepository.cs) repository to load and store aggregate state,
- [IProjection](./Core/Core/Projections/IProjection.cs) interface to handle the same way stream aggregation and materialised projections,
- Thanks to that added dedicated [AggregateStream](./Core/Core.EventStoreDB/Events/AggregateStreamExtensions.cs#L12) method for stream aggregation
- See [sample Aggregate](./Carts/Carts/Carts/Cart.cs) and [base class](./Core/Core/Aggregates/Aggregate.cs)

## Read Model
- Read models are rebuilt with eventual consistency using subscribe to all EventStoreDB feature,
- Added hosted service [SubscribeToAllBackgroundWorker](./Core/Core.EventStoreDB/Subscriptions/SubscribeToAllBackgroundWorker.cs) to handle subscribing to all. It handles checkpointing and simple retries if the connection was dropped.
- Added [ISubscriptionCheckpointRepository](./Core/Core.EventStoreDB/Subscriptions/ISubscriptionCheckpointRepository.cs) for handling Subscription checkpointing.
- Added checkpointing to EventStoreDB stream with [EventStoreDBSubscriptionCheckpointRepository](./Core/Core.EventStoreDB/Subscriptions/EventStoreDBSubscriptionCheckpointRepository.cs) and dummy in-memory checkpointer [InMemorySubscriptionCheckpointRepository](./Core/Core.EventStoreDB/Subscriptions/InMemorySubscriptionCheckpointRepository.cs),
- Added [ElasticSearchProjection](./Core/Core.ElasticSearch/Projections/ElasticSearchProjection.cs) as a sample how to project with [`left-fold`](https://en.wikipedia.org/wiki/Fold_(higher-order_function)) into external storage. Another (e.g. ElasticSearch, EntityFramework) can be implemented the same way.

## Tests
- Added sample of unit testing in [`Carts.Tests`](./Carts/Carts.Tests):
  - [Aggregate unit tests](./Carts/Carts.Tests/Carts/InitializingCart/InitializeCartTests.cs)
  - [Command handler unit tests](./Carts/Carts.Tests/Carts/InitializingCart/InitializeCartCommandHandlerTests.cs)
- Added sample of integration testing in [`Carts.Api.Tests`](./Carts/Carts.Api.Tests)
  - [API acceptance tests](./Carts/Carts.Api.Tests/Carts/InitializingCart/InitializeCartTests.cs)

## Other
- [EventTypeMapper](./Core/Core/Events/EventTypeMapper.cs) class to allow both convention-based mapping (by the .NET type name) and custom to handle event versioning,
- [StreamNameMapper](./Core/Core/Events/StreamNameMapper.cs) class for convention-based id (and optional tenant) mapping based on the stream type and module,
- IoC [registration helpers for EventStoreDB configuration](./Core/Core.EventStoreDB/Config.cs),


## Trivia

1. Docker useful commands
    - `docker-compose up` - start dockers
    - `docker-compose kill` - to stop running dockers.
    - `docker-compose down -v` - to clean stopped dockers.
    - `docker ps` - for showing running dockers
    - `docker ps -a` - to show all dockers (also stopped)

