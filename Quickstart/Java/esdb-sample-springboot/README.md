# Accessing Data with EventStore 

This guide walks you through creating a "`Hello, world!`" app with Spring
Boot (with actuator) and EventStore. The service accepts the following HTTP GET request:

``` [source,sh]
$ curl http://localhost:9000/hello-world
```


It responds with the following JSON:

``` [source,json]
{
"id": "bb657c09-6603-4b7a-b417-14ae2824744a",
"content": "Hello, Visitor!"
}
```

## What You Need

* About 15 minutes

* A favorite text editor or IDE

* [Java 1.8 ](https://openjdk.org/projects/jdk8/)

* [Gradle 8.0+](https://gradle.org/install/) or [Maven 3.5+](https://maven.apache.org/download.cgi)


## Build and run

### Start the EventStore server

Pull a docker image from: https://hub.docker.com/r/eventstore/eventstore/

Run EventStore in Docker with: 
``` [source,sh]
docker run --name esdb-node -it -p 2113:2113 -p 1113:1113 \
    eventstore/eventstore:latest --insecure  --enable-atom-pub-over-http --runprojections=all
```

Verify that the database is up and running by going to the EventStore dashboard at: http://localhost:2113/


### Build and run the sample app

For Maven, build the JAR file with `./mvnw clean package` and then run with:
``` [source,sh]
java -jar target/esdb-sample-springboot.0.0.1.jar
```

For Gradle, build the JAR file with `./gradlew clean build` and then run with:
``` [source,sh]
java -jar build/libs/esdb-sample-springboot.0.0.1.jar
```

## Test the app manually

For the visitorGreeted service default, curl or point a web browser to:
```
http://localhost:9000/hello-world
```

``` [source,bash]
$ curl http://localhost:9000/hello-world
{"id":"c615db58-8f5d-427e-8be6-39e849b68a5e","content":"Hello, Visitor!"}
```


To greet Nefertiti, curl or point a web browser to:
```
http://localhost:9000/hello-world?name=Nefertiti
```

``` [source,bash]
$ curl "http://localhost:9000/hello-world?name=Nefertiti"
{"id":"074979e1-5aa3-47ce-8dc3-e898e748a067","content":"Hello, Nefertiti!"}
```

For the visitorGreeted service heartbeat, curl or point a web browser to:
```
http://localhost:9001/actuator/health
```

``` [source,bash]
$ curl http://localhost:9001/actuator/health
{"status":"UP"}
```

