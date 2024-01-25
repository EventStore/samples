# Python Sample: Hello, World!

This guide shows you how to stand up a sample Hello World application, built with [Flask](https://flask.palletsprojects.com/en/3.0.x/), that connects to EventStoreDB.

The sample exposes a simple HTTP endpoint `http://localhost:8080/hello-world?visitor={visitor}` that shows how to append to and read from a stream in EventStoreDB.

When a visitor says hello via `hello-world?visitor={visitor}`, an event is appended to a stream to record the fact they have been greeted.

The stream is then read from beginning to end to return the full log of visitors on each call to `/hello-world?visitor={visitor}`.

You can see how this is done in the source code [here](./main.py).

## Prerequisites

Before running the application, make sure you have the following installed on your system:

- Docker: [Get Docker](https://docs.docker.com/get-docker/)
- Docker Compose: [Install Docker Compose](https://docs.docker.com/compose/install/)

## Running The Sample

1. Clone the repository:

   ```
   git clone https://github.com/EventStore/samples.git
   cd samples/Quickstart/Python/esdb-sample-python
   ```

2. Run the application and database using Docker Compose:

    ```
    docker compose up -d
    ```

3. Verify the containers are up and running:

    ```
    docker compose ps
    ```

    Output:
    ```
    NAME                                      IMAGE                          ...   STATUS                   PORTS
    esdb-sample-python-esdb-local-1           eventstore/eventstore:latest   ...   Up 7 seconds (healthy)   1112-1113/tcp, 0.0.0.0:2113->2113/tcp
    esdb-sample-python-esdb-sample-python-1   esdb-sample-python             ...   Up 7 seconds             0.0.0.0:8080->8080/tcp
    ```

4. Test the application:

    Say hello as `Ouro`:
    ```
    curl localhost:8080/hello-world?visitor=Ouro
    ```

    Say hello as `YourName`:
    ```
    curl localhost:8080/hello-world?visitor=YourName
    ```

    Output:
    ```
    1 visitors have been greeted, they are: [Ouro]
    ```
    ```
    2 visitors have been greeted, they are: [Ouro, YourName]
    ```

5. To stop and remove the containers, use:

    ```
    docker compose down
    ```

## Additional Information

For more in-depth and detailed examples of using EventStoreDB and the Python Client, refer to:
- EventStoreDB: [Getting Started With EventStoreDB](https://developers.eventstore.com/clients/grpc/)
- Python Client: [Python Client Samples](https://github.com/pyeventsourcing/esdbclient/tree/1.0)