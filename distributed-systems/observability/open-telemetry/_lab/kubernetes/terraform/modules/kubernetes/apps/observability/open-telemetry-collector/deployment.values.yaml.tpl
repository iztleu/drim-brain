mode: deployment

image:
  repository: otel/opentelemetry-collector-contrib

presets:
  clusterMetrics:
    enabled: true
  kubernetesEvents:
    enabled: false

config:
  receivers:
    otlp:
      protocols:
        grpc:
          endpoint: "0.0.0.0:4317"
        http:
          endpoint: "0.0.0.0:4318"
  exporters:
    loki:
      endpoint: ${loki_endpoint}
    otlp:
      endpoint: ${tempo_endpoint}
      tls:
        insecure: true
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
        exporters:
        - otlp
