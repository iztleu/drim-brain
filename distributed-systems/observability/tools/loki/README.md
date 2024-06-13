# Loki

Loki is a horizontally scalable, highly available, multi-tenant log aggregation system inspired by Prometheus. It is designed to store and query logs efficiently and to work seamlessly with Grafana, Mimir and Tempo.

## Architecture

![Architecture](_images/architecture.svg)

## Key Features and Capabilities

### Scalability and High Availability

* __Horizontal Scalability__: Loki can be scaled horizontally by adding more instances. Each instance can handle a portion of the log ingestion and querying, distributing the load and ensuring high performance.

* __High Availability__: Loki is designed to be highly available, with features like replication and distribution of data across multiple nodes to ensure that the system remains operational even if some nodes fail.

### Multi-Tenancy

Loki supports multi-tenancy, allowing multiple users or teams to use a single Loki instance while keeping their logs separate. Each tenant gets isolated storage and querying capabilities, ensuring data privacy and security.

### Log Ingestion:

* __Promtail__: Promtail is an agent that ships logs to Loki. It can scrape logs from various sources, such as local files and syslog, and send them to Loki with appropriate labels.

* __API and Integrations__: Logs can be ingested via the Loki HTTP API, and integrations exist for Kubernetes, Docker, and other systems to facilitate easy log collection.

### Label-Based Filtering

* __Labels__: Logs in Loki are tagged with labels, similar to metrics in Prometheus. These labels can be used to filter logs efficiently during querying. For example, logs can be labeled with job, instance, level, etc.

* __Efficient Indexing__: Loki indexes only the labels, not the full log content, making it more efficient in terms of storage and query performance compared to traditional full-text search systems.

### Query Language

* __LogQL__: Loki uses LogQL, a powerful query language inspired by PromQL (Prometheus Query Language). LogQL allows users to filter logs by labels, perform full-text searches, and aggregate logs over time.

* __Log Queries__: Basic log queries can filter logs by labels and perform regex searches on log content.

* __Metric Queries__: LogQL can also aggregate logs into metrics, allowing users to create dashboards and alerts based on log data.

### Integration with Grafana

* Loki integrates seamlessly with Grafana, a popular open-source visualization tool. Grafana provides a user-friendly interface for querying logs, creating dashboards, and setting up alerts based on Loki data.

* __Dashboards__: Users can create dynamic dashboards that combine log data from Loki with metrics from Prometheus and other sources.

* __Alerts__: Grafana can be configured to alert based on log patterns, leveraging Loki’s querying capabilities.

### Storage Backend

* __Configurable Storage__: Loki supports various storage backends, including local disk, object storage (S3, GCS), and more. This flexibility allows Loki to be deployed in different environments, from on-premises to cloud.

* __Chunk Storage__: Logs are stored in compressed chunks, which helps reduce storage costs and improve query performance.

### Performance and Efficiency

* __Optimized for Write-Heavy Workloads__: Loki is designed to handle high write volumes efficiently, making it suitable for environments with large amounts of log data, such as Kubernetes clusters.

* __Cost-Effective__: By indexing only labels and storing logs in compressed chunks, Loki offers a cost-effective solution for log storage compared to traditional logging systems.

## Loki vs ElasticSearch

### Loki Indexing Approach

#### Label-Based Indexing

* __Metadata Focus__: Loki indexes logs based on metadata (labels) attached to log streams, such as job, instance, level, etc.

* __Minimal Indexing__: Only labels are indexed, not the full log content.

#### Chunk Storage

* __Compressed Chunks__: Logs are stored in compressed chunks, referenced by their labels.

* __Separation of Index and Data__: Indexes for labels are separate from the actual log data chunks.

#### Storage Efficiency

* __Reduced Overhead__: By indexing only labels, Loki reduces the overhead and storage requirements compared to systems that index full log content.

### Elasticsearch Indexing Approach

#### Full-Text Indexing

* __Content Indexing__: Elasticsearch indexes the full content of logs, enabling comprehensive search capabilities.

* __Inverted Index__: Uses an inverted index to store each unique term with a list of documents (logs) containing that term.

#### Field-Based Indexing

* __Schema Flexibility__: Can index specific fields within log documents, allowing for structured queries on those fields.

#### Storage Requirements

* __Higher Overhead__: Due to full content indexing, Elasticsearch typically requires more storage space.

### Summary

* __Loki__: Indexes only metadata labels, resulting in minimal indexing overhead and more efficient storage.

* __Elasticsearch__: Indexes full log content and specific fields, providing powerful search capabilities at the cost of higher storage requirements.

#loki
