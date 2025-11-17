# Script to generate Helm charts for remaining services based on customers-service template

$services = @(
    @{
        Name = "retail-products-service"
        Component = "products-service"
        ServiceName = "Retail.Products"
        Port = 7003
        Database = "Retail.Product"
        HasDatabase = $true
        HasMongoDB = $false
    },
    @{
        Name = "retail-orders-write-service"
        Component = "orders-write-service"
        ServiceName = "Retail.Orders.Write"
        Port = 7002
        Database = "Retail.Order"
        HasDatabase = $true
        HasMongoDB = $false
    },
    @{
        Name = "retail-orders-read-service"
        Component = "orders-read-service"
        ServiceName = "Retail.Orders.Read"
        Port = 7005
        Database = "OrdersDb"
        HasDatabase = $false
        HasMongoDB = $true
    },
    @{
        Name = "retail-bff"
        Component = "bff"
        ServiceName = "Retail.BFF"
        Port = 7004
        Database = ""
        HasDatabase = $false
        HasMongoDB = $false
    },
    @{
        Name = "retail-ui"
        Component = "ui"
        ServiceName = "Retail.UI"
        Port = 7000
        Database = ""
        HasDatabase = $false
        HasMongoDB = $false
    }
)

$templatePath = "Deployment/Charts/retail-customers-service"
$basePath = "Deployment/Charts"

foreach ($service in $services) {
    $chartPath = Join-Path $basePath $service.Name
    
    Write-Host "Generating chart for $($service.Name)..." -ForegroundColor Cyan
    
    # Chart.yaml
    $chartYaml = @"
apiVersion: v2
name: $($service.Name)
description: A Helm chart for $($service.ServiceName)
type: application
version: 1.0.0
appVersion: "1.0.0"
"@
    Set-Content -Path (Join-Path $chartPath "Chart.yaml") -Value $chartYaml
    
    # Copy and modify templates
    $templates = @("_helpers.tpl", "service.yaml", "configmap.yaml", "secret.yaml", "hpa.yaml")
    foreach ($template in $templates) {
        $sourceFile = Join-Path $templatePath "templates\$template"
        $destFile = Join-Path $chartPath "templates\$template"
        
        if (Test-Path $sourceFile) {
            $content = Get-Content $sourceFile -Raw
            $content = $content -replace "retail-customers-service", $service.Name
            $content = $content -replace "customers-service", $service.Component
            $content = $content -replace "Retail.Customers", $service.ServiceName
            Set-Content -Path $destFile -Value $content
        }
    }
    
    # Generate deployment.yaml with service-specific config
    $deploymentContent = Get-Content (Join-Path $templatePath "templates\deployment.yaml") -Raw
    $deploymentContent = $deploymentContent -replace "retail-customers-service", $service.Name
    $deploymentContent = $deploymentContent -replace "customers-service", $service.Component
    $deploymentContent = $deploymentContent -replace "Retail.Customers", $service.ServiceName
    $deploymentContent = $deploymentContent -replace "7001", $service.Port
    
    # Adjust database connection string based on service type
    if ($service.HasMongoDB) {
        $dbConnection = "mongodb://mongodb-service:27017"
        $dbEnvVar = @"
            - name: ConnectionStrings__DefaultConnection
              value: "$dbConnection"
            - name: MongoDBSettings__DatabaseName
              value: "$($service.Database)"
            - name: MongoDBSettings__ConnectionString
              value: "$dbConnection"
"@
        $deploymentContent = $deploymentContent -replace "(?s)- name: ConnectionStrings__DefaultConnection.*?MultipleActiveResultSets=True;", $dbEnvVar
        $deploymentContent = $deploymentContent -replace "- name: DB_PASSWORD.*?key: db-password", ""
    } elseif (-not $service.HasDatabase) {
        # Remove database-related env vars for services without databases
        $deploymentContent = $deploymentContent -replace "(?s)- name: ConnectionStrings__DefaultConnection.*?MultipleActiveResultSets=True;", ""
        $deploymentContent = $deploymentContent -replace "- name: DB_PASSWORD.*?key: db-password", ""
    } else {
        $deploymentContent = $deploymentContent -replace "Retail.Customer", $service.Database
    }
    
    Set-Content -Path (Join-Path $chartPath "templates\deployment.yaml") -Value $deploymentContent
    
    # Generate values.yaml
    $valuesContent = Get-Content (Join-Path $templatePath "values.yaml") -Raw
    $valuesContent = $valuesContent -replace "retail-customers-service", $service.Name
    $valuesContent = $valuesContent -replace "7001", $service.Port
    $valuesContent = $valuesContent -replace "Retail.Customer", $service.Database
    $valuesContent = $valuesContent -replace "customers.local", "$($service.Component).local"
    
    if ($service.HasMongoDB) {
        $valuesContent = $valuesContent -replace 'database:\s*server:.*', "database:`n    server: `"mongodb-service`""
        $valuesContent = $valuesContent -replace 'database:\s*port:.*', "database:`n    port: 27017"
        $valuesContent = $valuesContent -replace 'database:\s*database:.*', "database:`n    database: `"$($service.Database)`""
    } elseif (-not $service.HasDatabase) {
        $valuesContent = $valuesContent -replace '(?s)config:.*database:.*trustServerCertificate:.*', "config:`n  environment: Development`n  rabbitmq:"
    }
    
    Set-Content -Path (Join-Path $chartPath "values.yaml") -Value $valuesContent
    
    Write-Host "  ✓ Generated $($service.Name) chart" -ForegroundColor Green
}

Write-Host ""
Write-Host "All service charts generated!" -ForegroundColor Green

