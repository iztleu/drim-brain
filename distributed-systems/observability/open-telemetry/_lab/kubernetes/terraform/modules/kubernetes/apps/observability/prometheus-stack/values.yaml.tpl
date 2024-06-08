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
      defaultDatasourceEnabled: false
      isDefaultDatasource: false
#      url: http://prometheus-stack-kube-prom-prometheus:9090/

prometheus:
  enabled: false
#  prometheusSpec:
#    storageSpec:
#      volumeClaimTemplate:
#        spec:
#          storageClassName: ${storage_class_name}
#          accessModes: ["ReadWriteOnce"]
#          resources:
#            requests:
#              storage: ${storage_size}
#        selector: {}
#    serviceMonitorSelectorNilUsesHelmValues: false
