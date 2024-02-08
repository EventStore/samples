package com.example.esdbsamplespringboot;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseBody;
import com.eventstore.dbclient.*;
import com.google.gson.Gson;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;

@Controller
public class HelloWorldController {
    private static final String VISITORS_STREAM = "visitors-stream";
    private final EventStoreDBClient eventStore;
    private final Gson gsonMapper;

    @Autowired
    public HelloWorldController(EventStoreDBClient eventStore, Gson gsonMapper) {
        this.eventStore = eventStore;
        this.gsonMapper = gsonMapper;
    }

    @ResponseBody
    @GetMapping("/hello-world")
    public ResponseEntity<?> sayHello(@RequestParam(name = "visitor", required = false, defaultValue = "Visitor") String visitor) {
        try {
            VisitorGreeted visitorGreeted = new VisitorGreeted(visitor);

            byte[] vgBytes = gsonMapper.toJson(visitorGreeted).getBytes();
            EventData event = EventData
                    .builderAsJson("VisitorGreeted", vgBytes)
                    .build();

            WriteResult writeResult = eventStore
                    .appendToStream(VISITORS_STREAM, event)
                    .get();

            ReadResult eventStream = eventStore
                    .readStream(
                            VISITORS_STREAM,
                            ReadStreamOptions.get().fromStart())
                    .get();

            List<String> visitorsGreeted = new ArrayList<>();
            for (ResolvedEvent re : eventStream.getEvents()) {
                VisitorGreeted vg = gsonMapper.fromJson(
                        new String(re.getOriginalEvent().getEventData()),
                        VisitorGreeted.class);

                visitorsGreeted.add(vg.getVisitor());
            }

            String res = String.format(
                    "%d visitors have been greeted, they are: [%s]",
                    visitorsGreeted.size(),
                    String.join(",", visitorsGreeted));

            return ResponseEntity.ok(res);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(e.getMessage());
        }
    }

    static class VisitorGreeted {
        private String visitor;

        public VisitorGreeted(String visitor) {
            setVisitor(visitor);
        }

        public String getVisitor() {
            return visitor;
        }

        public void setVisitor(String visitor) {
            this.visitor = visitor;
        }
    }
}
