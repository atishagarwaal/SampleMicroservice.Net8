using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrderCreatedEventNameSpace;
using Retail.Api.Products.src.CleanArchitecture.Application.EventHandlers;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Products.ComponentTests
{
    /// <summary>
    /// Unit tests for OrderCreatedEventHandler class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("OrderCreatedEventHandler")]
    public sealed class OrderCreatedEventHandlerTests
    {
        private Mock<IProductService> _mockProductService = null!;
        private Mock<ILogger<OrderCreatedEventHandler>> _mockLogger = null!;
        private OrderCreatedEventHandler _handler = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<OrderCreatedEventHandler>>();
            _handler = new OrderCreatedEventHandler(_mockProductService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task HandleAsync_WithNullEvent_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _handler.HandleAsync(null!));
        }

        [TestMethod]
        public async Task HandleAsync_WithValidEvent_CompletesSuccessfully()
        {
            // Arrange
            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                LineItems = Array.Empty<LineItem>()
            };

            _mockProductService
                .Setup(x => x.HandleOrderCreatedEvent(It.IsAny<OrderCreatedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _handler.HandleAsync(orderCreatedEvent);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task HandleAsync_WhenProductServiceThrowsException_PropagatesException()
        {
            // Arrange
            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                LineItems = Array.Empty<LineItem>()
            };

            var expectedException = new InvalidOperationException("Service error");
            _mockProductService
                .Setup(x => x.HandleOrderCreatedEvent(It.IsAny<OrderCreatedEvent>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _handler.HandleAsync(orderCreatedEvent));

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

            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                LineItems = lineItems
            };

            _mockProductService
                .Setup(x => x.HandleOrderCreatedEvent(It.IsAny<OrderCreatedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _handler.HandleAsync(orderCreatedEvent);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}

