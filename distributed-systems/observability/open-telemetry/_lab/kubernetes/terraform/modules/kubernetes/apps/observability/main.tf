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
  name       = "prometheus-stack"
  repository = "https://prometheus-community.github.io/helm-charts"
  chart      = "kube-prometheus-stack"
  version    = "59.0.0"
  namespace  = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/helm/prometheus-stack/values.yaml.tpl", {
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
    kind       = "Certificate"
    metadata = {
      name      = local.grafana_cert
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
    kind       = "IngressRoute"
    metadata = {
      name      = "grafana"
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
          middlewares = [
            {
              name = "trusted-ips"
              namespace = "traefik"
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

resource "kubernetes_manifest" "prometheus_cert" {
  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind       = "Certificate"
    metadata = {
      name      = local.prometheus_cert
      namespace = kubernetes_namespace.observability.metadata[0].name
    }
    spec = {
      commonName = var.prometheus_url
      secretName = local.prometheus_cert
      dnsNames = [
        var.prometheus_url
      ]
      issuerRef = {
        kind = "ClusterIssuer"
        name = var.cert_manager_cluster_issuer
      }
    }
  }
}

resource "kubernetes_manifest" "prometheus_ingressroute" {
  manifest = {
    apiVersion = "traefik.io/v1alpha1"
    kind       = "IngressRoute"
    metadata = {
      name      = "prometheus"
      namespace = kubernetes_namespace.observability.metadata[0].name
    }
    spec = {
      entryPoints = [
        var.traefik_entry_point
      ]
      routes = [
        {
          match = "Host(`${var.prometheus_url}`)"
          kind  = "Rule"
          services = [
            {
              name = "prometheus-stack-kube-prom-prometheus"
              port = 9090
            }
          ]
        }
      ]
      tls = {
        secretName = local.prometheus_cert
      }
    }
  }
}

resource "kubernetes_manifest" "alertmanager_cert" {
  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind       = "Certificate"
    metadata = {
      name      = local.alertmanager_cert
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
    kind       = "IngressRoute"
    metadata = {
      name      = "alertmanager"
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
  name       = "loki"
  repository = "https://grafana.github.io/helm-charts"
  chart      = "loki"
  version    = "6.6.2"
  namespace  = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/helm/loki/values.yaml.tpl", {
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
  name       = "tempo"
  repository = "https://grafana.github.io/helm-charts"
  chart      = "tempo-distributed"
  version    = "1.9.11"
  namespace  = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/helm/tempo/values.yaml.tpl", {
      s3_endpoint = var.s3_endpoint,
      s3_access_key_id = var.s3_access_key_id,
      s3_secret_access_key = var.s3_secret_access_key,
    })
  ]
}

resource "kubernetes_config_map" "grafana_tempo_datasource" {
  metadata {
    name      = "grafana-tempo-datasource"
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

# OpenTelemetry Collector DaemonSet

resource "helm_release" "open-telemetry-collector-daemonset" {
  name       = "open-telemetry-collector-daemonset"
  repository = "https://open-telemetry.github.io/opentelemetry-helm-charts"
  chart      = "opentelemetry-collector"
  version    = "0.92.0"
  namespace  = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/helm/open-telemetry-collector/daemonset.values.yaml.tpl", {
      loki_endpoint = "http://loki-write:3100/loki/api/v1/push",
      tempo_endpoint = "http://tempo-ingester:3100",
    })
  ]
}

# OpenTelemetry Collector Deployment

resource "helm_release" "open-telemetry-collector-deployment" {
  name       = "open-telemetry-collector-deployment"
  repository = "https://open-telemetry.github.io/opentelemetry-helm-charts"
  chart      = "opentelemetry-collector"
  version    = "0.92.0"
  namespace  = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/helm/open-telemetry-collector/deployment.values.yaml.tpl", {
      loki_endpoint = "http://loki-write:3100/loki/api/v1/push",
    })
  ]
}
