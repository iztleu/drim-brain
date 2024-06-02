global:
  domain: ${url}

redis-ha:
  enabled: true

controller:
  replicas: 1

server:
  replicas: 2
  certificate:
    enabled: false

repoServer:
  replicas: 2

applicationSet:
  replicas: 2

configs:
  params:
    server.insecure: true
