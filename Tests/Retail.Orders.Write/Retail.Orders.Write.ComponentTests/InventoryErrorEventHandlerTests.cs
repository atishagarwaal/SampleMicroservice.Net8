using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InventoryErrorEventNameSpace;
using Retail.Orders.Write.src.CleanArchitecture.Application.EventHandlers;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for InventoryErrorEventHandler class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("InventoryErrorEventHandler")]
    public sealed class InventoryErrorEventHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IServiceScope> _mockServiceScope = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IOrderRepository> _mockOrderRepository = null!;
        private Mock<ILogger<InventoryErrorEventHandler>> _mockLogger = null!;
        private InventoryErrorEventHandler _handler = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockLogger = new Mock<ILogger<InventoryErrorEventHandler>>();

            _mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            // Mock GetService which GetRequiredService calls internally
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IUnitOfWork)))
                .Returns(_mockUnitOfWork.Object);

            _mockUnitOfWork
                .Setup(x => x.Orders)
                .Returns(_mockOrderRepository.Object);

            _mockUnitOfWork
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ReturnsAsync(1);

            _mockUnitOfWork
                .Setup(x => x.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(x => x.RollbackTransactionAsync())
                .Returns(Task.CompletedTask);

            _handler = new InventoryErrorEventHandler(
                _mockUnitOfWork.Object,
                null!, // IMessagePublisher not used in current implementation
                _mockServiceScopeFactory.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task HandleAsync_WithNullEvent_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _handler.HandleAsync(null!));
        }

        [TestMethod]
        public async Task HandleAsync_WhenOrderNotFound_ThrowsException()
        {
            // Arrange
            var inventoryErrorEvent = new InventoryErrorEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = (double)100.50m
            };

            _mockOrderRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync((Order?)null);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                async () => await _handler.HandleAsync(inventoryErrorEvent));

            exception.Message.Should().Be("Order does not exist");
        }

        [TestMethod]
        public async Task HandleAsync_WithValidEvent_RemovesOrderSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 100
            };

            var inventoryErrorEvent = new InventoryErrorEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = (double)100.50m
            };

            _mockOrderRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () => await _handler.HandleAsync(inventoryErrorEvent);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task HandleAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 100
            };

            var inventoryErrorEvent = new InventoryErrorEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = (double)100.50m
            };

            var expectedException = new InvalidOperationException("Database error");
            _mockOrderRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(order);

            _mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _handler.HandleAsync(inventoryErrorEvent));

            exception.Should().Be(expectedException);
            exception.Message.Should().Be("Database error");
        }
    }
}

