secrets:
  name: main-env-configuration
  accessKey: ${access_key}
  secretKey: ${secret_key}

tenant:
  name: main
  configuration:
    name: main-env-configuration
  pools:
  - servers: 4
    name: pool-0
    volumesPerServer: 4
    size: ${storage_size}
    storageClassName: ${storage_class_name}
  metrics:
    enabled: true
  certificate:
    requestAutoCert: false
  prometheusOperator: true
