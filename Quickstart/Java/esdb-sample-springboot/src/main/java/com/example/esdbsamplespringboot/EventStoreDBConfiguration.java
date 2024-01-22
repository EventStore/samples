package com.example.esdbsamplespringboot;

import com.eventstore.dbclient.*;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class EventStoreDBConfiguration {
    @Bean
    public EventStoreDBClient EventStoreDBClient() {
        return EventStoreDBClient.create(
                EventStoreDBConnectionString.parseOrThrow(
                        "esdb://admin:changeit@esdb-local:2113?tls=false&tlsVerifyCert=false"));
    }
}
