# PowerShell script to deploy Retail Microservices to local Kubernetes cluster
# Prerequisites: kubectl, helm, Docker Desktop with Kubernetes enabled (or minikube/kind)

param(
    [Parameter(Mandatory=$false)]
    [string]$Namespace = "retail-microservices",
    
    [Parameter(Mandatory=$false)]
    [switch]$BuildImages = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipInfrastructure = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$ImageRegistry = ""
)

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Retail Microservices Local Deployment" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Check prerequisites
Write-Host "`nChecking prerequisites..." -ForegroundColor Yellow
$tools = @("kubectl", "helm", "docker")
foreach ($tool in $tools) {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) {
        Write-Host "ERROR: $tool is not installed or not in PATH" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ✓ $tool found" -ForegroundColor Green
}

# Check Kubernetes connection
Write-Host "`nChecking Kubernetes connection..." -ForegroundColor Yellow
try {
    kubectl cluster-info | Out-Null
    Write-Host "  ✓ Kubernetes cluster is accessible" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Cannot connect to Kubernetes cluster" -ForegroundColor Red
    Write-Host "    Please ensure Kubernetes is running (Docker Desktop, minikube, or kind)" -ForegroundColor Yellow
    exit 1
}

# Create namespace
Write-Host "`nCreating namespace: $Namespace" -ForegroundColor Yellow
kubectl create namespace $Namespace --dry-run=client -o yaml | kubectl apply -f -
Write-Host "  ✓ Namespace created/verified" -ForegroundColor Green

# Build Docker images if requested
if ($BuildImages) {
    Write-Host "`nBuilding Docker images..." -ForegroundColor Yellow
    $services = @(
        @{Name="retail-customers-service"; Path="Source/Retail.Customers/Retail.Customers.Service"},
        @{Name="retail-products-service"; Path="Source/Retail.Products/Retail.Products.Service"},
        @{Name="retail-orders-write-service"; Path="Source/Retail.Orders.Write/Retail.Orders.Write.Service"},
        @{Name="retail-orders-read-service"; Path="Source/Retail.Orders.Read/Retail.Orders.Read.Service"},
        @{Name="retail-bff"; Path="Source/Retail.BFF"},
        @{Name="retail-ui"; Path="Source/Retail.UI"}
    )
    
    foreach ($service in $services) {
        Write-Host "  Building $($service.Name)..." -ForegroundColor Cyan
        $imageName = if ($ImageRegistry) { "$ImageRegistry/$($service.Name)" } else { $service.Name }
        docker build -t "${imageName}:latest" -f "$($service.Path)/Dockerfile" "$($service.Path)"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "    ✗ Failed to build $($service.Name)" -ForegroundColor Red
            exit 1
        }
        Write-Host "    ✓ Built $($service.Name)" -ForegroundColor Green
    }
}

# Deploy infrastructure (if not skipped)
if (-not $SkipInfrastructure) {
    Write-Host "`nDeploying infrastructure dependencies..." -ForegroundColor Yellow
    Write-Host "  Note: For local deployment, you may want to use external SQL Server, MongoDB, and RabbitMQ" -ForegroundColor Yellow
    Write-Host "  Set -SkipInfrastructure to skip infrastructure deployment" -ForegroundColor Yellow
    
    # SQL Server deployment (optional - can use external)
    Write-Host "`n  Deploying SQL Server..." -ForegroundColor Cyan
    kubectl apply -f Deployment/Kubernetes/sqlserver.yaml -n $Namespace
    Write-Host "    ✓ SQL Server deployment created" -ForegroundColor Green
    
    # MongoDB deployment (optional - can use external)
    Write-Host "  Deploying MongoDB..." -ForegroundColor Cyan
    kubectl apply -f Deployment/Kubernetes/mongodb.yaml -n $Namespace
    Write-Host "    ✓ MongoDB deployment created" -ForegroundColor Green
    
    # RabbitMQ deployment (optional - can use external)
    Write-Host "  Deploying RabbitMQ..." -ForegroundColor Cyan
    kubectl apply -f Deployment/Kubernetes/rabbitmq.yaml -n $Namespace
    Write-Host "    ✓ RabbitMQ deployment created" -ForegroundColor Green
    
    Write-Host "`n  Waiting for infrastructure to be ready..." -ForegroundColor Yellow
    kubectl wait --for=condition=ready pod -l app=sqlserver -n $Namespace --timeout=300s
    kubectl wait --for=condition=ready pod -l app=mongodb -n $Namespace --timeout=300s
    kubectl wait --for=condition=ready pod -l app=rabbitmq -n $Namespace --timeout=300s
}

# Update Helm dependencies
Write-Host "`nUpdating Helm chart dependencies..." -ForegroundColor Yellow
Push-Location Deployment/Charts/retail-microservices
helm dependency update
Pop-Location
Write-Host "  ✓ Dependencies updated" -ForegroundColor Green

# Deploy using Helm
Write-Host "`nDeploying microservices with Helm..." -ForegroundColor Yellow
$valuesFile = "Deployment/Charts/retail-microservices/values-local.yaml"
if ($ImageRegistry) {
    $tempValues = "Deployment/Charts/retail-microservices/values-local-temp.yaml"
    Copy-Item $valuesFile $tempValues
    Add-Content $tempValues "`nglobal:"
    Add-Content $tempValues "  imageRegistry: $ImageRegistry"
    $valuesFile = $tempValues
}

helm upgrade --install retail-microservices `
    Deployment/Charts/retail-microservices `
    --namespace $Namespace `
    --create-namespace `
    -f $valuesFile `
    --wait `
    --timeout 10m

if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ Helm deployment failed" -ForegroundColor Red
    exit 1
}

Write-Host "  ✓ Microservices deployed" -ForegroundColor Green

# Show deployment status
Write-Host "`nDeployment Status:" -ForegroundColor Yellow
kubectl get pods -n $Namespace
kubectl get services -n $Namespace

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "`nTo access services:" -ForegroundColor Yellow
Write-Host "  UI: http://localhost:30000" -ForegroundColor White
Write-Host "  BFF: http://localhost:30004" -ForegroundColor White
Write-Host "  Customers: http://localhost:30001" -ForegroundColor White
Write-Host "  Products: http://localhost:30003" -ForegroundColor White
Write-Host "  Orders.Write: http://localhost:30002" -ForegroundColor White
Write-Host "  Orders.Read: http://localhost:30005" -ForegroundColor White
Write-Host "`nTo view logs:" -ForegroundColor Yellow
Write-Host "  kubectl logs -f deployment/retail-customers-service -n $Namespace" -ForegroundColor White
Write-Host "`nTo delete deployment:" -ForegroundColor Yellow
Write-Host "  helm uninstall retail-microservices -n $Namespace" -ForegroundColor White

