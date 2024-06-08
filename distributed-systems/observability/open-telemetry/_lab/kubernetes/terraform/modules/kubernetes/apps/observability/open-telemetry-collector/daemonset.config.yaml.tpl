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

exporters:
  otlp/tempo:
    endpoint: ${tempo_endpoint}
    tls:
      insecure: true
  prometheusremotewrite:
    endpoint: ${prometheus_remote_write_endpoint}
    headers:
      "X-Scope-OrgID": ${prometheus_remote_write_org_id}

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
