loki:
  tenant_id: ${tenant}
  auth_enabled: false
  querier:
    multi_tenant_queries_enabled: false
  storage:
    bucketNames:
      chunks: loki-chunks
      ruler: loki-ruler
      admin: loki-admin
    type: s3
    s3:
      endpoint: ${s3_endpoint}
      region: .
      secretAccessKey: ${s3_secret_access_key}
      accessKeyId: ${s3_access_key_id}
      s3ForcePathStyle: true
      insecure: true
  schemaConfig:
    configs:
    - from: 2024-05-28
      object_store: s3
      store: tsdb
      schema: v13
      index:
        prefix: index_
        period: 24h

chunksCache:
  enabled: true
  allocatedMemory: ${loki_chunks_cache_allocated_memory}

minio:
  enabled: false
