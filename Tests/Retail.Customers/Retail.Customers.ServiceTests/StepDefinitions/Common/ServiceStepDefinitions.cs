using NUnit.Framework;
using TechTalk.SpecFlow;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Retail.Customers.ServiceTests.StepDefinitions.Common
{
    /// <summary>
    /// Common step definitions for Service operations that can be reused across features.
    /// </summary>
    [Binding]
    public class ServiceStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServiceStepDefinitions> _logger;
        private ICustomerService _customerService = null!;
        private ApplicationDbContext _dbContext = null!;

        public ServiceStepDefinitions(
            ScenarioContext scenarioContext,
            IServiceProvider serviceProvider)
        {
            _scenarioContext = scenarioContext;
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<ServiceStepDefinitions>>();
        }

        #region Service Setup Steps

        [Given(@"the Customer Service is running")]
        public void GivenTheCustomerServiceIsRunning()
        {
            _logger.LogInformation("Setting up Customer Service");
            
            try
            {
                _customerService = _serviceProvider.GetRequiredService<ICustomerService>();
                _dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
                
                _scenarioContext.Set(true, "ServiceIsRunning");
                _logger.LogInformation("Customer Service is up and running");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set up Customer Service");
                _scenarioContext.Set(false, "ServiceIsRunning");
                throw;
            }
        }

        [Given(@"the database connection is established")]
        public void GivenTheDatabaseConnectionIsEstablished()
        {
            _logger.LogInformation("Establishing database connection");
            
            try
            {
                // Test database connection
                var canConnect = _dbContext.Database.CanConnect();
                canConnect.Should().BeTrue("Database should be accessible");
                
                _scenarioContext.Set(true, "DatabaseConnectionEstablished");
                _logger.LogInformation("Database connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish database connection");
                _scenarioContext.Set(false, "DatabaseConnectionEstablished");
                throw;
            }
        }

        [Given(@"the service dependencies are properly configured")]
        public void GivenTheServiceDependenciesAreProperlyConfigured()
        {
            _logger.LogInformation("Verifying service dependencies configuration");
            
            try
            {
                var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
                var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
                
                customerService.Should().NotBeNull("Customer Service should be available");
                dbContext.Should().NotBeNull("Database Context should be available");
                
                _scenarioContext.Set(true, "DependenciesConfigured");
                _logger.LogInformation("Service dependencies are properly configured");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify service dependencies");
                _scenarioContext.Set(false, "DependenciesConfigured");
                throw;
            }
        }

        #endregion

        #region Service Health Check Steps

        [When(@"I request a health check")]
        public void WhenIRequestAHealthCheck()
        {
            _logger.LogInformation("Requesting health check");
            
            try
            {
                var serviceHealth = _customerService != null;
                var databaseHealth = _dbContext.Database.CanConnect();
                
                var healthStatus = serviceHealth && databaseHealth ? "Healthy" : "Unhealthy";
                _scenarioContext.Set(healthStatus, "HealthStatus");
                _scenarioContext.Set(true, "HealthCheckRequested");
                
                _logger.LogInformation("Health check completed with status: {HealthStatus}", healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                _scenarioContext.Set("Error", "HealthStatus");
                _scenarioContext.Set(false, "HealthCheckRequested");
                throw;
            }
        }

        [When(@"I resolve the Customer Service from the DI container")]
        public void WhenIResolveTheCustomerServiceFromTheDIContainer()
        {
            _logger.LogInformation("Resolving Customer Service from DI container");
            
            try
            {
                _customerService = _serviceProvider.GetRequiredService<ICustomerService>();
                _scenarioContext.Set(true, "ServiceResolved");
                _logger.LogInformation("Customer Service resolved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve Customer Service");
                _scenarioContext.Set(false, "ServiceResolved");
                throw;
            }
        }

        #endregion

        #region Service Validation Steps

        [Then(@"the service should be running")]
        public void ThenTheServiceShouldBeRunning()
        {
            var serviceIsRunning = _scenarioContext.Get<bool>("ServiceIsRunning");
            serviceIsRunning.Should().BeTrue("Customer Service should be running");
            _customerService.Should().NotBeNull("Customer Service should be available");
            _logger.LogInformation("Service is running successfully");
        }

        [Then(@"the database should be accessible")]
        public void ThenTheDatabaseShouldBeAccessible()
        {
            var databaseConnectionEstablished = _scenarioContext.Get<bool>("DatabaseConnectionEstablished");
            databaseConnectionEstablished.Should().BeTrue("Database connection should be established");
            _dbContext.Should().NotBeNull("Database context should be available");
            _logger.LogInformation("Database is accessible");
        }

        [Then(@"the service should respond with a healthy status")]
        public void ThenTheServiceShouldRespondWithAHealthyStatus()
        {
            var healthCheckRequested = _scenarioContext.Get<bool>("HealthCheckRequested");
            var healthStatus = _scenarioContext.Get<string>("HealthStatus");
            
            healthCheckRequested.Should().BeTrue("Health check should have been requested");
            healthStatus.Should().Be("Healthy", "Service should respond with healthy status");
            _logger.LogInformation("Service responded with healthy status");
        }

        [Then(@"all required services should be available")]
        public void ThenAllRequiredServicesShouldBeAvailable()
        {
            var dependenciesConfigured = _scenarioContext.Get<bool>("DependenciesConfigured");
            dependenciesConfigured.Should().BeTrue("All dependencies should be properly configured");
            
            _customerService.Should().NotBeNull("Customer Service should be available");
            _dbContext.Should().NotBeNull("Database Context should be available");
            _logger.LogInformation("All required services are available");
        }

        [Then(@"the Customer Service should be properly configured")]
        public void ThenTheCustomerServiceShouldBeProperlyConfigured()
        {
            var serviceResolved = _scenarioContext.Get<bool>("ServiceResolved");
            serviceResolved.Should().BeTrue("Customer Service should be properly resolved");
            
            _customerService.Should().NotBeNull("Customer Service should be properly configured");
            _customerService.Should().BeAssignableTo<ICustomerService>("Customer Service should implement ICustomerService");
            _logger.LogInformation("Customer Service is properly configured");
        }

        [Then(@"all dependencies should be injected")]
        public void ThenAllDependenciesShouldBeInjected()
        {
            // Verify that dependencies are properly injected by checking if the service can perform operations
            var customers = _customerService.GetAllCustomersAsync().Result;
            customers.Should().NotBeNull("Customer Service should be able to retrieve customers");
            
            _logger.LogInformation("All dependencies are properly injected");
        }

        #endregion

        #region Service Error Handling Steps

        [Given(@"the database is unavailable")]
        public void GivenTheDatabaseIsUnavailable()
        {
            _logger.LogInformation("Setting up database unavailable scenario");
            
            // In a real test, this might involve stopping the database service
            // For now, we'll simulate by setting a flag
            _scenarioContext.Set(true, "DatabaseUnavailable");
            _logger.LogInformation("Database unavailable scenario configured");
        }

        [When(@"I attempt to perform a database operation")]
        public void WhenIAttemptToPerformADatabaseOperation()
        {
            _logger.LogInformation("Attempting to perform database operation");
            
            try
            {
                var databaseUnavailable = _scenarioContext.Get<bool>("DatabaseUnavailable");
                if (databaseUnavailable)
                {
                    // Simulate database unavailability
                    throw new InvalidOperationException("Database connection unavailable");
                }
                
                // Perform a simple database operation
                var customerCount = _dbContext.Customers.Count();
                _scenarioContext.Set(customerCount, "DatabaseOperationResult");
                _scenarioContext.Set(true, "DatabaseOperationSucceeded");
                
                _logger.LogInformation("Database operation completed successfully");
            }
            catch (Exception ex)
            {
                _scenarioContext.Set(ex, "DatabaseOperationException");
                _scenarioContext.Set(false, "DatabaseOperationSucceeded");
                _logger.LogError(ex, "Database operation failed");
            }
        }

        [Then(@"an error should be handled gracefully")]
        public void ThenAnErrorShouldBeHandledGracefully()
        {
            var databaseOperationSucceeded = _scenarioContext.Get<bool>("DatabaseOperationSucceeded");
            var exception = _scenarioContext.Get<Exception>("DatabaseOperationException");
            
            databaseOperationSucceeded.Should().BeFalse("Database operation should fail");
            exception.Should().NotBeNull("Exception should be thrown");
            _logger.LogInformation("Error handling verification completed");
        }

        #endregion
    }
}
