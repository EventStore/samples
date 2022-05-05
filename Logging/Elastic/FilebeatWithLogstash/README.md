# Sending EventStoreDB logs to Elasticsearch with Filebeat with Logstash

Even though Filebeat can pipe logs directly to Elasticsearch and do a basic Kibana setup, you'd like to have more control and expand the processing pipeline. That's why for production, it's recommended to use both. Multiple Filebeat instances (e.g. from different EventStoreDB clusters) can collect logs and pipe them to Logstash, which will play an aggregator role. Filebeat can output logs to Logstash, and Logstash can receive and process these logs with the Beats input. Logstash can transform and route logs to Elasticsearch instance(s). 

In that configuration, Filebeat should be installed on the EventStoreDB node (or have access to file logs) and define Logstash as output. See the sample Filebeat 8.2 configuration file.

```yml
############################
#  EventStoreDB logs file input
############################
filebeat.inputs:
  - type: log
    paths:
      - /var/log/eventstore/*/log*.json
    json.keys_under_root: true
    json.add_error_key: true

############################
#  Logstash output to transform and prepare logs
############################
output.logstash:
  hosts: ["logstash:5044"]
```

Then the sample Logstash 8.2 configuration file will look like the below. It shows how to take the EventStoreDB logs from Filebeat, split them based on the log type (regular and stats) and output them to separate indices to Elasticsearch:

```ruby
############################
#  Filebeat input 
############################
input {
  beats {
    port => 5044
  }
}

############################
#  Filter out stats from regular logs
#  add respecting field with log type
############################
filter {
  # check if log path includes "log-stats"
  # so pattern for stats
  if [log][file][path] =~ "log-stats" {
    mutate {
      add_field => {
        "log_type" => "stats"
      }
    }
  }
  else {
    mutate {
      add_field => {
        "log_type" => "logs"
      }
    }
  }
}

############################
#  Send logs to Elastic
#  Create separate indexes for stats and regular logs
#  using field defined in the filter transformation
############################
output {
  elasticsearch {
    hosts => [ "elasticsearch:9200" ]
    index => 'eventstoredb-%{[log_type]}'
  }
}
```

You can play with such configuration through the [sample docker-compose](./docker-compose.yml) by runing in this folder:

```shell
docker-compose up
```