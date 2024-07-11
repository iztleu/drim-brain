# resource "kubernetes_manifest" "crypto_bank_app_project" {
#   manifest = {
#     apiVersion = "argoproj.io/v1alpha1"
#     kind       = "AppProject"
#     metadata = {
#       name      = "crypto-bank"
#       namespace = "argocd"
#       finalizers = [
#         "resources-finalizer.argocd.argoproj.io"
#       ]
#     }
#     spec = {
#       description = "Crypto Bank"
#       sourceRepos = ["*"]
#       destinations = [{
#         namespace = "default"
#         server    = "https://kubernetes.default.svc"
#       }]
#     }
#   }
# }

# resource "kubernetes_manifest" "crypto_bank_api_application" {
#   manifest = {
#     apiVersion = "argoproj.io/v1alpha1"
#     kind = "Application"
#     metadata = {
#       name = "crypto-bank-api"
#       namespace = "argocd"
#     }
#     spec = {
#       project = "crypto-bank"
#       source = {
#         repoURL = "https://github.com/drim-dev/drim-brain.git"
#         targetRevision = "HEAD"
#         path = "distributed-systems/observability/open-telemetry/_lab/kubernetes/argocd/charts/crypto-bank-api"
#         helm = {
#           releaseName = "crypto-bank-api"
#           valueFiles = [
#             "../../environments/development/crypto-bank-api/values.yaml"
#           ]
#         }
#       }
#       destination = {
#         server = "https://kubernetes.default.svc"
#         namespace = "default"
#       }
#       syncPolicy = {
#         automated = {
#           prune = true
#         }
#       }
#     }
#   }
# }
