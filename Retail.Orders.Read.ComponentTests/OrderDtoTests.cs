using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for OrderDto class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class OrderDtoTests
    {
        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Constructor_CreatesInstance()
        {
            // Act
            var orderDto = new OrderDto();

            // Assert
            orderDto.Should().NotBeNull();
            orderDto.Should().BeOfType<OrderDto>();
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var orderDto = new OrderDto();
            var id = 1L;
            var customerId = 123L;
            var orderDate = DateTime.Now;
            var totalAmount = 99.99;

            // Act
            orderDto.Id = id;
            orderDto.CustomerId = customerId;
            orderDto.OrderDate = orderDate;
            orderDto.TotalAmount = totalAmount;

            // Assert
            orderDto.Id.Should().Be(id);
            orderDto.CustomerId.Should().Be(customerId);
            orderDto.OrderDate.Should().Be(orderDate);
            orderDto.TotalAmount.Should().Be(totalAmount);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_LineItems_CanBeSetAndRetrieved()
        {
            // Arrange
            var orderDto = new OrderDto();
            var lineItems = new List<LineItemDto>
            {
                new LineItemDto { Id = 1, SkuId = 100, Qty = 2 },
                new LineItemDto { Id = 2, SkuId = 200, Qty = 1 }
            };

            // Act
            orderDto.LineItems = lineItems;

            // Assert
            orderDto.LineItems.Should().BeEquivalentTo(lineItems);
            orderDto.LineItems.Should().HaveCount(2);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_LineItems_CanBeNull()
        {
            // Arrange
            var orderDto = new OrderDto();

            // Act
            orderDto.LineItems = null;

            // Assert
            orderDto.LineItems.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_DefaultValues_AreCorrect()
        {
            // Act
            var orderDto = new OrderDto();

            // Assert
            orderDto.Id.Should().Be(0);
            orderDto.CustomerId.Should().Be(0);
            orderDto.OrderDate.Should().Be(default(DateTime));
            orderDto.TotalAmount.Should().Be(0.0);
            orderDto.LineItems.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_WithValidData_PropertiesAreSet()
        {
            // Arrange
            var id = 456L;
            var customerId = 789L;
            var orderDate = new DateTime(2024, 1, 15);
            var totalAmount = 149.99;
            var lineItems = new List<LineItemDto>
            {
                new LineItemDto { Id = 1, SkuId = 100, Qty = 3 }
            };

            // Act
            var orderDto = new OrderDto
            {
                Id = id,
                CustomerId = customerId,
                OrderDate = orderDate,
                TotalAmount = totalAmount,
                LineItems = lineItems
            };

            // Assert
            orderDto.Id.Should().Be(id);
            orderDto.CustomerId.Should().Be(customerId);
            orderDto.OrderDate.Should().Be(orderDate);
            orderDto.TotalAmount.Should().Be(totalAmount);
            orderDto.LineItems.Should().BeEquivalentTo(lineItems);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_LineItems_CanBeEmptyList()
        {
            // Arrange
            var orderDto = new OrderDto();
            var emptyLineItems = new List<LineItemDto>();

            // Act
            orderDto.LineItems = emptyLineItems;

            // Assert
            orderDto.LineItems.Should().BeEmpty();
            orderDto.LineItems.Should().NotBeNull();
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_TotalAmount_CanBeZero()
        {
            // Arrange
            var orderDto = new OrderDto();

            // Act
            orderDto.TotalAmount = 0.0;

            // Assert
            orderDto.TotalAmount.Should().Be(0.0);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_TotalAmount_CanBeNegative()
        {
            // Arrange
            var orderDto = new OrderDto();

            // Act
            orderDto.TotalAmount = -10.50;

            // Assert
            orderDto.TotalAmount.Should().Be(-10.50);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_TotalAmount_CanBeLargeNumber()
        {
            // Arrange
            var orderDto = new OrderDto();
            var largeAmount = double.MaxValue;

            // Act
            orderDto.TotalAmount = largeAmount;

            // Assert
            orderDto.TotalAmount.Should().Be(largeAmount);
        }
    }
}
