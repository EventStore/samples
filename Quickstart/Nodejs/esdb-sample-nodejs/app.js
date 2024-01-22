const express = require('express')
const app = express()
const port = 8080

const {
    EventStoreDBClient,
    jsonEvent,
    FORWARDS,
    START,
} = require("@eventstore/db-client");

const eventStore = new EventStoreDBClient(
    { endpoint: 'esdb-local:2113' }, 
    { insecure: true }
)

const visitorsStream = 'visitors-stream'

app.get('/hello-world', async (req, res) => {
    const visitor = req.query.visitor ?? 'Visitor'

    const event = jsonEvent({
        type: 'VisitorGreeted',
        data: {
            visitor,
        },
    })

    await eventStore.appendToStream(visitorsStream, [event])

    const eventStream = eventStore.readStream(visitorsStream, {
        fromRevision: START,
        direction: FORWARDS,
    })

    let visitorsGreeted = []
    for await (const { event } of eventStream)
        visitorsGreeted.push(event.data.visitor)

    res.send(`${visitorsGreeted.length} visitors have been greeted, they are: [${visitorsGreeted.join(',')}]`)
})

app.listen(port, () => {
    console.log(`Sample app listening on port ${port}`)
})