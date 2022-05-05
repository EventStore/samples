# Sending EventStoreDB logs to Elasticsearch with Filebeat

Logstash was the initial Elastic try to provide a log harvester tool. However, it appeared to have performance limitations. Elastic came up with the [Beats family](https://www.elastic.co/beats/), which allows gathering data from various specialized sources (files, metrics, network data, etc.). Elastic recommends Filebeat as the log collection and shipment tool off the host servers. Filebeat uses a backpressure-sensitive protocol when sending data to Logstash or Elasticsearch to account for higher volumes of data.

Filebeat can pipe logs directly to Elasticsearch and set up a Kibana data view.

Filebeat needs to either be installed on the EventStoreDB node or have access to logs storage. The processing pipeline can be configured through the configuration file (e.g. `filebeat.yml`). This file contains the three essential building blocks:
- input - configuration for file source, e.g. if stored in JSON format.
- output - place where we'd like to put transformed logs, e.g. Elasticsearch, Logstash,
- setup - additional setup and simple transformations (e.g. Elasticsearch indices template, Kibana data view).

See the sample Filebeat 8.2 configuration file. It shows how to take the EventStoreDB log files, output them to Elasticsearch prefixing index with `eventstoredb` and create a Kibana data view:

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
#  ElasticSearch direct output
############################
output.elasticsearch:
  index: "eventstoredb-%{[agent.version]}"
  hosts: ["elasticsearch:9200"]

############################
#  ElasticSearch dashboard configuration
#  (index pattern and data view)
############################
setup.dashboards:
  enabled: true
  index: "eventstoredb-*"

setup.template:
  name: "eventstoredb"
  pattern: "eventstoredb-%{[agent.version]}"

############################
#  Kibana dashboard configuration
############################
setup.kibana:
  host: "kibana:5601"

```

You can play with such configuration through the [sample docker-compose](https://github.com/EventStore/samples/tree/main/Logging/Elastic/Filebeat).
