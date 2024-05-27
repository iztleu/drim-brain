terraform {
  required_version = ">= 1.7.3"
  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "=2.30"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "=2.13.2"
    }
  }
}

provider "kubernetes" {
  config_path = "./k3s_kubeconfig.yaml"
}

provider "helm" {
  kubernetes {
    config_path = "./k3s_kubeconfig.yaml"
  }
}
