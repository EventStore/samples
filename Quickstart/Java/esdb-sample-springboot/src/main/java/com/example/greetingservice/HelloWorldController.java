package com.example.greetingservice;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseBody;

import com.fasterxml.jackson.databind.ObjectMapper;

import java.util.UUID;
import com.eventstore.dbclient.*;

@Controller
public class HelloWorldController {

	@GetMapping("/hello-world")
	@ResponseBody
	public Greeting sayHello(@RequestParam(name="name", required=false, defaultValue="Visitor") String name) {
                       
                EventStoreDBClient client = null;

                // create event data from incoming greeting
                String template = "Hello, %s!";
                String greeting = String.format(template, name);
                Greeting incomingGreeting = new Greeting();
                incomingGreeting.setId(UUID.randomUUID());
                incomingGreeting.setContent(greeting);  
                EventData event = setEventData(incomingGreeting, "Greeting Event");              

        	// Connect to the database and record the greeting
                if (client == null) {
                        client = initClient();
                }

                try {
                	setGreetingEvent(client, "Greetings", event);
                	getGreetingEvent(client, "Greetings", event);
                } catch (Exception e) {
                	incomingGreeting.setContent("Failed to connect to ESDB");
                } finally {       	
                	return incomingGreeting;
                }

	}

        public EventStoreDBClient initClient() {
                EsdbConnection dbConn = new EsdbConnection();
                return dbConn.initEsClient();             
        }

        public EventData setEventData(Greeting greeting, String eventType) {
                EventData event = EventData
                        .builderAsJson(eventType, greeting)
                        .build();
                return event;
        }      

        public WriteResult setGreetingEvent(EventStoreDBClient client, String streamName, EventData event) throws Throwable {
                WriteResult writeResult = client
                        .appendToStream(streamName, event)
                        .get();
                return writeResult;
        }

        public Greeting getGreetingEvent(EventStoreDBClient client, String streamName, EventData event) throws Throwable {    
                ReadStreamOptions readStreamOptions = ReadStreamOptions.get()
                        .fromStart()
                        .notResolveLinkTos();

                ReadResult readResult = client
                        .readStream(streamName, readStreamOptions)
                        .get();

                ResolvedEvent resolvedEvent = readResult
                        .getEvents()
                        .get(0);

                Greeting writtenEvent = resolvedEvent.getOriginalEvent()
                        .getEventDataAs(Greeting.class);

                // RecordedEvent recordedEvent = resolvedEvent.getOriginalEvent();

                return writtenEvent;             

        }

}
