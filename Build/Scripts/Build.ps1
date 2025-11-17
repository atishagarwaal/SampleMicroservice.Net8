# Build script for Retail Microservices .NET 8 solution
# Usage: .\Build\Scripts\Build.ps1 [-Configuration Release] [-NoRestore] [-Verbose]

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [switch]$NoRestore = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent (Split-Path -Parent $scriptPath)
$solutionFile = Join-Path $rootPath "SampleMicroservice.Net8.sln"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Building Retail Microservices Solution" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Solution: $solutionFile" -ForegroundColor Yellow
Write-Host ""

# Check if solution file exists
if (-not (Test-Path $solutionFile)) {
    Write-Host "ERROR: Solution file not found at $solutionFile" -ForegroundColor Red
    exit 1
}

# Restore packages
if (-not $NoRestore) {
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    $restoreArgs = @("restore", $solutionFile)
    if ($Verbose) {
        $restoreArgs += "--verbosity", "detailed"
    }
    
    dotnet $restoreArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Package restore failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Packages restored" -ForegroundColor Green
    Write-Host ""
}

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
$buildArgs = @(
    "build",
    $solutionFile,
    "--configuration", $Configuration,
    "--no-incremental"
)

if ($Verbose) {
    $buildArgs += "--verbosity", "detailed"
}

dotnet $buildArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan

