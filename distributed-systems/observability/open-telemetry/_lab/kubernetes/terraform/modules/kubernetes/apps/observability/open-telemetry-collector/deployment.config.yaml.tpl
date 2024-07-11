processors:
  batch:

exporters:
  prometheusremotewrite:
    endpoint: ${prometheus_remote_write_endpoint}
    headers:
      "X-Scope-OrgID": ${prometheus_remote_write_org_id}

service:
  pipelines:
    metrics:
      processors:
      - batch
      exporters:
      - prometheusremotewrite
