resource "kubernetes_namespace" "observability" {
  metadata {
    name = var.namespace
  }
}

# Prometheus, Alertmanager & Grafana

locals {
  grafana_cert = "grafana-cert"
  prometheus_cert = "prometheus-cert"
  alertmanager_cert = "alertmanager-cert"
  mimir_tenant = "otel-lab"
}

resource "random_string" "grafana_admin_password" {
  length = 16
  special = true
}

resource "kubernetes_secret" "grafana_admin_password" {
  metadata {
    name = "grafana-admin-password"
    namespace = kubernetes_namespace.observability.metadata[0].name
  }
  data = {
    password = random_string.grafana_admin_password.result
  }
}

resource "helm_release" "prometheus-stack" {
  name = "prometheus-stack"
  repository = "https://prometheus-community.github.io/helm-charts"
  chart = "kube-prometheus-stack"
  version = "59.0.0"
  namespace = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/prometheus-stack/values.yaml.tpl", {
      namespace = kubernetes_namespace.observability.metadata[0].name,
      storage_class_name = var.storage_class_name,
      storage_size = var.prometheus_storage_size,
      grafana_admin_password = random_string.grafana_admin_password.result
    })
  ]
}

resource "kubernetes_manifest" "grafana_cert" {
  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind = "Certificate"
    metadata = {
      name = local.grafana_cert
      namespace = kubernetes_namespace.observability.metadata[0].name
    }
    spec = {
      commonName = var.grafana_url
      secretName = local.grafana_cert
      dnsNames = [
        var.grafana_url
      ]
      issuerRef = {
        kind = "ClusterIssuer"
        name = var.cert_manager_cluster_issuer
      }
    }
  }
}

resource "kubernetes_manifest" "grafana_ingressroute" {
  manifest = {
    apiVersion = "traefik.io/v1alpha1"
    kind = "IngressRoute"
    metadata = {
      name = "grafana"
      namespace = kubernetes_namespace.observability.metadata[0].name
    }
    spec = {
      entryPoints = [
        var.traefik_entry_point
      ]
      routes = [
        {
          match = "Host(`${var.grafana_url}`)"
          kind  = "Rule"
          services = [
            {
              name = "prometheus-stack-grafana"
              port = 80
            }
          ]
        }
      ]
      tls = {
        secretName = local.grafana_cert
      }
    }
  }
}

resource "kubernetes_manifest" "alertmanager_cert" {
  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind = "Certificate"
    metadata = {
      name = local.alertmanager_cert
      namespace = kubernetes_namespace.observability.metadata[0].name
    }
    spec = {
      commonName = var.alertmanager_url
      secretName = local.alertmanager_cert
      dnsNames = [
        var.alertmanager_url
      ]
      issuerRef = {
        kind = "ClusterIssuer"
        name = var.cert_manager_cluster_issuer
      }
    }
  }
}

resource "kubernetes_manifest" "alertmanager_ingressroute" {
  manifest = {
    apiVersion = "traefik.io/v1alpha1"
    kind = "IngressRoute"
    metadata = {
      name = "alertmanager"
      namespace = kubernetes_namespace.observability.metadata[0].name
    }
    spec = {
      entryPoints = [
        var.traefik_entry_point
      ]
      routes = [
        {
          match = "Host(`${var.alertmanager_url}`)"
          kind  = "Rule"
          services = [
            {
              name = "prometheus-stack-kube-prom-alertmanager"
              port = 9093
            }
          ]
        }
      ]
      tls = {
        secretName = local.alertmanager_cert
      }
    }
  }
}

# Loki

resource "helm_release" "loki" {
  name = "loki"
  repository = "https://grafana.github.io/helm-charts"
  chart = "loki"
  version = "6.6.2"
  namespace = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/loki/values.yaml.tpl", {
      s3_endpoint = var.s3_endpoint,
      s3_access_key_id = var.s3_access_key_id,
      s3_secret_access_key = var.s3_secret_access_key,
      loki_chunks_cache_allocated_memory = var.loki_chunks_cache_allocated_memory,
    })
  ]
}

resource "kubernetes_config_map" "grafana_loki_datasource" {
  metadata {
    name = "grafana-loki-datasource"
    namespace = kubernetes_namespace.observability.metadata[0].name
    labels = {
      grafana_datasource = "1"
    }
  }

  data = {
    "loki-datasource.yaml" = <<-EOT
      apiVersion: 1
      datasources:
        - name: Loki
          type: loki
          access: proxy
          url: http://loki-read:3100  # Replace with your Loki service URL
          isDefault: false
          jsonData:
            maxLines: 1000
      EOT
  }
}

# Tempo

resource "helm_release" "tempo" {
  name = "tempo"
  repository = "https://grafana.github.io/helm-charts"
  chart = "tempo-distributed"
  version = "1.9.11"
  namespace = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/tempo/values.yaml.tpl", {
      s3_endpoint = var.s3_endpoint,
      s3_access_key_id = var.s3_access_key_id,
      s3_secret_access_key = var.s3_secret_access_key,
    })
  ]
}

resource "kubernetes_config_map" "grafana_tempo_datasource" {
  metadata {
    name = "grafana-tempo-datasource"
    namespace = kubernetes_namespace.observability.metadata[0].name
    labels = {
      grafana_datasource = "1"
    }
  }

  data = {
    "tempo-datasource.yaml" = <<-EOT
      apiVersion: 1
      datasources:
        - name: Tempo
          type: tempo
          access: proxy
          url: http://tempo-query-frontend:3100
          isDefault: false
          jsonData:
            httpMethod: POST
      EOT
  }
}

# Mimir

resource "helm_release" "mimir" {
  name = "mimir"
  repository = "https://grafana.github.io/helm-charts"
  chart = "mimir-distributed"
  version = "5.3.0"
  namespace  = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/mimir/values.yaml.tpl", {
      s3_endpoint = var.s3_endpoint,
      s3_access_key_id = var.s3_access_key_id,
      s3_secret_access_key = var.s3_secret_access_key,
    })
  ]
}

resource "kubernetes_config_map" "grafana_mimir_datasource" {
  metadata {
    name = "grafana-mimir-datasource"
    namespace = kubernetes_namespace.observability.metadata[0].name
    labels = {
      grafana_datasource = "1"
    }
  }

  data = {
    "tempo-datasource.yaml" = <<-EOT
      apiVersion: 1
      datasources:
        - name: Mimir
          type: prometheus
          access: proxy
          url: http://mimir-query-frontend:8080/prometheus
          isDefault: false
          jsonData:
            httpMethod: POST
            httpHeaderName1: X-Scope-OrgID
          secureJsonData:
            httpHeaderValue1: ${local.mimir_tenant}
      EOT
  }
}

# OpenTelemetry Operator

resource "helm_release" "open_telemetry_operator" {
  name = "open-telemetry-operator"
  repository = "https://open-telemetry.github.io/opentelemetry-helm-charts"
  chart = "opentelemetry-operator"
  version = "0.61.0"
  namespace = kubernetes_namespace.observability.metadata[0].name

  set {
    name = "manager.collectorImage.repository"
    value = "otel/opentelemetry-collector-contrib"
  }
}

# OpenTelemetry Collector DaemonSet

resource "kubernetes_manifest" "open_telemetry_collector_daemonset" {
  computed_fields = ["spec.config"]
  manifest = {
    apiVersion = "opentelemetry.io/v1alpha1"
    kind = "OpenTelemetryCollector"
    metadata = {
      name = "open-telemetry-collector-daemonset"
      namespace = kubernetes_namespace.observability.metadata[0].name
    }
    spec = {
      mode = "daemonset"
      targetAllocator = {
        enabled = true
        allocationStrategy = "per-node"
        prometheusCR = {
          enabled = true
        }
      }
      env = [
        {
          name = "K8S_NODE_IP"
          valueFrom = {
            fieldRef = {
              fieldPath = "status.hostIP"
            }
          }
        }
      ]
      config = templatefile("${path.module}/open-telemetry-collector/daemonset.config.yaml.tpl", {
        tempo_endpoint = "http://tempo-distributor:4317",
        prometheus_remote_write_endpoint = "http://mimir-distributor:8080/api/v1/push",
        prometheus_remote_write_org_id = local.mimir_tenant,
      })
    }
  }
}

## RBAC configs for DaemonSet Collector

resource "kubernetes_cluster_role" "open_telemetry_collector_daemonset_role" {
  metadata {
    name = "open-telemetry-collector-daemonset-role"
  }
  rule {
    api_groups = [""]
    resources = ["nodes/stats"]
    verbs = ["get", "list", "watch"]
  }
}

resource "kubernetes_cluster_role_binding" "open_telemetry_collector_daemonset_role_binding" {
  metadata {
    name = "open-telemetry-collector-daemonset-role-binding"
  }

  subject {
    kind = "ServiceAccount"
    name = "open-telemetry-collector-daemonset-collector"
    namespace = kubernetes_namespace.observability.metadata[0].name
  }

  role_ref {
    kind = "ClusterRole"
    name = kubernetes_cluster_role.open_telemetry_collector_daemonset_role.metadata[0].name
    api_group = "rbac.authorization.k8s.io"
  }
}

## RBAC configs for Target Allocator

resource "kubernetes_cluster_role" "open_telemetry_target_allocator_role" {
  metadata {
    name = "open-telemetry-target-allocator-role"
  }

  rule {
    api_groups = [""]
    resources = ["nodes", "nodes/metrics", "services", "endpoints", "pods"]
    verbs = ["get", "list", "watch"]
  }

  rule {
    api_groups = [""]
    resources = ["configmaps"]
    verbs = ["get"]
  }

  rule {
    api_groups = ["discovery.k8s.io"]
    resources = ["endpointslices"]
    verbs = ["get", "list", "watch"]
  }

  rule {
    api_groups = ["networking.k8s.io"]
    resources = ["ingresses"]
    verbs = ["get", "list", "watch"]
  }

  rule {
    non_resource_urls = ["/metrics"]
    verbs = ["get"]
  }

  rule {
    api_groups = ["monitoring.coreos.com"]
    resources = ["servicemonitors", "podmonitors"]
    verbs = ["*"]
  }

  rule {
    api_groups = [""]
    resources = ["namespaces"]
    verbs = ["get", "list", "watch"]
  }
}

resource "kubernetes_cluster_role_binding" "open_telemetry_target_allocator_cluster_role_binding" {
  metadata {
    name = "open-telemetry-target-allocator-cluster-role-binding"
  }

  subject {
    kind = "ServiceAccount"
    name = "open-telemetry-collector-daemonset-targetallocator"
    namespace = kubernetes_namespace.observability.metadata[0].name
  }

  role_ref {
    kind = "ClusterRole"
    name = kubernetes_cluster_role.open_telemetry_target_allocator_role.metadata[0].name
    api_group = "rbac.authorization.k8s.io"
  }
}

# OpenTelemetry Collector Deployment

# resource "kubernetes_manifest" "open-telemetry-collector-deployment" {
#   computed_fields = ["spec.config"]
#   manifest = {
#     apiVersion = "opentelemetry.io/v1alpha1"
#     kind = "OpenTelemetryCollector"
#     metadata = {
#       name = "open-telemetry-collector-deployment"
#       namespace = kubernetes_namespace.observability.metadata[0].name
#     }
#     spec = {
#       mode = "deployment"
#       config = templatefile("${path.module}/open-telemetry-collector/deployment.config.yaml.tpl", {
#         prometheus_remote_write_endpoint = "http://mimir-distributor:8080/api/v1/push",
#         prometheus_remote_write_org_id = local.mimir_tenant,
#       })
#     }
#   }
# }
