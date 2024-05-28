namespaceOverride: ${namespace}

prometheus-windows-exporter:
  prometheus:
    monitor:
      enabled: false

grafana:
  enabled: true
  namespaceOverride: ${namespace}
  adminPassword: ${grafana_admin_password}
  sidecar:
    datasources:
      enabled: true
      url: http://prometheus-stack-kube-prom-prometheus:9090/

prometheus:
  enabled: true
  prometheusSpec:
    storageSpec:
      volumeClaimTemplate:
        spec:
          storageClassName: ${storage_class_name}
          accessModes: ["ReadWriteOnce"]
          resources:
            requests:
              storage: ${storage_size}
        selector: {}
