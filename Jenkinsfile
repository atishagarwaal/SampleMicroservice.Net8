pipeline {
    agent any

    tools {
        dotnet 'dotnet-sdk-8.0' // Configure this tool in Jenkins Global Tool Configuration
    }

    environment {
        SOLUTION_FILE = 'SampleMicroservice.Net8.sln'
        BUILD_CONFIGURATION = 'Release'
        DOTNET_VERSION = '8.0'
        // Docker registry configuration (optional)
        DOCKER_REGISTRY = credentials('docker-registry-credentials') // Configure in Jenkins credentials
        DOCKER_REGISTRY_URL = "${env.DOCKER_REGISTRY_URL ?: 'your-registry.com'}"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
                script {
                    env.GIT_COMMIT_SHORT = sh(
                        script: 'git rev-parse --short HEAD',
                        returnStdout: true
                    ).trim()
                    env.BUILD_VERSION = "${env.BUILD_NUMBER}-${env.GIT_COMMIT_SHORT}"
                }
            }
        }

        stage('Restore NuGet Packages') {
            steps {
                script {
                    echo 'Restoring NuGet packages...'
                    sh "dotnet restore ${SOLUTION_FILE} --configfile nuget.config"
                }
            }
        }

        stage('Build') {
            steps {
                script {
                    echo "Building solution: ${SOLUTION_FILE}"
                    sh """
                        dotnet build ${SOLUTION_FILE} \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-incremental \
                            --verbosity minimal
                    """
                }
            }
            post {
                success {
                    archiveArtifacts artifacts: '**/bin/**/*.dll', allowEmptyArchive: true
                    archiveArtifacts artifacts: '**/bin/**/*.pdb', allowEmptyArchive: true
                }
            }
        }

        stage('Unit Tests') {
            steps {
                script {
                    echo 'Running unit tests...'
                    sh """
                        dotnet test ${SOLUTION_FILE} \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-build \
                            --verbosity normal \
                            --logger 'trx;LogFileName=unit-test-results.trx' \
                            --logger 'junit;LogFileName=unit-test-results.xml' \
                            --filter 'Category=UnitTests' \
                            --collect:'XPlat Code Coverage' \
                            --results-directory:./TestResults/UnitTests
                    """
                }
            }
            post {
                always {
                    publishTestResults testResultsPattern: '**/unit-test-results.trx'
                    publishTestResults testResultsPattern: '**/unit-test-results.xml'
                    publishCoverage adapters: [
                        coberturaAdapter('**/coverage.cobertura.xml')
                    ], sourceFileResolver: sourceFiles('STORE_ALL_BUILD')
                }
            }
        }

        stage('Component Tests') {
            steps {
                script {
                    echo 'Running component tests...'
                    sh """
                        dotnet test ${SOLUTION_FILE} \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-build \
                            --verbosity normal \
                            --logger 'trx;LogFileName=component-test-results.trx' \
                            --filter 'Category=ComponentTests' \
                            --results-directory:./TestResults/ComponentTests
                    """
                }
            }
            post {
                always {
                    publishTestResults testResultsPattern: '**/component-test-results.trx'
                }
            }
        }

        stage('Service Tests') {
            steps {
                script {
                    echo 'Running service tests...'
                    sh """
                        dotnet test ${SOLUTION_FILE} \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-build \
                            --verbosity normal \
                            --logger 'trx;LogFileName=service-test-results.trx' \
                            --filter 'Category=ServiceTests' \
                            --results-directory:./TestResults/ServiceTests
                    """
                }
            }
            post {
                always {
                    publishTestResults testResultsPattern: '**/service-test-results.trx'
                }
            }
        }

        stage('Code Analysis') {
            steps {
                script {
                    echo 'Running code analysis...'
                    sh """
                        dotnet build ${SOLUTION_FILE} \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-incremental \
                            /p:RunAnalyzers=true \
                            /p:CodeAnalysisRuleSet=Build/Code\\ Analysis/Code\\ Analysis\\ Rules.ruleset
                    """
                }
            }
        }

        stage('Publish Artifacts') {
            steps {
                script {
                    echo 'Publishing build artifacts...'
                    sh """
                        dotnet publish ${SOLUTION_FILE} \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-build \
                            --output ./artifacts/publish
                    """
                }
            }
            post {
                success {
                    archiveArtifacts artifacts: 'artifacts/publish/**/*', allowEmptyArchive: true
                }
            }
        }

        stage('Build Docker Images') {
            when {
                anyOf {
                    branch 'main'
                    branch 'develop'
                    branch 'master'
                }
            }
            steps {
                script {
                    echo 'Docker image building is optional. Configure DOCKER_REGISTRY_URL to enable.'
                    echo 'To build Docker images, uncomment the buildDockerImage calls below:'
                    // Uncomment and configure these when Docker registry is available:
                    /*
                    buildDockerImage('retail-customers-service', 'Source/Retail.Customers/Retail.Customers.Service')
                    buildDockerImage('retail-products-service', 'Source/Retail.Products/Retail.Products.Service')
                    buildDockerImage('retail-orders-write-service', 'Source/Retail.Orders.Write/Retail.Orders.Write.Service')
                    buildDockerImage('retail-orders-read-service', 'Source/Retail.Orders.Read/Retail.Orders.Read.Service')
                    buildDockerImage('retail-bff', 'Source/Retail.BFF')
                    buildDockerImage('retail-ui', 'Source/Retail.UI')
                    */
                }
            }
        }

        stage('Publish NuGet Packages') {
            when {
                anyOf {
                    branch 'main'
                    branch 'master'
                }
            }
            steps {
                script {
                    echo 'Publishing Contracts packages...'
                    sh """
                        dotnet pack Source/Retail.Customers/Retail.Customers.Contracts/Retail.Customers.Contracts.csproj \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-build \
                            --output ./artifacts/packages
                        
                        dotnet pack Source/Retail.Products/Retail.Products.Contracts/Retail.Products.Contracts.csproj \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-build \
                            --output ./artifacts/packages
                        
                        dotnet pack Source/Retail.Orders.Read/Retail.Orders.Read.Contracts/Retail.Orders.Read.Contracts.csproj \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-build \
                            --output ./artifacts/packages
                        
                        dotnet pack Source/Retail.Orders.Write/Retail.Orders.Write.Contracts/Retail.Orders.Write.Contracts.csproj \
                            --configuration ${BUILD_CONFIGURATION} \
                            --no-build \
                            --output ./artifacts/packages
                    """
                }
            }
            post {
                success {
                    archiveArtifacts artifacts: 'artifacts/packages/**/*.nupkg', allowEmptyArchive: true
                }
            }
        }
    }

    post {
        always {
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
            emailext(
                subject: "Build Failed: ${env.JOB_NAME} #${env.BUILD_NUMBER}",
                body: "Build ${env.BUILD_NUMBER} of ${env.JOB_NAME} has failed.\n\nCheck console output at ${env.BUILD_URL}",
                to: "${env.CHANGE_AUTHOR_EMAIL ?: 'devops@yourcompany.com'}"
            )
        }
        unstable {
            echo 'Pipeline is unstable!'
        }
    }
}

/**
 * Helper function to build Docker images for services
 * Uncomment and use when Docker registry is configured
 */
def buildDockerImage(String imageName, String dockerfilePath) {
    if (!env.DOCKER_REGISTRY_URL) {
        echo "DOCKER_REGISTRY_URL not configured. Skipping Docker build for ${imageName}"
        return
    }
    
    def imageTag = "${DOCKER_REGISTRY_URL}/${imageName}:${BUILD_VERSION}"
    def latestTag = "${DOCKER_REGISTRY_URL}/${imageName}:latest"
    
    echo "Building Docker image: ${imageName}"
    
    sh """
        docker build \
            -t ${imageTag} \
            -t ${latestTag} \
            -f ${dockerfilePath}/Dockerfile \
            --build-arg BUILD_CONFIGURATION=${BUILD_CONFIGURATION} \
            ${dockerfilePath}
    """
    
    sh """
        docker push ${imageTag}
        docker push ${latestTag}
    """
    
    echo "Successfully built and pushed: ${imageTag}"
}

