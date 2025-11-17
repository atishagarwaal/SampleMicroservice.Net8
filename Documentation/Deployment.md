# Retail Microservices Deployment

This directory contains Kubernetes deployment infrastructure for the Retail Microservices .NET 8 system, following Kubernetes best practices.

## Structure

```
Deployment/
├── Charts/                    # Helm charts for each service
│   ├── retail-microservices/ # Main umbrella chart
│   ├── retail-customers-service/
│   ├── retail-products-service/
│   ├── retail-orders-write-service/
│   ├── retail-orders-read-service/
│   ├── retail-bff/
│   └── retail-ui/
├── Kubernetes/               # Raw Kubernetes manifests
│   ├── sqlserver.yaml
│   ├── mongodb.yaml
│   └── rabbitmq.yaml
└── Scripts/                  # Deployment scripts
    ├── deploy-local.ps1
    └── generate-service-charts.ps1
```

## Kubernetes Best Practices Implemented

### Security
- **Security Contexts**: Non-root user (UID 1000), dropped capabilities, no privilege escalation
- **Secrets Management**: Sensitive data stored in Kubernetes Secrets, not ConfigMaps
- **Service Accounts**: Dedicated service accounts for each service
- **Pod Security**: fsGroup set for proper volume permissions

### Health Checks
- **Liveness Probes**: Detect and restart unhealthy containers
- **Readiness Probes**: Ensure traffic only goes to ready pods
- **Separate Delays**: Different initial delays for liveness (30s) vs readiness (10s)
- **Appropriate Timeouts**: 5-10 second timeouts based on probe type

### Resource Management
- **Resource Limits**: CPU and memory limits set for all containers
- **Resource Requests**: Guaranteed resources for scheduling
- **Termination Grace Period**: 30 seconds for graceful shutdown

### Configuration
- **ConfigMaps**: Non-sensitive configuration stored in ConfigMaps
- **Environment Variables**: Sensitive values from Secrets via env vars
- **Labels**: Consistent labeling for resource selection and management

### Infrastructure
- **Health Checks**: All infrastructure components have liveness/readiness probes
- **Persistent Storage**: PVCs for data persistence
- **Service Discovery**: ClusterIP services for internal communication

## Prerequisites

- **Kubernetes Cluster**: Docker Desktop with Kubernetes enabled, minikube, or kind
- **kubectl**: Kubernetes command-line tool
- **Helm**: Version 3.0+ for managing deployments
- **Docker**: For building container images

## Quick Start - Local Deployment

### 1. Enable Kubernetes

**Docker Desktop:**
- Open Docker Desktop
- Go to Settings → Kubernetes
- Enable Kubernetes
- Click "Apply & Restart"

**Minikube:**
```bash
minikube start
```

**Kind:**
```bash
kind create cluster --name retail-cluster
```

### 2. Build Docker Images

Build all service images:
```powershell
.\Deployment\Scripts\deploy-local.ps1 -BuildImages
```

Or build individually:
```powershell
docker build -t retail-customers-service:latest -f Source/Retail.Customers/Retail.Customers.Service/Dockerfile Source/Retail.Customers/Retail.Customers.Service
docker build -t retail-products-service:latest -f Source/Retail.Products/Retail.Products.Service/Dockerfile Source/Retail.Products/Retail.Products.Service
docker build -t retail-orders-write-service:latest -f Source/Retail.Orders.Write/Retail.Orders.Write.Service/Dockerfile Source/Retail.Orders.Write/Retail.Orders.Write.Service
docker build -t retail-orders-read-service:latest -f Source/Retail.Orders.Read/Retail.Orders.Read.Service/Dockerfile Source/Retail.Orders.Read/Retail.Orders.Read.Service
docker build -t retail-bff:latest -f Source/Retail.BFF/Dockerfile Source/Retail.BFF
docker build -t retail-ui:latest -f Source/Retail.UI/Dockerfile Source/Retail.UI
```

### 3. Deploy Infrastructure (Optional)

If you want to deploy SQL Server, MongoDB, and RabbitMQ in Kubernetes:

```powershell
kubectl apply -f Deployment/Kubernetes/sqlserver.yaml
kubectl apply -f Deployment/Kubernetes/mongodb.yaml
kubectl apply -f Deployment/Kubernetes/rabbitmq.yaml
```

**Note:** For local development, you may prefer to use external services running on your host machine. In that case, skip infrastructure deployment and use `host.docker.internal` as the hostname.

### 4. Deploy Microservices

**Using the deployment script:**
```powershell
.\Deployment\Scripts\deploy-local.ps1 -BuildImages -SkipInfrastructure
```

**Using Helm directly:**
```powershell
# Update dependencies
cd Deployment/Charts/retail-microservices
helm dependency update
cd ../../..

# Deploy
helm install retail-microservices `
    Deployment/Charts/retail-microservices `
    --namespace retail-microservices `
    --create-namespace `
    -f Deployment/Charts/retail-microservices/values-local.yaml
```

### 5. Access Services

With NodePort services (local deployment):
- **UI**: http://localhost:30000
- **BFF**: http://localhost:30004
- **Customers**: http://localhost:30001
- **Products**: http://localhost:30003
- **Orders.Write**: http://localhost:30002
- **Orders.Read**: http://localhost:30005

## Configuration

### Environment Variables

Services are configured via environment variables and ConfigMaps. Key configuration areas:

- **Database Connection**: Set via `ConnectionStrings__DefaultConnection`
- **RabbitMQ**: Configured via `TopologyConfiguration__*` variables
- **OpenTelemetry**: Configured via `OpenTelemetry__*` variables

### Values Files

- **values.yaml**: Default production values
- **values-local.yaml**: Local development values with NodePort services

### Customizing Deployment

Edit the appropriate `values.yaml` file or create a custom values file:

```powershell
helm install retail-microservices `
    Deployment/Charts/retail-microservices `
    -f Deployment/Charts/retail-microservices/values-local.yaml `
    -f my-custom-values.yaml
```

## Monitoring

### Health Checks

All services expose health check endpoints:
- **Liveness**: `/health/liveness` - Used to restart unhealthy pods
- **Readiness**: `/health/readiness` - Used to route traffic only to ready pods

### Metrics

Prometheus metrics are available at `/metrics` on each service.

### Logs

View service logs:
```powershell
kubectl logs -f deployment/retail-customers-service -n retail-microservices
```

## Troubleshooting

### Services Not Starting

1. Check pod status:
   ```powershell
   kubectl get pods -n retail-microservices
   ```

2. View pod logs:
   ```powershell
   kubectl logs <pod-name> -n retail-microservices
   ```

3. Describe pod for events:
   ```powershell
   kubectl describe pod <pod-name> -n retail-microservices
   ```

### Database Connection Issues

- Verify SQL Server/MongoDB is running and accessible
- Check connection strings in environment variables:
  ```powershell
  kubectl get configmap -n retail-microservices
  kubectl describe configmap <configmap-name> -n retail-microservices
  ```

### RabbitMQ Connection Issues

- Verify RabbitMQ service is running:
  ```powershell
  kubectl get svc rabbitmq-service -n retail-microservices
  ```
- Check RabbitMQ management UI (if enabled): http://localhost:15672

## Cleanup

To remove the deployment:

```powershell
helm uninstall retail-microservices -n retail-microservices
kubectl delete namespace retail-microservices
```

## Production Deployment

For production deployments:

1. Use a container registry for images
2. Configure proper secrets management (e.g., Azure Key Vault, AWS Secrets Manager)
3. Enable autoscaling
4. Configure ingress with TLS
5. Set up monitoring and alerting
6. Use persistent volumes for databases
7. Configure resource limits appropriately
8. Review and adjust security contexts based on your security requirements

## Additional Resources

- [Helm Documentation](https://helm.sh/docs/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Kubernetes Best Practices](https://kubernetes.io/docs/concepts/security/pod-security-standards/)
- [.NET Container Images](https://hub.docker.com/_/microsoft-dotnet-aspnet)
