# Jenkins CI/CD Pipeline

This document describes the Jenkins CI/CD pipeline configuration for the SampleMicroservice.Net8 solution.

## Overview

The `Jenkinsfile` defines a declarative pipeline that automates:
- Source code checkout
- NuGet package restoration
- Solution build
- Test execution (Unit, Component, Service tests)
- Code analysis
- Artifact publishing
- Docker image building (optional)
- NuGet package publishing (optional)

## Prerequisites

### Jenkins Configuration

1. **Install Required Plugins**:
   - Pipeline Plugin
   - .NET SDK Plugin (or configure .NET SDK manually)
   - Test Results Analyzer Plugin
   - Code Coverage Plugin (for coverage reports)
   - Docker Pipeline Plugin (if using Docker builds)
   - Email Extension Plugin (for notifications)

2. **Configure .NET SDK Tool**:
   - Go to Jenkins → Manage Jenkins → Global Tool Configuration
   - Add .NET SDK installation:
     - Name: `dotnet-sdk-8.0`
     - Install automatically: Yes
     - Version: 8.0.x (latest)

3. **Configure Credentials** (Optional):
   - Docker Registry credentials: `docker-registry-credentials`
   - Email credentials (if using email notifications)

4. **Configure Environment Variables** (Optional):
   - `DOCKER_REGISTRY_URL`: Your Docker registry URL (e.g., `registry.example.com`)
   - `CHANGE_AUTHOR_EMAIL`: Default email for build notifications

## Pipeline Stages

### 1. Checkout
- Checks out source code from SCM
- Captures Git commit hash for versioning

### 2. Restore NuGet Packages
- Restores all NuGet packages using `nuget.config`
- Ensures all dependencies are available

### 3. Build
- Builds the entire solution in Release configuration
- Archives build artifacts (DLLs, PDBs)

### 4. Unit Tests
- Runs all unit tests
- Generates TRX and JUnit test result files
- Collects code coverage data
- Publishes test results and coverage reports

### 5. Component Tests
- Runs component/integration tests
- Publishes test results

### 6. Service Tests
- Runs service/end-to-end tests
- Publishes test results

### 7. Code Analysis
- Runs code analyzers (StyleCop, FxCop, etc.)
- Validates code quality rules

### 8. Publish Artifacts
- Publishes compiled binaries for deployment

### 9. Build Docker Images (Optional)
- Builds Docker images for all services
- Only runs on main/develop/master branches
- Requires Docker registry configuration

### 10. Publish NuGet Packages (Optional)
- Packages Contracts projects as NuGet packages
- Only runs on main/master branches
- Archives packages for distribution

## Test Categories

The pipeline uses test categories to filter tests:
- `UnitTest`: Fast, isolated unit tests
- `ComponentTest`: Component/integration tests
- `ServiceTest`: Service/end-to-end tests

Ensure your test projects use these categories:
```csharp
[TestCategory("UnitTest")]
[TestCategory("ComponentTest")]
[TestCategory("ServiceTest")]
```

## Usage

### Creating a Jenkins Job

1. Create a new Pipeline job in Jenkins
2. Configure SCM:
   - Source Code Management: Git
   - Repository URL: Your repository URL
   - Branch: `*/main` (or your default branch)
3. Pipeline Definition:
   - Definition: Pipeline script from SCM
   - SCM: Git
   - Script Path: `Jenkinsfile`

### Running the Pipeline

The pipeline runs automatically on:
- Push to configured branches
- Pull request creation/updates (if configured)
- Manual trigger

### Manual Execution

You can manually trigger the pipeline from Jenkins UI or via:
```bash
curl -X POST http://jenkins-server/job/your-job-name/build
```

## Customization

### Disable Docker Builds

If you don't use Docker, the Docker build stage is already optional. It only runs when:
- Branch is main/develop/master
- Docker registry is configured

### Customize Test Execution

Modify the test stages to:
- Run specific test projects
- Use different test filters
- Configure test timeouts
- Add retry logic

### Add Deployment Stages

Add deployment stages after successful builds:
```groovy
stage('Deploy to Staging') {
    when {
        branch 'develop'
    }
    steps {
        // Add deployment steps
    }
}
```

## Troubleshooting

### Build Failures

1. Check .NET SDK version matches project target framework
2. Verify NuGet package sources are accessible
3. Ensure all test projects compile successfully

### Test Failures

1. Review test result reports in Jenkins
2. Check test environment requirements (databases, services)
3. Verify test categories are correctly applied

### Docker Build Failures

1. Ensure Docker is installed on Jenkins agent
2. Verify Docker registry credentials are correct
3. Check Dockerfile paths are correct

## Best Practices

1. **Keep Pipeline Fast**: Use parallel execution where possible
2. **Fail Fast**: Run quick checks (build, unit tests) first
3. **Artifact Retention**: Configure artifact retention policies
4. **Notifications**: Set up email/Slack notifications for failures
5. **Security**: Store sensitive credentials in Jenkins credentials store
6. **Versioning**: Use semantic versioning for releases

## Environment Variables

The pipeline uses these environment variables:
- `BUILD_NUMBER`: Jenkins build number
- `GIT_COMMIT_SHORT`: Short Git commit hash
- `BUILD_VERSION`: Combined version string
- `BUILD_CONFIGURATION`: Build configuration (Release/Debug)
- `DOCKER_REGISTRY_URL`: Docker registry URL (optional)

## Support

For issues or questions:
1. Check Jenkins console output
2. Review test result reports
3. Consult Jenkins pipeline documentation
4. Contact DevOps team

