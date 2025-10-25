using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Service;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
using MessagingLibrary.Interface;

namespace Retail.Products.ServiceTests.Common
{
    /// <summary>
    /// Base class for service tests.
    /// </summary>
    public abstract class TestBase
    {
        protected IServiceProvider ServiceProvider { get; private set; } = null!;
        protected Mock<IUnitOfWork> MockUnitOfWork { get; private set; } = null!;
        protected Mock<IMessagePublisher> MockMessagePublisher { get; private set; } = null!;

        protected virtual void SetupServices()
        {
            var services = new ServiceCollection();

            // Add configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: false)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add mocks
            MockUnitOfWork = new Mock<IUnitOfWork>();
            MockMessagePublisher = new Mock<IMessagePublisher>();

            services.AddSingleton(MockUnitOfWork.Object);
            services.AddSingleton(MockMessagePublisher.Object);

            // Add AutoMapper with a mock implementation
            var mockMapper = new Mock<AutoMapper.IMapper>();
            services.AddSingleton(mockMapper.Object);

            // Add mock IServiceScopeFactory
            var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            services.AddSingleton(mockServiceScopeFactory.Object);

            // Note: We're not registering ProductService in DI because it has complex dependencies
            // Instead, we'll create it manually in the step definitions with mocked dependencies

            ServiceProvider = services.BuildServiceProvider();
        }

        protected virtual void Cleanup()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
