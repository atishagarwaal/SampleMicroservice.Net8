# NuGet package publishing script for Retail Microservices Contracts
# Usage: .\Build\Scripts\PublishNuGetPackages.ps1 [-Version 1.0.0] [-Source nuget.org] [-ApiKey key] [-SkipBuild]

param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "1.0.0",
    
    [Parameter(Mandatory=$false)]
    [string]$Source = "nuget.org",
    
    [Parameter(Mandatory=$false)]
    [string]$ApiKey = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputDirectory = "NuGetPackages"
)

$ErrorActionPreference = "Stop"

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent (Split-Path -Parent $scriptPath)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Publishing NuGet Packages" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Source: $Source" -ForegroundColor Yellow
Write-Host ""

# Find all Contracts projects
$contractsProjects = @(
    "Source\Retail.Customers\Retail.Customers.Contracts\Retail.Customers.Contracts.csproj",
    "Source\Retail.Products\Retail.Products.Contracts\Retail.Products.Contracts.csproj",
    "Source\Retail.Orders.Write\Retail.Orders.Write.Contracts\Retail.Orders.Write.Contracts.csproj",
    "Source\Retail.Orders.Read\Retail.Orders.Read.Contracts\Retail.Orders.Read.Contracts.csproj"
)

$outputPath = Join-Path $rootPath $OutputDirectory
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
}

$packagesCreated = @()

foreach ($projectPath in $contractsProjects) {
    $fullPath = Join-Path $rootPath $projectPath
    
    if (-not (Test-Path $fullPath)) {
        Write-Host "WARNING: Project not found: $projectPath" -ForegroundColor Yellow
        continue
    }
    
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($fullPath)
    Write-Host "Processing $projectName..." -ForegroundColor Cyan
    
    # Build project if not skipped
    if (-not $SkipBuild) {
        Write-Host "  Building project..." -ForegroundColor Gray
        dotnet build $fullPath --configuration Release --no-restore
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR: Build failed for $projectName" -ForegroundColor Red
            continue
        }
    }
    
    # Pack project
    Write-Host "  Packing project..." -ForegroundColor Gray
    $packArgs = @(
        "pack",
        $fullPath,
        "--configuration", "Release",
        "--output", $outputPath,
        "--no-build"
    )
    
    if ($Version) {
        $packArgs += "/p:Version=$Version"
    }
    
    dotnet $packArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Pack failed for $projectName" -ForegroundColor Red
        continue
    }
    
    $packageFile = Get-ChildItem -Path $outputPath -Filter "$projectName.$Version.nupkg" | Select-Object -First 1
    if ($packageFile) {
        $packagesCreated += $packageFile.FullName
        Write-Host "  ✓ Created: $($packageFile.Name)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Packages created: $($packagesCreated.Count)" -ForegroundColor Green
Write-Host "Output directory: $outputPath" -ForegroundColor Yellow
Write-Host "=========================================" -ForegroundColor Cyan

# Publish packages if API key provided
if ($ApiKey -and $packagesCreated.Count -gt 0) {
    Write-Host ""
    Write-Host "Publishing packages to $Source..." -ForegroundColor Cyan
    
    foreach ($packageFile in $packagesCreated) {
        Write-Host "Publishing $([System.IO.Path]::GetFileName($packageFile))..." -ForegroundColor Yellow
        
        $pushArgs = @(
            "nuget", "push",
            $packageFile,
            "--source", $Source,
            "--api-key", $ApiKey,
            "--skip-duplicate"
        )
        
        dotnet $pushArgs
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Published successfully" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Failed to publish" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "Publishing completed!" -ForegroundColor Green
} elseif ($packagesCreated.Count -gt 0) {
    Write-Host ""
    Write-Host "To publish packages, run:" -ForegroundColor Yellow
    Write-Host "  dotnet nuget push <package> --source $Source --api-key <key>" -ForegroundColor White
}

