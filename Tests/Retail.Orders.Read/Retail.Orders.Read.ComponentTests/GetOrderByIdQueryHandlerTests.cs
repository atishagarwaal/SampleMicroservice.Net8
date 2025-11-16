using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    /// Unit tests for GetOrderByIdQueryHandler class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class GetOrderByIdQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IServiceScope> _mockServiceScope = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IConverter<Order, OrderDto>> _mockOrderDtoConverter = null!;
        private GetOrderByIdQueryHandler _handler = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockOrderDtoConverter = new Mock<IConverter<Order, OrderDto>>();

            _mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IUnitOfWork)))
                .Returns(_mockUnitOfWork.Object);

            _handler = new GetOrderByIdQueryHandler(
                _mockUnitOfWork.Object,
                _mockOrderDtoConverter.Object,
                _mockServiceScopeFactory.Object);
        }

        [TestMethod]
        [TestCategory("GetOrderByIdQueryHandler")]
        public void GetOrderByIdQueryHandler_Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _handler.Should().NotBeNull();
            _handler.Should().BeOfType<GetOrderByIdQueryHandler>();
        }

        [TestMethod]
        [TestCategory("GetOrderByIdQueryHandler")]
        public async Task Handle_WithValidId_ReturnsMappedOrder()
        {
            // Arrange
            var orderId = 1L;
            var order = new Order { Id = orderId, CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };
            var orderDto = new OrderDto { Id = orderId, CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };

            _mockUnitOfWork
                .Setup(x => x.Orders.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            _mockOrderDtoConverter
                .Setup(x => x.Convert(order))
                .Returns(orderDto);

            var query = new GetOrderByIdQuery { Id = orderId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(orderDto);

            _mockServiceScopeFactory.Verify(x => x.CreateScope(), Times.Once);
            _mockUnitOfWork.Verify(x => x.Orders.GetByIdAsync(orderId), Times.Once);
            _mockOrderDtoConverter.Verify(x => x.Convert(order), Times.Once);
        }

        [TestMethod]
        [TestCategory("GetOrderByIdQueryHandler")]
        public async Task Handle_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var orderId = 999L;

            _mockUnitOfWork
                .Setup(x => x.Orders.GetByIdAsync(orderId))
                .ReturnsAsync((Order?)null);

            var query = new GetOrderByIdQuery { Id = orderId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();

            _mockServiceScopeFactory.Verify(x => x.CreateScope(), Times.Once);
            _mockUnitOfWork.Verify(x => x.Orders.GetByIdAsync(orderId), Times.Once);
            _mockOrderDtoConverter.Verify(x => x.Convert(It.IsAny<Order>()), Times.Never);
        }
    }
}

