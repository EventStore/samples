#[macro_use]
extern crate rocket;

use eventstore::{AppendToStreamOptions, Client, EventData, ReadStreamOptions};
use rocket::State;
use serde::{Deserialize, Serialize};
use std::error::Error;
use uuid::Uuid;

type Result<A> = std::result::Result<A, Box<dyn Error>>;

#[derive(Serialize, Deserialize)]
struct VisitorGreeted {
    pub visitor: String,
}

const CONNECTION_STRING: &str =
    "esdb://admin:changeit@esdblocal:2113?tls=false&tlsVerifyCert=false";
const VISITORS_STREAM: &str = "visitors-stream";

#[rocket::main]
async fn main() {
    let event_store = Client::new(CONNECTION_STRING.parse().unwrap()).unwrap();

    let _ = rocket::build()
        .manage(event_store)
        .mount("/", routes![hello_world, hello_world_visitor])
        .launch()
        .await;
}

#[get("/hello-world")]
async fn hello_world(event_store: &State<Client>) -> String {
    greet_visitor(None, event_store).await.unwrap()
}

#[get("/hello-world?<visitor>")]
async fn hello_world_visitor(visitor: &str, event_store: &State<Client>) -> String {
    greet_visitor(Some(visitor), event_store).await.unwrap()
}

async fn greet_visitor(visitor: Option<&str>, event_store: &State<Client>) -> Result<String> {
    let visitor_or_default = visitor.unwrap_or("Visitor");
    let visitor_greeted = VisitorGreeted {
        visitor: visitor_or_default.to_string(),
    };

    let event_data = EventData::json("VisitorGreeted", visitor_greeted)?.id(Uuid::new_v4());

    event_store
        .append_to_stream(
            VISITORS_STREAM,
            &AppendToStreamOptions::default(),
            event_data,
        )
        .await?;

    let mut event_stream = event_store
        .read_stream(VISITORS_STREAM, &ReadStreamOptions::default())
        .await?;

    let mut visitors_greeted: Vec<String> = Vec::new();
    while let Some(re) = event_stream.next().await? {
        let vg = re.get_original_event().as_json::<VisitorGreeted>()?;
        visitors_greeted.push(vg.visitor);
    }

    Ok(format!(
        "{} visitors have been greeted, they are: [{}]",
        visitors_greeted.len(),
        visitors_greeted.join(",")
    ))
}
