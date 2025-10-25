using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Retail.BFFWeb.Api.Interface;

namespace Retail.BFF.ServiceTests.Common
{
    public abstract class TestBase
    {
        protected IServiceProvider ServiceProvider { get; private set; } = null!;
        protected Mock<IProductProvider> MockProductProvider { get; private set; } = null!;
        protected Mock<ICustomerProvider> MockCustomerProvider { get; private set; } = null!;
        protected Mock<IOrderProvider> MockOrderProvider { get; private set; } = null!;

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
            MockProductProvider = new Mock<IProductProvider>();
            MockCustomerProvider = new Mock<ICustomerProvider>();
            MockOrderProvider = new Mock<IOrderProvider>();

            // Register mocks as services
            services.AddSingleton(MockProductProvider.Object);
            services.AddSingleton(MockCustomerProvider.Object);
            services.AddSingleton(MockOrderProvider.Object);

            // Build service provider
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
