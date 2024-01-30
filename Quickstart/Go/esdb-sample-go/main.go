package main

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"math"
	"net/http"
	"strings"

	"github.com/EventStore/EventStore-Client-Go/v3/esdb"
	"github.com/gin-gonic/gin"
)

func main() {
	router := gin.Default()

	settings, err := esdb.ParseConnectionString("esdb://admin:changeit@esdb-local:2113?tls=false&tlsVerifyCert=false")
	if err != nil {
		panic(err)
	}

	eventStore, err := esdb.NewClient(settings)
	if err != nil {
		panic(err)
	}

	const visitorsStream string = "visitors-stream"

	router.GET("/hello-world", func(c *gin.Context) {
		visitor := c.Query("visitor")
		if visitor == "" {
			visitor = "Visitor"
		}

		visitorGreetedJson, err := json.Marshal(&VisitorGreeted{Visitor: visitor})
		if err != nil {
			panic(err)
		}

		eventData := esdb.EventData{
			ContentType: esdb.ContentTypeJson,
			EventType:   "VisitorGreeted",
			Data:        visitorGreetedJson,
		}

		_, err = eventStore.AppendToStream(context.Background(), visitorsStream, esdb.AppendToStreamOptions{ExpectedRevision: esdb.Any{}}, eventData)
		if err != nil {
			panic(err)
		}

		eventStream, err := eventStore.ReadStream(context.Background(), visitorsStream, esdb.ReadStreamOptions{Direction: esdb.Forwards}, math.MaxUint64)
		if err != nil {
			panic(err)
		}

		defer eventStream.Close()

		var visitorsGreeted []string
		for {
			event, err := eventStream.Recv()
			if errors.Is(err, io.EOF) {
				break
			}

			if err != nil {
				panic(err)
			}

			var visitorGreeted VisitorGreeted
			err = json.Unmarshal(event.Event.Data, &visitorGreeted)
			if err != nil {
				panic(err)
			}

			visitorsGreeted = append(visitorsGreeted, visitorGreeted.Visitor)
		}
		c.String(http.StatusOK, fmt.Sprintf("%d visitors have been greeted, they are: [%s]", len(visitorsGreeted), strings.Join(visitorsGreeted, ",")))
	})

	router.Run(fmt.Sprintf(":%d", 8080))
}

type VisitorGreeted struct {
	Visitor string `json:"Visitor"`
}
