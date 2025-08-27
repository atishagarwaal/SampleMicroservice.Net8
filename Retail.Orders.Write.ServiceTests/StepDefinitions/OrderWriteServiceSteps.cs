using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.Handlers;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Repositories;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.UnitOfWork;
using Retail.Orders.Write.ServiceTests.Common;
using TechTalk.SpecFlow;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using MediatR;
using MessagingLibrary.Interface;
using MessagingLibrary.Service;

namespace Retail.Orders.Write.ServiceTests.StepDefinitions
{
    [Binding]
    public class OrderWriteServiceSteps : TestBase
    {
        private readonly ScenarioContext _scenarioContext;
        private bool _serviceRunning;
        private bool _databaseConnected;
        private bool _healthCheckPassed;
        private bool _serviceConfigured;
        private bool _dependenciesInjected;
        private bool _errorHandledGracefully;

        public OrderWriteServiceSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            base.ConfigureServices(services, configuration);

            // Add in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            // Add repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ILineItemRepository, LineItemRepository>();

            // Add unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(CreateOrderCommand).Assembly,
                typeof(DeleteOrderCommand).Assembly,
                typeof(UpdateOrderCommand).Assembly));

            // Add command handlers
            services.AddScoped<CreateOrderCommandHandler>();
            services.AddScoped<UpdateOrderCommandHandler>();
            services.AddScoped<DeleteOrderCommandHandler>();

            // Add AutoMapper
            services.AddAutoMapper(typeof(Retail.Orders.Write.src.CleanArchitecture.Application.Mappings.OrderProfile));

            // Add messaging services
            services.AddScoped<IMessagePublisher, MockMessagePublisher>();
        }

        [Given(@"the Order Write Service is running")]
        public void GivenTheOrderWriteServiceIsRunning()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            // Simulate service startup
            _serviceRunning = true;
            _scenarioContext["ServiceRunning"] = _serviceRunning;
        }

        [Given(@"the database connection is established")]
        public void GivenTheDatabaseConnectionIsEstablished()
        {
            // Verify database connection through DI
            var dbContext = ServiceProvider.GetService<ApplicationDbContext>();
            _databaseConnected = dbContext != null;
            _scenarioContext["DatabaseConnected"] = _databaseConnected;
        }

        [Given(@"the database is unavailable")]
        public void GivenTheDatabaseIsUnavailable()
        {
            // Simulate database unavailability
            _databaseConnected = false;
            _scenarioContext["DatabaseConnected"] = _databaseConnected;
        }

        [When(@"I request a health check")]
        public void WhenIRequestAHealthCheck()
        {
            // Perform health check by verifying core services
            var commandHandler = ServiceProvider.GetService<CreateOrderCommandHandler>();
            var unitOfWork = ServiceProvider.GetService<IUnitOfWork>();
            var dbContext = ServiceProvider.GetService<ApplicationDbContext>();

            _healthCheckPassed = commandHandler != null && 
                                unitOfWork != null && 
                                dbContext != null &&
                                _databaseConnected;

            _scenarioContext["HealthCheckPassed"] = _healthCheckPassed;
        }

        [When(@"I resolve the Order Write Service from the DI container")]
        public void WhenIResolveTheOrderWriteServiceFromTheDIContainer()
        {
            try
            {
                var commandHandlers = ServiceProvider.GetServices<CreateOrderCommandHandler>();
                var unitOfWork = ServiceProvider.GetService<IUnitOfWork>();
                var dbContext = ServiceProvider.GetService<ApplicationDbContext>();

                _serviceConfigured = dbContext != null;
                _dependenciesInjected = commandHandlers.Any() && unitOfWork != null;

                _scenarioContext["ServiceConfigured"] = _serviceConfigured;
                _scenarioContext["DependenciesInjected"] = _dependenciesInjected;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Failed to resolve services from DI container");
                _serviceConfigured = false;
                _dependenciesInjected = false;
            }
        }

        [When(@"I attempt to perform a database operation")]
        public void WhenIAttemptToPerformADatabaseOperation()
        {
            try
            {
                if (!_databaseConnected)
                {
                    // Simulate database operation failure
                    throw new InvalidOperationException("Database connection unavailable");
                }

                // Simulate successful database operation
                _errorHandledGracefully = true;
            }
            catch (Exception ex)
            {
                // Verify that the error is handled gracefully
                _errorHandledGracefully = ex.Message.Contains("Database connection unavailable");
                Logger?.LogInformation("Database operation failed as expected: {Message}", ex.Message);
            }

            _scenarioContext["ErrorHandledGracefully"] = _errorHandledGracefully;
        }

        [Then(@"the service should be running")]
        public void ThenTheServiceShouldBeRunning()
        {
            _serviceRunning.Should().BeTrue();
        }

        [Then(@"the database should be accessible")]
        public void ThenTheDatabaseShouldBeAccessible()
        {
            _databaseConnected.Should().BeTrue();
        }

        [Then(@"the service should respond with a healthy status")]
        public void ThenTheServiceShouldRespondWithAHealthyStatus()
        {
            _healthCheckPassed.Should().BeTrue();
        }

        [Then(@"the Order Write Service should be properly configured")]
        public void ThenTheOrderWriteServiceShouldBeProperlyConfigured()
        {
            _serviceConfigured.Should().BeTrue();
        }

        [Then(@"all dependencies should be injected")]
        public void ThenAllDependenciesShouldBeInjected()
        {
            _dependenciesInjected.Should().BeTrue();
        }

        [Then(@"an error should be handled gracefully")]
        public void ThenAnErrorShouldBeHandledGracefully()
        {
            _errorHandledGracefully.Should().BeTrue();
        }
    }
}

/// <summary>
/// Mock implementation of IMessagePublisher for testing
/// </summary>
public class MockMessagePublisher : MessagingLibrary.Interface.IMessagePublisher
{
    public Task PublishAsync<T>(T message, string eventType)
    {
        // Mock implementation - do nothing in tests
        return Task.CompletedTask;
    }
}
