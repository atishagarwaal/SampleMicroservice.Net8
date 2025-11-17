# Test execution script for Retail Microservices .NET 8 solution
# Usage: .\Build\Scripts\RunTests.ps1 [-Configuration Release] [-Filter "Category=UnitTests"] [-Coverage] [-Verbose]

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$Filter = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$Coverage = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$ResultsDirectory = "TestResults"
)

$ErrorActionPreference = "Stop"

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent (Split-Path -Parent $scriptPath)
$solutionFile = Join-Path $rootPath "SampleMicroservice.Net8.sln"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Running Tests" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
if ($Filter) {
    Write-Host "Filter: $Filter" -ForegroundColor Yellow
}
if ($Coverage) {
    Write-Host "Code Coverage: Enabled" -ForegroundColor Yellow
}
Write-Host ""

# Check if solution file exists
if (-not (Test-Path $solutionFile)) {
    Write-Host "ERROR: Solution file not found at $solutionFile" -ForegroundColor Red
    exit 1
}

# Create results directory
$resultsPath = Join-Path $rootPath $ResultsDirectory
if (-not (Test-Path $resultsPath)) {
    New-Item -ItemType Directory -Path $resultsPath -Force | Out-Null
}

# Build test arguments
$testArgs = @(
    "test",
    $solutionFile,
    "--configuration", $Configuration,
    "--no-build",
    "--logger", "trx",
    "--results-directory", $resultsPath
)

if ($Filter) {
    $testArgs += "--filter", $Filter
}

if ($Coverage) {
    $testArgs += "--collect:`"XPlat Code Coverage`""
}

if ($Verbose) {
    $testArgs += "--verbosity", "detailed"
}

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet $testArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Tests failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Tests completed successfully!" -ForegroundColor Green
Write-Host "Results: $resultsPath" -ForegroundColor Yellow
Write-Host "=========================================" -ForegroundColor Cyan

if ($Coverage) {
    Write-Host ""
    Write-Host "Code coverage reports generated in:" -ForegroundColor Yellow
    Get-ChildItem -Path $resultsPath -Recurse -Filter "coverage.cobertura.xml" | ForEach-Object {
        Write-Host "  $($_.FullName)" -ForegroundColor White
    }
}

