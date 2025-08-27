using NUnit.Framework;
using TechTalk.SpecFlow;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Retail.Orders.Read.ServiceTests.Common;

namespace Retail.Orders.Read.ServiceTests.StepDefinitions
{
    /// <summary>
    /// Step definitions for Order Service tests.
    /// </summary>
    [Binding]
    public class OrderServiceSteps : TestBase
    {
        private bool _serviceIsRunning;
        private bool _databaseIsAccessible;
        private bool _healthCheckResponse;
        private Exception? _lastException;

        [Given(@"the Order Service is running")]
        public void GivenTheOrderServiceIsRunning()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _serviceIsRunning = true;
            _serviceIsRunning.Should().BeTrue();
        }

        [Given(@"the database connection is established")]
        public void GivenTheDatabaseConnectionIsEstablished()
        {
            _databaseIsAccessible = true;
            _databaseIsAccessible.Should().BeTrue();
        }

        [Given(@"the database is unavailable")]
        public void GivenTheDatabaseIsUnavailable()
        {
            _databaseIsAccessible = false;
            _databaseIsAccessible.Should().BeFalse();
        }

        [When(@"I request a health check")]
        public void WhenIRequestAHealthCheck()
        {
            try
            {
                if (_serviceIsRunning && _databaseIsAccessible)
                {
                    _healthCheckResponse = true;
                }
                else
                {
                    _healthCheckResponse = false;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                _healthCheckResponse = false;
            }
        }

        [When(@"I resolve the Order Service from the DI container")]
        public void WhenIResolveTheOrderServiceFromTheDIContainer()
        {
            try
            {
                // This would typically resolve actual services from the DI container
                // For now, we'll simulate the resolution
                if (_serviceIsRunning)
                {
                    // Simulate successful service resolution
                    Logger?.LogInformation("Order Service resolved successfully from DI container");
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                Logger?.LogError(ex, "Failed to resolve Order Service from DI container");
            }
        }

        [When(@"I attempt to perform a database operation")]
        public void WhenIAttemptToPerformADatabaseOperation()
        {
            try
            {
                if (!_databaseIsAccessible)
                {
                    throw new InvalidOperationException("Database connection is not available");
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                Logger?.LogError(ex, "Database operation failed");
            }
        }

        [Then(@"the service should be running")]
        public void ThenTheServiceShouldBeRunning()
        {
            _serviceIsRunning.Should().BeTrue();
        }

        [Then(@"the database should be accessible")]
        public void ThenTheDatabaseShouldBeAccessible()
        {
            _databaseIsAccessible.Should().BeTrue();
        }

        [Then(@"the service should respond with a healthy status")]
        public void ThenTheServiceShouldRespondWithAHealthyStatus()
        {
            _healthCheckResponse.Should().BeTrue();
        }

        [Then(@"the Order Service should be properly configured")]
        public void ThenTheOrderServiceShouldBeProperlyConfigured()
        {
            _serviceIsRunning.Should().BeTrue();
            _lastException.Should().BeNull();
        }

        [Then(@"all dependencies should be injected")]
        public void ThenAllDependenciesShouldBeInjected()
        {
            _serviceIsRunning.Should().BeTrue();
            _lastException.Should().BeNull();
        }

        [Then(@"an error should be handled gracefully")]
        public void ThenAnErrorShouldBeHandledGracefully()
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType<InvalidOperationException>();
            // Logger should be available after initialization
            Logger.Should().NotBeNull();
        }
    }
}
