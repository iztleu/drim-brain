mode: daemonset

image:
  repository: otel/opentelemetry-collector-contrib

presets:
  kubernetesAttributes:
    enabled: true
  kubeletMetrics:
    enabled: false
  logsCollection:
    enabled: true

config:
  receivers:
    otlp:
      protocols:
        grpc:
          endpoint: "localhost:1234"
        http:
          endpoint: "localhost:5678"
  exporters:
    loki:
      endpoint: ${loki_endpoint}
    otlp:
      endpoint: ${tempo_endpoint}
  processors:
    resourcedetection:
      detectors: ["env", "system"]
      override: false
    transform/drop_unneeded_resource_attributes:
      error_mode: ignore
      log_statements:
      - context: resource
        statements:
        - delete_key(attributes, "k8s.pod.start_time")
        - delete_key(attributes, "os.description")
        - delete_key(attributes, "os.type")
        - delete_key(attributes, "process.command_args")
        - delete_key(attributes, "process.executable.path")
        - delete_key(attributes, "process.pid")
        - delete_key(attributes, "process.runtime.description")
        - delete_key(attributes, "process.runtime.name")
        - delete_key(attributes, "process.runtime.version")
  service:
    pipelines:
      logs:
        exporters:
        - loki
        processors:
        - resourcedetection
        - transform/drop_unneeded_resource_attributes
        - batch
      traces:
        receivers:
        - otlp
        processors:
        - batch
        exporters:
        - otlp
