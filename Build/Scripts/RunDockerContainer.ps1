# Docker operations script for Retail Microservices
# Usage: .\Build\Scripts\RunDockerContainer.ps1 -Action [build|run|stop|remove] -Service [service-name] [-Tag latest] [-Port 7001]

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("build", "run", "stop", "remove", "list")]
    [string]$Action,
    
    [Parameter(Mandatory=$false)]
    [string]$Service = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Tag = "latest",
    
    [Parameter(Mandatory=$false)]
    [int]$Port = 0,
    
    [Parameter(Mandatory=$false)]
    [string]$Registry = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$All = $false
)

$ErrorActionPreference = "Stop"

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent (Split-Path -Parent $scriptPath)

# Service definitions
$services = @{
    "customers" = @{
        Name = "retail-customers-service"
        Path = "Source\Retail.Customers\Retail.Customers.Service"
        Dockerfile = "Source\Retail.Customers\Retail.Customers.Service\Dockerfile"
        Port = 7001
    }
    "products" = @{
        Name = "retail-products-service"
        Path = "Source\Retail.Products\Retail.Products.Service"
        Dockerfile = "Source\Retail.Products\Retail.Products.Service\Dockerfile"
        Port = 7003
    }
    "orders-write" = @{
        Name = "retail-orders-write-service"
        Path = "Source\Retail.Orders.Write\Retail.Orders.Write.Service"
        Dockerfile = "Source\Retail.Orders.Write\Retail.Orders.Write.Service\Dockerfile"
        Port = 7002
    }
    "orders-read" = @{
        Name = "retail-orders-read-service"
        Path = "Source\Retail.Orders.Read\Retail.Orders.Read.Service"
        Dockerfile = "Source\Retail.Orders.Read\Retail.Orders.Read.Service\Dockerfile"
        Port = 7005
    }
    "bff" = @{
        Name = "retail-bff"
        Path = "Source\Retail.BFF"
        Dockerfile = "Source\Retail.BFF\Dockerfile"
        Port = 7004
    }
    "ui" = @{
        Name = "retail-ui"
        Path = "Source\Retail.UI"
        Dockerfile = "Source\Retail.UI\Dockerfile"
        Port = 7000
    }
}

function Build-Image {
    param($ServiceInfo, $ImageTag, $ImageRegistry)
    
    $imageName = if ($ImageRegistry) { "$ImageRegistry/$($ServiceInfo.Name)" } else { $ServiceInfo.Name }
    $fullTag = "$imageName`:$ImageTag"
    $dockerfilePath = Join-Path $rootPath $ServiceInfo.Dockerfile
    $buildContext = Join-Path $rootPath $ServiceInfo.Path
    
    Write-Host "Building $fullTag..." -ForegroundColor Cyan
    Write-Host "  Dockerfile: $dockerfilePath" -ForegroundColor Gray
    Write-Host "  Context: $buildContext" -ForegroundColor Gray
    
    docker build -t $fullTag -f $dockerfilePath $buildContext
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to build $fullTag" -ForegroundColor Red
        return $false
    }
    
    Write-Host "✓ Built $fullTag" -ForegroundColor Green
    return $true
}

function Run-Container {
    param($ServiceInfo, $ImageTag, $ImageRegistry, $HostPort)
    
    $imageName = if ($ImageRegistry) { "$ImageRegistry/$($ServiceInfo.Name)" } else { $ServiceInfo.Name }
    $fullTag = "$imageName`:$ImageTag"
    $containerName = $ServiceInfo.Name
    $containerPort = $ServiceInfo.Port
    $portMapping = if ($HostPort) { "${HostPort}:${containerPort}" } else { "${containerPort}:${containerPort}" }
    
    Write-Host "Running container $containerName..." -ForegroundColor Cyan
    Write-Host "  Image: $fullTag" -ForegroundColor Gray
    Write-Host "  Port: $portMapping" -ForegroundColor Gray
    
    # Stop and remove existing container if it exists
    docker stop $containerName 2>$null
    docker rm $containerName 2>$null
    
    docker run -d `
        --name $containerName `
        -p $portMapping `
        $fullTag
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to run container $containerName" -ForegroundColor Red
        return $false
    }
    
    Write-Host "✓ Container $containerName is running" -ForegroundColor Green
    Write-Host "  Access at: http://localhost:$($HostPort -or $containerPort)" -ForegroundColor Yellow
    return $true
}

function Stop-Container {
    param($ServiceInfo)
    
    $containerName = $ServiceInfo.Name
    Write-Host "Stopping container $containerName..." -ForegroundColor Cyan
    
    docker stop $containerName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Container $containerName may not exist" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "✓ Stopped $containerName" -ForegroundColor Green
    return $true
}

function Remove-Container {
    param($ServiceInfo)
    
    $containerName = $ServiceInfo.Name
    Write-Host "Removing container $containerName..." -ForegroundColor Cyan
    
    docker stop $containerName 2>$null
    docker rm $containerName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Container $containerName may not exist" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "✓ Removed $containerName" -ForegroundColor Green
    return $true
}

# Main execution
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Docker Operations" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Action: $Action" -ForegroundColor Yellow
Write-Host ""

$servicesToProcess = @()

if ($All) {
    $servicesToProcess = $services.Values
} elseif ($Service) {
    if ($services.ContainsKey($Service)) {
        $servicesToProcess = @($services[$Service])
    } else {
        Write-Host "ERROR: Unknown service '$Service'" -ForegroundColor Red
        Write-Host "Available services: $($services.Keys -join ', ')" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "ERROR: Specify -Service or -All" -ForegroundColor Red
    exit 1
}

switch ($Action) {
    "build" {
        foreach ($svc in $servicesToProcess) {
            Build-Image -ServiceInfo $svc -ImageTag $Tag -ImageRegistry $Registry
        }
    }
    "run" {
        foreach ($svc in $servicesToProcess) {
            $hostPort = if ($Port) { $Port } else { $svc.Port }
            Run-Container -ServiceInfo $svc -ImageTag $Tag -ImageRegistry $Registry -HostPort $hostPort
        }
    }
    "stop" {
        foreach ($svc in $servicesToProcess) {
            Stop-Container -ServiceInfo $svc
        }
    }
    "remove" {
        foreach ($svc in $servicesToProcess) {
            Remove-Container -ServiceInfo $svc
        }
    }
    "list" {
        Write-Host "Available services:" -ForegroundColor Yellow
        foreach ($key in $services.Keys) {
            $svc = $services[$key]
            Write-Host "  $key - $($svc.Name) (Port: $($svc.Port))" -ForegroundColor White
        }
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Operation completed" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan

