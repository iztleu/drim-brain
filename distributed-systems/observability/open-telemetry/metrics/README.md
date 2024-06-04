# Metrics

In OpenTelemetry, metrics are a type of telemetry data that provide quantitative measurements about a system's behavior. They are essential for monitoring the performance, health, and usage of applications and systems. Metrics in OpenTelemetry can be used to track various aspects such as latency, throughput, error rates, and resource utilization.

## Key Concepts

### Meters

* A `Meter` is the primary interface for creating and recording metrics in OpenTelemetry.

* It provides methods to create different types of instruments like counters, gauges, histograms, etc.

### Instruments

Instruments are the actual metrics you record. They come in various types, each suited for different kinds of measurements:

* __Counter__: A value that accumulates over time – you can think of this like an odometer on a car; it only ever goes up.

* __Asynchronous Counter__: Same as the Counter, but is collected once for each export. Could be used if you don’t have access to the continuous increments, but only to the aggregated value.

* __UpDownCounter__: A value that accumulates over time, but can also go down again. An example could be a queue length, it will increase and decrease with the number of work items in the queue.

* __Asynchronous UpDownCounter__: Same as the UpDownCounter, but is collected once for each export. Could be used if you don’t have access to the continuous changes, but only to the aggregated value (e.g., current queue size).

* __Gauge__: Measures a current value at the time it is read. An example would be the fuel gauge in a vehicle. Gauges are asynchronous.

* __Histogram__: A client-side aggregation of values, such as request latencies. A histogram is a good choice if you are interested in value statistics. For example: How many requests take fewer than 1s?

### Labels/Attributes

Labels (also referred to as attributes) are key-value pairs that provide additional context for the metrics. For example, you might tag a request duration metric with the endpoint that was called.

### Exporters

Exporters are used to send the collected metrics data to various backend systems for storage, analysis, and visualization. OpenTelemetry supports multiple exporters, including Prometheus, OTLP (OpenTelemetry Protocol), and more.

## Basic Workflow

1. __Setup and Initialization__: Initialize the OpenTelemetry SDK and configure the metrics pipeline, including the meter provider, instruments, and exporters.

2. __Create and Record Metrics__: Use the Meter to create instruments (counters, gauges, histograms). Record data using these instruments at appropriate points in your application code.

3. __Export and Visualize Metrics__: Configure exporters to send the metrics data to monitoring and visualization systems like Prometheus, Grafana, or other observability platforms.

## Methodologies

Three widely recognized methodologies for gathering metrics are USE (Utilization, Saturation, Errors), RED (Rate, Errors, Duration), and the Four Golden Signals. Each of these methodologies provides a different perspective on what to monitor and why. Here’s a detailed description of each:

### USE Methodology (Utilization, Saturation, Errors)

The USE methodology focuses on monitoring the state and performance of system resources. It is particularly useful for systems and infrastructure monitoring.

* __Utilization__:

  * __Definition__: Measures the average time a resource is busy or in use.
  * __Examples__: CPU utilization (percentage of time the CPU is active), memory utilization (percentage of memory in use), disk I/O utilization (percentage of time the disk is busy).
  * __Purpose__: Helps identify if resources are being used effectively or if they are underutilized or overutilized.

* __Saturation:

  * __Definition__: Measures the degree to which a resource is overloaded or has queued work.
  * __Examples__: Length of the CPU run queue, number of waiting disk I/O operations, memory swap rate.
  * __Purpose__: Helps identify bottlenecks and potential points of contention where performance might degrade due to high load.

* __Errors__:

  * __Definition__: Measures the count of error events.
  * __Examples__: Disk read/write errors, network packet errors, application exceptions.
  * __Purpose__: Helps identify reliability issues and track the frequency and types of errors occurring in the system.

### RED Methodology (Rate, Errors, Duration)

The RED methodology is tailored for monitoring microservices and is focused on the performance and reliability of individual services.

* __Rate__:

  * __Definition__: The number of requests per second the service is handling.
  * __Examples__: HTTP requests per second, transactions per second.
  * __Purpose__: Helps understand the load on the service and how it changes over time.

* __Errors__:

  * __Definition__: The number of failed requests per second.
  * __Examples__: HTTP 5xx responses per second, failed database transactions.
  * __Purpose__: Helps identify reliability issues and monitor the frequency of errors to ensure the service is functioning correctly.

* __Duration__:

  * __Definition__: The amount of time each request takes.
  * __Examples__: Average latency or response time of HTTP requests.
  * __Purpose__: Helps assess the performance of the service and identify latency issues that could affect user experience.

### Four Golden Signals

The Four Golden Signals provide a comprehensive approach to monitoring system performance and user experience, especially in distributed systems.

* __Latency__:

  * __Definition__: The time it takes to service a request.
  * __Examples__: Time taken for an HTTP request to complete, database query execution time.
  * __Purpose__: Helps identify performance bottlenecks and ensure that response times meet user expectations.

* __Traffic__:

  * __Definition__: A measure of how much demand is being placed on the system.
  * __Examples__: Number of HTTP requests per second, data throughput in bytes per second.
  * __Purpose__: Helps understand usage patterns and capacity requirements.

* __Errors__:

  * __Definition__: The rate of requests that fail.
  * __Examples__: Percentage of HTTP requests resulting in 4xx/5xx status codes, failed transactions.
  * __Purpose__: Helps ensure system reliability and identify issues that affect service availability.

* __Saturation__:

  * __Definition__: How “full” your service is.
  * __Examples__: CPU usage, memory usage, queue lengths.
  * __Purpose__: Helps identify resource constraints and potential performance issues due to high load.

### Summary

Each methodology offers a unique perspective on monitoring:

* USE focuses on system resources and is great for infrastructure monitoring.

* RED is service-centric, ideal for microservices monitoring.

* Four Golden Signals provide a balanced view suitable for distributed systems and user-facing applications.

Combining these methodologies can provide comprehensive insights into the health and performance of applications and systems.

## Links

* https://opentelemetry.io/docs/concepts/signals/metrics/
* https://opentelemetry.io/docs/specs/semconv/general/metrics/
* https://www.brendangregg.com/usemethod.html
* https://grafana.com/blog/2018/08/02/the-red-method-how-to-instrument-your-services/
* https://sre.google/sre-book/monitoring-distributed-systems/

#metrics
