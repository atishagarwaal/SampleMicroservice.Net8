using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary.Results;
using CommonLibrary.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Application.Handlers;
using Retail.Orders.Read.src.CleanArchitecture.Application.Queries;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for GetAllOrdersQueryHandler class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class GetAllOrdersQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IServiceScope> _mockServiceScope = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IConverter<Order, OrderDto>> _mockOrderDtoConverter = null!;
        private Mock<ILogger<GetAllOrdersQueryHandler>> _mockLogger = null!;
        private Mock<IMetricsService> _mockMetrics = null!;
        private GetAllOrdersQueryHandler _handler = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockOrderDtoConverter = new Mock<IConverter<Order, OrderDto>>();
            _mockLogger = new Mock<ILogger<GetAllOrdersQueryHandler>>();
            _mockMetrics = new Mock<IMetricsService>();

            _mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IUnitOfWork)))
                .Returns(_mockUnitOfWork.Object);

            // Setup metrics mock
            _mockMetrics.Setup(x => x.TrackDuration(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(Mock.Of<IDisposable>());

            _handler = new GetAllOrdersQueryHandler(
                _mockUnitOfWork.Object,
                _mockOrderDtoConverter.Object,
                _mockServiceScopeFactory.Object,
                _mockLogger.Object,
                _mockMetrics.Object);
        }

        [TestMethod]
        [TestCategory("GetAllOrdersQueryHandler")]
        public void GetAllOrdersQueryHandler_Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _handler.Should().NotBeNull();
            _handler.Should().BeOfType<GetAllOrdersQueryHandler>();
        }

        [TestMethod]
        [TestCategory("GetAllOrdersQueryHandler")]
        public async Task Handle_WithValidData_ReturnsMappedOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 },
                new Order { Id = 2, CustomerId = 101, OrderDate = DateTime.Now, TotalAmount = 75.50 }
            };

            var orderDtos = new List<OrderDto>
            {
                new OrderDto { Id = 1, CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 },
                new OrderDto { Id = 2, CustomerId = 101, OrderDate = DateTime.Now, TotalAmount = 75.50 }
            };

            _mockUnitOfWork
                .Setup(x => x.Orders.GetAllAsync())
                .ReturnsAsync(orders);

            _mockOrderDtoConverter
                .Setup(x => x.Convert(It.IsAny<Order>()))
                .Returns<Order>(order => orderDtos.First(dto => dto.Id == order.Id));

            var query = new GetAllOrdersQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            var resultList = result.Value.ToList();
            resultList.Should().HaveCount(2);
            resultList.Should().BeEquivalentTo(orderDtos);
            resultList[0].Id.Should().Be(1);
            resultList[1].Id.Should().Be(2);
        }

        [TestMethod]
        [TestCategory("GetAllOrdersQueryHandler")]
        public async Task Handle_WithEmptyData_ReturnsEmptyList()
        {
            // Arrange
            var orders = new List<Order>();

            _mockUnitOfWork
                .Setup(x => x.Orders.GetAllAsync())
                .ReturnsAsync(orders);

            var query = new GetAllOrdersQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            var resultList = result.Value.ToList();
            resultList.Should().BeEmpty();
        }
    }
}

