receivers:
  otlp:
    protocols:
      grpc:
        endpoint: "0.0.0.0:4317"
      http:
        endpoint: "0.0.0.0:4318"
  prometheus:
    config:
      scrape_configs:
      - job_name: otel-collector
        scrape_interval: 10s
        static_configs:
        - targets: [ "0.0.0.0:8888" ]
        metric_relabel_configs:
        - action: labeldrop
          regex: (id|name)
          replacement: $$1
        - action: labelmap
          regex: label_(.+)
          replacement: $$1
  filelog:
    include_file_path: true
    include:
    - /var/log/pods/*/*/*.log
    operators:
    - id: container-parser
      type: container
    - type: json_parser
      id: json_parser
  kubeletstats:
    collection_interval: 10s
    auth_type: "serviceAccount"
    endpoint: "https://$${env:K8S_NODE_IP}:10250"
    insecure_skip_verify: true
    metric_groups:
      - pod
      - container

processors:
  batch:
  transform:
    log_statements:
    - context: log
      conditions:
      - attributes["LogLevel"] == "Trace"
      statements:
      - set(attributes["level"], "trace")
    - context: log
      conditions:
      - attributes["LogLevel"] == "Debug"
      statements:
      - set(attributes["level"], "debug")
    - context: log
      conditions:
      - attributes["LogLevel"] == "Information"
      statements:
      - set(attributes["level"], "info")
    - context: log
      conditions:
      - attributes["LogLevel"] == "Warning"
      statements:
      - set(attributes["level"], "warning")
    - context: log
      conditions:
      - attributes["LogLevel"] == "Error"
      statements:
      - set(attributes["level"], "error")
    - context: log
      conditions:
      - attributes["LogLevel"] == "Critical"
      statements:
      - set(attributes["level"], "fatal")
  attributes/loki_labels:
    actions:
    - action: insert
      key: loki.attribute.labels
      value: k8s.namespace.name, k8s.pod.name, k8s.container.name, level

exporters:
  otlp/tempo:
    endpoint: ${tempo_endpoint}
    tls:
      insecure: true
  prometheusremotewrite:
    endpoint: ${prometheus_remote_write_endpoint}
    headers:
      "X-Scope-OrgID": ${prometheus_remote_write_org_id}
  loki:
    endpoint: ${loki_endpoint}

service:
  pipelines:
    traces:
      receivers:
      - otlp
      processors:
      - batch
      exporters:
      - otlp/tempo
    metrics:
      receivers:
      - prometheus
      - kubeletstats
      processors:
      - batch
      exporters:
      - prometheusremotewrite
    logs:
      receivers:
      - filelog
      processors:
      - transform
      - attributes/loki_labels
      - batch
      exporters:
      - loki
