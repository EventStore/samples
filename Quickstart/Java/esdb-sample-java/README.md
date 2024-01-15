# Simple example of Java application using the EventStore client

This sample plain java example demonstrates how to use the EventStore client.

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
java -jar target/java-sample-0.0.1-allinone.jar
```

For Gradle, build the JAR file with `./gradlew clean build` and then run with:
``` [source,sh]
java -jar build/libs/java-sample-0.0.1-all.0.0.1.jar
```

:warning: When packaging your application as an executable jar, if you are using the Maven shade plugin or the Gradle shadow plugin, configure it to merge the descriptor file.

Maven and Gradle examples are included here in the `pom.xml` and `build.gradle` files.

For Maven add: 
```
<transformer implementation="org.apache.maven.plugins.shade.resource.ServicesResourceTransformer"/>
```

For Gradle, in the `shadowJar` block, add:
```
mergeServiceFiles()
```


