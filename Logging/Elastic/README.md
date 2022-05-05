# Sending EventStoreDB logs to Elasticsearch

Elastic Stack is one of the most popular tools for ingesting and analyzing logs and statistics:
- [Elasticsearch](https://www.elastic.co/guide/en/elasticsearch/reference/8.2/index.html) was built for advanced filtering and text analysis. 
- [Filebeat](https://www.elastic.co/guide/en/beats/filebeat/8.2/index.html) allow tailing files efficiently. 
- [Logstash](https://www.elastic.co/guide/en/logstash/current/getting-started-with-logstash.html) enables log transformations and processing pipelines. 
- [Kibana](https://www.elastic.co/guide/en/kibana/8.2/index.html) is a dashboard and visualization UI for Elasticsearch data.

EventStoreDB exposes structured information through its logs and statistics, allowing straightforward integration with mentioned tooling.

This samples show how to configure various ways of sending logs from EventStoreDB to Elasticsearch:
- [Logstash](./Logstash/),
- [Filebeat](./Filebeat/),
- [FilebeatWithLogstash](./FilebeatWithLogstash/)

**DISCLAIMER: Configurations in samples are presented as docker-compose to simplify the developer environment setup. It aims to give the quick option to play with Elastic setup. It's NOT recommended to run setup through docker-compose on production. You should follow the [EventStoreDB installation guide](https://developers.eventstore.com/server/v21.10/installation.html) and [Elastic documentation](https://www.elastic.co/guide/index.html).**