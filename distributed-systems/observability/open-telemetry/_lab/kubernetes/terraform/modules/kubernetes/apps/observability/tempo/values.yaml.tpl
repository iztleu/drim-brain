storage:
  trace:
    backend: s3
    s3:
      access_key: ${s3_access_key_id}
      secret_key: ${s3_secret_access_key}
      bucket: tempo-traces
      endpoint: ${s3_endpoint}
      insecure: true

minio:
  enabled: false

traces:
  otlp:
    grpc:
      enabled: true
    http:
      enabled: true
  zipkin:
    enabled: false
  jaeger:
    thriftHttp:
      enabled: false
  opencensus:
    enabled: false
