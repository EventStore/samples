FROM gradle:7.6.3-jdk8 AS build
COPY --chown=gradle:gradle . /home/gradle/src
WORKDIR /home/gradle/src
RUN gradle build --no-daemon 

FROM openjdk:8-jre-slim
RUN mkdir /app
COPY --from=build /home/gradle/src/build/libs/*.jar /app/esdb-sample-springboot.jar
EXPOSE 8080
ENTRYPOINT ["java", "-jar", "/app/esdb-sample-springboot.jar"]