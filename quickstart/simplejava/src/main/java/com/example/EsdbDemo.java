package com.example;

import com.eventstore.dbclient.*;
import java.util.UUID;
import java.util.concurrent.ExecutionException;


public class EsdbDemo {


    public static void main(String[] args) throws ConnectionStringParsingException, ExecutionException, InterruptedException, java.io.IOException {

        // init esdb client
        String connectionString = "esdb://127.0.0.1:2113?tls=false"; // 0.0.0.0:2113&defaultdeadline=60000 127.0.0.1:2113 localhost:2113
        EventStoreDBClientSettings settings = null;
        EventStoreDBClient client = null;
        try {
                settings = EventStoreDBConnectionString.parse(connectionString);
                client = EventStoreDBClient.create(settings);
        } catch (Throwable e) {
                throw new RuntimeException(e);
        }        
        
        // set up sample event
        AccountCreated createdEvent = new AccountCreated();
        createdEvent.setId(UUID.randomUUID());
        createdEvent.setLogin("ouros"); 
        EventData event = EventData
                .builderAsJson("account-created", createdEvent)
                .build();
        
        // append sample event to stream
        try {
                /*WriteResult writeResult = client
                        .appendToStream("accounts", event)
                        .get();
                */
                client.appendToStream("accounts", event)
                .get();

//                AppendToStreamOptions appendToStreamOptions = AppendToStreamOptions.get()
//                        .authenticated("admin", "changeit");
//
//                client.appendToStream("accounts", appendToStreamOptions, event)
//                        .get();

            System.out.println("Append completed.");
        } catch (Exception e) {
                System.out.println("Caught exception: " + e);
        }
/*
        ReadStreamOptions readStreamOptions = ReadStreamOptions.get()
                .forwards()
                .fromStart()
                .maxCount(10);

        ReadResult readResult = client.readStream("accounts", readStreamOptions)
                .get();

        ResolvedEvent resolvedEvent = readResult
                .getEvents()
                .get(0);

        AccountCreated writtenEvent = resolvedEvent.getOriginalEvent()
                .getEventDataAs(AccountCreated.class);

*/

    }

}
