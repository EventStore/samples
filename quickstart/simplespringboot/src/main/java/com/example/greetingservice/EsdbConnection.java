package com.example.greetingservice;

import org.springframework.stereotype.Component;
import com.eventstore.dbclient.*;

@Component
public class EsdbConnection {

	// Set up the connection to EventStoreDB as a singleton
    public EventStoreDBClient initEsClient() {
            String connectionString = "esdb://127.0.0.1:2113?tls=false"; // 60000&keepAliveTimeout=10000&keepAliveInterval=10000
            EventStoreDBClientSettings settings = EventStoreDBConnectionString.parseOrThrow(connectionString);
            EventStoreDBClient client = EventStoreDBClient.create(settings);  
            return client;              
    }
}



