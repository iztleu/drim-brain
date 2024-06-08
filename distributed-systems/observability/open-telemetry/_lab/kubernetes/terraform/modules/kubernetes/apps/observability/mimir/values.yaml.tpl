alertmanager:
  enabled: false

overrides_exporter:
  enabled: false

minio:
  enabled: false

nginx:
  enabled: false

metaMonitoring:
  dashboards:
    enabled: true

mimir:
  structuredConfig:
    common:
      storage:
        backend: s3
        s3:
          region: .
          endpoint: ${s3_endpoint}
          access_key_id: ${s3_access_key_id}
          secret_access_key: ${s3_secret_access_key}
          insecure: true
    blocks_storage:
      s3:
        bucket_name: mimir-blocks
    ruler_storage:
      s3:
        bucket_name: mimir-ruler
