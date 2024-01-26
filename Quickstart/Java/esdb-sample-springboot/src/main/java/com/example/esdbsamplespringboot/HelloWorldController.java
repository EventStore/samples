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
import java.util.ArrayList;
import java.util.List;

@Controller
public class HelloWorldController {
    private static final String VISITORS_STREAM = "visitors-stream";
    private static final Gson gson = new Gson();
    private final EventStoreDBClient eventStore;

    @Autowired
    public HelloWorldController(EventStoreDBClient eventStore) {
        this.eventStore = eventStore;
    }

    @ResponseBody
	@GetMapping("/hello-world")
	public ResponseEntity<?> sayHello(@RequestParam(name="visitor", required=false, defaultValue="Visitor") String visitor) {
        try {
            VisitorGreeted visitorGreeted = new VisitorGreeted(visitor);

            EventData event = EventData
                    .builderAsJson("VisitorGreeted", visitorGreeted)
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
                VisitorGreeted vg = gson.fromJson(
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
