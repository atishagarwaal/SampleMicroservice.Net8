using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InventoryUpdatedEventNameSpace;
using Retail.Api.Customers.src.CleanArchitecture.Application.EventHandlers;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using MessagingLibrary.Interface;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for InventoryUpdatedEventHandler class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("InventoryUpdatedEventHandler")]
    public sealed class InventoryUpdatedEventHandlerTests
    {
        private Mock<ICustomerService> _mockCustomerService = null!;
        private Mock<IMessagePublisher> _mockMessagePublisher = null!;
        private Mock<ILogger<InventoryUpdatedEventHandler>> _mockLogger = null!;
        private InventoryUpdatedEventHandler _handler = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockMessagePublisher = new Mock<IMessagePublisher>();
            _mockLogger = new Mock<ILogger<InventoryUpdatedEventHandler>>();
            _handler = new InventoryUpdatedEventHandler(
                _mockCustomerService.Object,
                _mockMessagePublisher.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task HandleAsync_WithNullEvent_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _handler.HandleAsync(null!));
        }

        [TestMethod]
        public async Task HandleAsync_WithValidEvent_CompletesSuccessfully()
        {
            // Arrange
            var inventoryUpdatedEvent = new InventoryUpdatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = 100,
                LineItems = Array.Empty<LineItem>()
            };

            _mockCustomerService
                .Setup(x => x.HandleOrderCreatedEvent(It.IsAny<InventoryUpdatedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _handler.HandleAsync(inventoryUpdatedEvent);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task HandleAsync_WhenCustomerServiceThrowsException_PropagatesException()
        {
            // Arrange
            var inventoryUpdatedEvent = new InventoryUpdatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = 100,
                LineItems = Array.Empty<LineItem>()
            };

            var expectedException = new InvalidOperationException("Service error");
            _mockCustomerService
                .Setup(x => x.HandleOrderCreatedEvent(It.IsAny<InventoryUpdatedEvent>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _handler.HandleAsync(inventoryUpdatedEvent));

            exception.Should().Be(expectedException);
            exception.Message.Should().Be("Service error");
        }

        [TestMethod]
        public async Task HandleAsync_WithEventContainingLineItems_ProcessesSuccessfully()
        {
            // Arrange
            var lineItems = new[]
            {
                new LineItem { SkuId = 1, Qty = 2 },
                new LineItem { SkuId = 2, Qty = 3 }
            };

            var inventoryUpdatedEvent = new InventoryUpdatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = 100,
                LineItems = lineItems
            };

            _mockCustomerService
                .Setup(x => x.HandleOrderCreatedEvent(It.IsAny<InventoryUpdatedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _handler.HandleAsync(inventoryUpdatedEvent);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}

