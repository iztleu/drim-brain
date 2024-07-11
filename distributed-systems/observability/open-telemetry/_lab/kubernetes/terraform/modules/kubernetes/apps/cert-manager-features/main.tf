resource "kubernetes_manifest" "cluster_issuer" {
  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind = "ClusterIssuer"
    metadata = {
      name = var.cluster_issuer
    }
    spec = {
      acme = {
        email = var.acme_email
        server = "https://acme-v02.api.letsencrypt.org/directory"
        privateKeySecretRef = {
          name = "letsencrypt-issuer-account-key"
        }
        solvers = [{
          http01 = {
            ingress = {
              serviceType      = "ClusterIP"
              ingressClassName = "traefik"
            }
          }
        }]
      }
    }
  }
}
