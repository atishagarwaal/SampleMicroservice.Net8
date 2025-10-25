using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using FluentAssertions;

namespace Retail.Orders.Write.ServiceTests.Common
{
    /// <summary>
    /// Base class for all service tests.
    /// </summary>
    public abstract class TestBase
    {
        protected IServiceProvider ServiceProvider { get; private set; } = null!;
        protected IConfiguration Configuration { get; private set; } = null!;
        protected ILogger<TestBase> Logger { get; private set; } = null!;

        /// <summary>
        /// Initialize the test base. This should be called from derived classes.
        /// </summary>
        protected virtual void Initialize()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Configuration = configuration;
            ConfigureServices(services, configuration);
            ServiceProvider = services.BuildServiceProvider();
            Logger = ServiceProvider.GetRequiredService<ILogger<TestBase>>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// Configure services for testing.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        protected virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Add configuration
            services.AddSingleton(configuration);
        }

        /// <summary>
        /// Assert that a service is registered and can be resolved.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        protected void AssertServiceIsRegistered<T>() where T : class
        {
            var service = ServiceProvider.GetService<T>();
            service.Should().NotBeNull();
        }

        /// <summary>
        /// Assert that a service is registered and can be resolved as singleton.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        protected void AssertServiceIsSingleton<T>() where T : class
        {
            var service1 = ServiceProvider.GetService<T>();
            var service2 = ServiceProvider.GetService<T>();
            service1.Should().BeSameAs(service2);
        }
    }
}
