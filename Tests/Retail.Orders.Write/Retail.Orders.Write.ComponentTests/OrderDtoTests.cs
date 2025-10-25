using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for OrderDto class.
    /// </summary>
    [TestClass]
    public class OrderDtoTests
    {
        [TestMethod]
        public void OrderDto_WithValidData_ShouldBeValid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99,
                LineItems = new List<LineItemDto>
                {
                    new LineItemDto { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 }
                }
            };

            // Act & Assert
            orderDto.Should().NotBeNull();
            orderDto.Id.Should().Be(1);
            orderDto.CustomerId.Should().Be(123);
            orderDto.OrderDate.Should().NotBe(default(DateTime));
            orderDto.TotalAmount.Should().Be(99.99);
            orderDto.LineItems.Should().NotBeNull();
            orderDto.LineItems.Should().HaveCount(1);
        }

        [TestMethod]
        public void OrderDto_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var expectedId = 1L;
            var expectedCustomerId = 123L;
            var expectedOrderDate = DateTime.Now;
            var expectedTotalAmount = 99.99;
            var expectedLineItems = new List<LineItemDto>
            {
                new LineItemDto { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 },
                new LineItemDto { Id = 2, OrderId = 1, SkuId = 200, Qty = 1 }
            };

            // Act
            var orderDto = new OrderDto
            {
                Id = expectedId,
                CustomerId = expectedCustomerId,
                OrderDate = expectedOrderDate,
                TotalAmount = expectedTotalAmount,
                LineItems = expectedLineItems
            };

            // Assert
            orderDto.Id.Should().Be(expectedId);
            orderDto.CustomerId.Should().Be(expectedCustomerId);
            orderDto.OrderDate.Should().Be(expectedOrderDate);
            orderDto.TotalAmount.Should().Be(expectedTotalAmount);
            orderDto.LineItems.Should().BeEquivalentTo(expectedLineItems);
        }

        [TestMethod]
        public void OrderDto_LineItems_ShouldBeAddable()
        {
            // Arrange
            var orderDto = new OrderDto { LineItems = new List<LineItemDto>() };
            var lineItem = new LineItemDto { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 };

            // Act
            orderDto.LineItems.Add(lineItem);

            // Assert
            orderDto.LineItems.Should().HaveCount(1);
            orderDto.LineItems.First().Should().Be(lineItem);
        }

        [TestMethod]
        public void OrderDto_LineItems_ShouldBeRemovable()
        {
            // Arrange
            var orderDto = new OrderDto { LineItems = new List<LineItemDto>() };
            var lineItem = new LineItemDto { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 };
            orderDto.LineItems.Add(lineItem);

            // Act
            orderDto.LineItems.Remove(lineItem);

            // Assert
            orderDto.LineItems.Should().BeEmpty();
        }

        [TestMethod]
        public void OrderDto_WithNullLineItems_ShouldBeValid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99,
                LineItems = null
            };

            // Act & Assert
            orderDto.Should().NotBeNull();
            orderDto.LineItems.Should().BeNull();
        }

        [TestMethod]
        public void OrderDto_WithEmptyLineItems_ShouldBeValid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99,
                LineItems = new List<LineItemDto>()
            };

            // Act & Assert
            orderDto.Should().NotBeNull();
            orderDto.LineItems.Should().NotBeNull();
            orderDto.LineItems.Should().BeEmpty();
        }

        [TestMethod]
        public void OrderDto_TotalAmount_ShouldHandleDecimalValues()
        {
            // Arrange
            var expectedTotalAmount = 123.45;

            // Act
            var orderDto = new OrderDto { TotalAmount = expectedTotalAmount };

            // Assert
            orderDto.TotalAmount.Should().Be(expectedTotalAmount);
        }

        [TestMethod]
        public void OrderDto_OrderDate_ShouldHandleDifferentDateFormats()
        {
            // Arrange
            var expectedOrderDate = new DateTime(2024, 1, 15, 10, 30, 0);

            // Act
            var orderDto = new OrderDto { OrderDate = expectedOrderDate };

            // Assert
            orderDto.OrderDate.Should().Be(expectedOrderDate);
            orderDto.OrderDate.Year.Should().Be(2024);
            orderDto.OrderDate.Month.Should().Be(1);
            orderDto.OrderDate.Day.Should().Be(15);
        }
    }
}
