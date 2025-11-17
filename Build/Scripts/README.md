# Build Scripts

This directory contains automation scripts for building, testing, and deploying the Retail Microservices solution.

## Available Scripts

### Build.ps1 / Build.sh
Builds the entire solution.

**PowerShell:**
```powershell
.\Build\Scripts\Build.ps1 [-Configuration Release] [-NoRestore] [-Verbose]
```

**Bash:**
```bash
./Build/Scripts/Build.sh [--configuration Release] [--no-restore] [--verbose]
```

**Examples:**
```powershell
# Build in Release mode (default)
.\Build\Scripts\Build.ps1

# Build in Debug mode
.\Build\Scripts\Build.ps1 -Configuration Debug

# Build without restoring packages
.\Build\Scripts\Build.ps1 -NoRestore
```

### RunTests.ps1 / RunTests.sh
Runs all tests in the solution.

**PowerShell:**
```powershell
.\Build\Scripts\RunTests.ps1 [-Configuration Release] [-Filter "Category=UnitTests"] [-Coverage] [-Verbose]
```

**Bash:**
```bash
./Build/Scripts/RunTests.sh [--configuration Release] [--filter "Category=UnitTests"] [--coverage] [--verbose]
```

**Examples:**
```powershell
# Run all tests
.\Build\Scripts\RunTests.ps1

# Run with code coverage
.\Build\Scripts\RunTests.ps1 -Coverage

# Run specific test category
.\Build\Scripts\RunTests.ps1 -Filter "Category=UnitTests"

# Run tests for specific service
.\Build\Scripts\RunTests.ps1 -Filter "FullyQualifiedName~Retail.Customers"
```

### RunDockerContainer.ps1 / RunDockerContainer.sh
Manages Docker containers for services.

**PowerShell:**
```powershell
.\Build\Scripts\RunDockerContainer.ps1 -Action [build|run|stop|remove|list] -Service [service-name] [-Tag latest] [-Port 7001] [-Registry registry] [-All]
```

**Bash:**
```bash
./Build/Scripts/RunDockerContainer.sh --action [build|run|stop|remove|list] --service [service-name] [--tag latest] [--port 7001] [--registry registry] [--all]
```

**Available Services:**
- `customers` - Retail.Customers.Service (Port: 7001)
- `products` - Retail.Products.Service (Port: 7003)
- `orders-write` - Retail.Orders.Write.Service (Port: 7002)
- `orders-read` - Retail.Orders.Read.Service (Port: 7005)
- `bff` - Retail.BFF (Port: 7004)
- `ui` - Retail.UI (Port: 7000)

**Examples:**
```powershell
# List available services
.\Build\Scripts\RunDockerContainer.ps1 -Action list

# Build a single service
.\Build\Scripts\RunDockerContainer.ps1 -Action build -Service customers

# Build all services
.\Build\Scripts\RunDockerContainer.ps1 -Action build -All

# Run a service container
.\Build\Scripts\RunDockerContainer.ps1 -Action run -Service customers -Port 7001

# Stop a service container
.\Build\Scripts\RunDockerContainer.ps1 -Action stop -Service customers

# Remove a service container
.\Build\Scripts\RunDockerContainer.ps1 -Action remove -Service customers
```

### PublishNuGetPackages.ps1 / PublishNuGetPackages.sh
Builds and publishes NuGet packages for Contracts projects.

**PowerShell:**
```powershell
.\Build\Scripts\PublishNuGetPackages.ps1 [-Version 1.0.0] [-Source nuget.org] [-ApiKey key] [-SkipBuild] [-OutputDirectory NuGetPackages]
```

**Bash:**
```bash
./Build/Scripts/PublishNuGetPackages.sh [--version 1.0.0] [--source nuget.org] [--api-key key] [--skip-build] [--output-directory NuGetPackages]
```

**Examples:**
```powershell
# Create packages without publishing
.\Build\Scripts\PublishNuGetPackages.ps1 -Version 1.0.0

# Create and publish packages
.\Build\Scripts\PublishNuGetPackages.ps1 -Version 1.0.0 -Source nuget.org -ApiKey YOUR_API_KEY

# Create packages with custom output directory
.\Build\Scripts\PublishNuGetPackages.ps1 -Version 1.0.0 -OutputDirectory ".\Packages"
```

## Prerequisites

- **.NET 8.0 SDK** - Required for building and testing
- **Docker** - Required for Docker operations (optional)
- **PowerShell 5.1+** or **Bash** - Required for running scripts

## Usage in CI/CD

These scripts are designed to be used in CI/CD pipelines:

```yaml
# Example GitHub Actions
- name: Build Solution
  run: ./Build/Scripts/Build.sh --configuration Release

- name: Run Tests
  run: ./Build/Scripts/RunTests.sh --coverage

- name: Build Docker Images
  run: ./Build/Scripts/RunDockerContainer.sh --action build --all
```

```groovy
// Example Jenkinsfile
stage('Build') {
    steps {
        bat 'powershell -File Build\\Scripts\\Build.ps1 -Configuration Release'
    }
}

stage('Test') {
    steps {
        bat 'powershell -File Build\\Scripts\\RunTests.ps1 -Coverage'
    }
}
```

## Script Conventions

- All scripts follow consistent parameter naming
- Exit codes: 0 = success, non-zero = failure
- Color-coded output for better readability
- Error handling with clear error messages
- Cross-platform support (PowerShell for Windows, Bash for Linux/Mac)

## Notes

- Bash scripts require execute permissions: `chmod +x Build/Scripts/*.sh`
- PowerShell scripts can be run directly or via `powershell -File`
- Docker scripts require Docker to be running
- NuGet publishing requires valid API key for the target source

