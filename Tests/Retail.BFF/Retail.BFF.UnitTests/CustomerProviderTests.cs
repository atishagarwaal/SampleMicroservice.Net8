using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Retail.BFFWeb.Api.Configurations;
using Retail.BFFWeb.Api.Interface;
using Retail.BFFWeb.Api.Model;
using Retail.BFFWeb.Api.Provider;

namespace Retail.BFF.UnitTests
{
    /// <summary>
    /// Unit tests for CustomerProvider class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("CustomerProvider")]
    public sealed class CustomerProviderTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory = null!;
        private Mock<IOptions<CustomerServiceConfig>> _mockServiceConfig = null!;
        private Mock<ILogger<CustomerProvider>> _mockLogger = null!;
        private Mock<IMetricsService> _mockMetrics = null!;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
        private CustomerProvider _customerProvider = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockServiceConfig = new Mock<IOptions<CustomerServiceConfig>>();
            _mockLogger = new Mock<ILogger<CustomerProvider>>();
            _mockMetrics = new Mock<IMetricsService>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var serviceConfig = new CustomerServiceConfig
            {
                BaseUrl = "http://localhost:5002",
                Endpoints = new CustomerEndpoints
                {
                    GetAllCustomersV1 = "/api/v1/customers",
                    GetCustomerByIdV1 = "/api/v1/customers/{id}"
                }
            };

            _mockServiceConfig.Setup(x => x.Value).Returns(serviceConfig);

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(string.Empty)).Returns(httpClient);

            // Setup metrics mock
            _mockMetrics.Setup(x => x.TrackDuration(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(Mock.Of<IDisposable>());

            _customerProvider = new CustomerProvider(
                _mockHttpClientFactory.Object,
                _mockServiceConfig.Object,
                _mockLogger.Object,
                _mockMetrics.Object);
        }

        [TestMethod]
        public void CustomerProvider_Constructor_CreatesInstance()
        {
            // Act & Assert
            _customerProvider.Should().NotBeNull();
        }

        [TestMethod]
        public void CustomerProvider_Constructor_WithNullServiceConfig_ThrowsArgumentNullException()
        {
            // Act & Assert
            Action act = () => new CustomerProvider(
                _mockHttpClientFactory.Object,
                null!,
                _mockLogger.Object,
                _mockMetrics.Object);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public async Task GetAllCustomersAsync_WithSuccessResponse_ReturnsCustomers()
        {
            // Arrange
            var customers = new[]
            {
                new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" },
                new CustomerDto { Id = 2, FirstName = "Jane", LastName = "Smith" }
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(customers))
                });

            // Act
            var result = await _customerProvider.GetAllCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [TestMethod]
        public async Task GetAllCustomersAsync_WithNonSuccessResponse_ReturnsEmptyList()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await _customerProvider.GetAllCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetAllCustomersAsync_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await _customerProvider.GetAllCustomersAsync());
        }

        [TestMethod]
        public async Task GetCustomerByIdAsync_WithSuccessResponse_ReturnsCustomer()
        {
            // Arrange
            var customer = new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(customer))
                });

            // Act
            var result = await _customerProvider.GetCustomerByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
        }

        [TestMethod]
        public async Task GetCustomerByIdAsync_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await _customerProvider.GetCustomerByIdAsync(1));
        }
    }
}

