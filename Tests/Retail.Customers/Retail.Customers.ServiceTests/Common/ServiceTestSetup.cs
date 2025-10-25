using BoDi;
using TechTalk.SpecFlow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using CommonLibrary.MessageContract;

namespace Retail.Customers.ServiceTests.Common
{
    /// <summary>
    /// Configures ServiceTestSetup bindings for Customer Service Tests.
    /// </summary>
    [Binding]
    public class ServiceTestSetup
    {
        private readonly IObjectContainer objectContainer;
        private readonly ScenarioContext scenarioContext;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ServiceTestSetup> logger;

        public ServiceTestSetup(
            IObjectContainer objectContainer,
            ScenarioContext scenarioContext)
        {
            this.objectContainer = objectContainer;
            this.scenarioContext = scenarioContext;
            this.serviceProvider = TestConfiguration.CreateTestServiceProvider();
            this.logger = serviceProvider.GetRequiredService<ILogger<ServiceTestSetup>>();
        }

        /// <summary>
        /// Called before every scenario.
        /// </summary>
        [BeforeScenario(Order = 0)]
        public void BeforeScenario()
        {
            logger.LogInformation("Setting up test scenario: {ScenarioTitle}", scenarioContext.ScenarioInfo.Title);
            
            // Register services in the object container
            objectContainer.RegisterInstanceAs(serviceProvider);

            // Initialize test database
            InitializeTestDatabase();
        }

        /// <summary>
        /// Cleanup after scenario.
        /// </summary>
        [AfterScenario]
        public void AfterScenario()
        {
            logger.LogInformation("Cleaning up test scenario: {ScenarioTitle}", scenarioContext.ScenarioInfo.Title);
            
            // Clear scenario context
            scenarioContext.Clear();
            
            // Clean up test database
            CleanupTestDatabase();
        }

        /// <summary>
        /// Set up before test run.
        /// </summary>
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Global test setup if needed
        }

        /// <summary>
        /// Cleanup after test run.
        /// </summary>
        [AfterTestRun]
        public static void AfterTestRun()
        {
            // Global test cleanup if needed
        }

        private void InitializeTestDatabase()
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure database is created
            dbContext.Database.EnsureCreated();
            
            // Seed test data if needed
            SeedTestData(dbContext);
        }

        private void CleanupTestDatabase()
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Clean up all data
            dbContext.Customers.RemoveRange(dbContext.Customers);
            dbContext.Notifications.RemoveRange(dbContext.Notifications);
            dbContext.SaveChanges();
        }

        private void SeedTestData(ApplicationDbContext dbContext)
        {
            // Add test customers if they don't exist
            if (!dbContext.Customers.Any())
            {
                var testCustomer = new Customer
                {
                    FirstName = Constants.TestCustomerFirstName,
                    LastName = Constants.TestCustomerLastName
                };

                dbContext.Customers.Add(testCustomer);
                dbContext.SaveChanges();
            }
        }


    }

    /// <summary>
    /// Helper extensions for scenario context.
    /// </summary>
    public static class ScenarioContextExtensions
    {
        public static void SetCustomerData(this ScenarioContext context, CustomerDto customer)
        {
            context.Set(customer, Constants.CustomerData);
        }

        public static CustomerDto GetCustomerData(this ScenarioContext context)
        {
            return context.Get<CustomerDto>(Constants.CustomerData);
        }

        public static void SetCustomerServiceResponse(this ScenarioContext context, object response)
        {
            context.Set(response, Constants.CustomerServiceResponse);
        }

        public static T GetCustomerServiceResponse<T>(this ScenarioContext context)
        {
            return context.Get<T>(Constants.CustomerServiceResponse);
        }

        public static void SetTestResult(this ScenarioContext context, object result)
        {
            context.Set(result, Constants.TestResult);
        }

        public static T GetTestResult<T>(this ScenarioContext context)
        {
            return context.Get<T>(Constants.TestResult);
        }
    }
}
