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

namespace Retail.BFF.UnitTests
{
    /// <summary>
    /// Unit tests for OrderProvider class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("OrderProvider")]
    public sealed class OrderProviderTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory = null!;
        private Mock<IOptions<OrderServiceConfig>> _mockServiceConfig = null!;
        private Mock<ILogger<OrderProvider>> _mockLogger = null!;
        private Mock<IMetricsService> _mockMetrics = null!;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
        private OrderProvider _orderProvider = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockServiceConfig = new Mock<IOptions<OrderServiceConfig>>();
            _mockLogger = new Mock<ILogger<OrderProvider>>();
            _mockMetrics = new Mock<IMetricsService>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var serviceConfig = new OrderServiceConfig
            {
                BaseUrl = "http://localhost:5003",
                Endpoints = new OrderEndpoints
                {
                    GetAllOrdersV1 = "/api/v1/orders",
                    GetOrderByIdV1 = "/api/v1/orders"
                }
            };

            _mockServiceConfig.Setup(x => x.Value).Returns(serviceConfig);

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(string.Empty)).Returns(httpClient);

            // Setup metrics mock
            _mockMetrics.Setup(x => x.TrackDuration(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(Mock.Of<IDisposable>());

            _orderProvider = new OrderProvider(
                _mockHttpClientFactory.Object,
                _mockServiceConfig.Object,
                _mockLogger.Object,
                _mockMetrics.Object);
        }

        [TestMethod]
        public void OrderProvider_Constructor_CreatesInstance()
        {
            // Act & Assert
            _orderProvider.Should().NotBeNull();
        }

        [TestMethod]
        public void OrderProvider_Constructor_WithNullServiceConfig_ThrowsArgumentNullException()
        {
            // Act & Assert
            Action act = () => new OrderProvider(
                _mockHttpClientFactory.Object,
                null!,
                _mockLogger.Object,
                _mockMetrics.Object);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public async Task GetAllOrdersAsync_WithSuccessResponse_ReturnsOrders()
        {
            // Arrange
            var orders = new[]
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>()
                }
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
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(orders))
                });

            // Act
            var result = await _orderProvider.GetAllOrdersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [TestMethod]
        public async Task GetAllOrdersAsync_WithNonSuccessResponse_ReturnsEmptyList()
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
            var result = await _orderProvider.GetAllOrdersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetAllOrdersAsync_WhenExceptionOccurs_ThrowsException()
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
                async () => await _orderProvider.GetAllOrdersAsync());
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_WithSuccessResponse_ReturnsOrder()
        {
            // Arrange
            var order = new OrderDto
            {
                Id = 1,
                CustomerId = 1,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99,
                LineItems = new List<LineItemDto>()
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
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(order))
                });

            // Act
            var result = await _orderProvider.GetOrderByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.CustomerId.Should().Be(1);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_WhenExceptionOccurs_ThrowsException()
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
                async () => await _orderProvider.GetOrderByIdAsync(1));
        }
    }
}

