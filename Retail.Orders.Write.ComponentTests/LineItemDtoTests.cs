using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItemDto class.
    /// </summary>
    [TestClass]
    public class LineItemDtoTests
    {
        [TestMethod]
        public void LineItemDto_WithValidData_ShouldBeValid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = 123,
                SkuId = 456,
                Qty = 2
            };

            // Act & Assert
            lineItemDto.Should().NotBeNull();
            lineItemDto.Id.Should().Be(1);
            lineItemDto.OrderId.Should().Be(123);
            lineItemDto.SkuId.Should().Be(456);
            lineItemDto.Qty.Should().Be(2);
        }

        [TestMethod]
        public void LineItemDto_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var expectedId = 1L;
            var expectedOrderId = 123L;
            var expectedSkuId = 456L;
            var expectedQty = 2;

            // Act
            var lineItemDto = new LineItemDto
            {
                Id = expectedId,
                OrderId = expectedOrderId,
                SkuId = expectedSkuId,
                Qty = expectedQty
            };

            // Assert
            lineItemDto.Id.Should().Be(expectedId);
            lineItemDto.OrderId.Should().Be(expectedOrderId);
            lineItemDto.SkuId.Should().Be(expectedSkuId);
            lineItemDto.Qty.Should().Be(expectedQty);
        }

        [TestMethod]
        public void LineItemDto_WithDefaultValues_ShouldBeValid()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act & Assert
            lineItemDto.Should().NotBeNull();
            lineItemDto.Id.Should().Be(0);
            lineItemDto.OrderId.Should().Be(0);
            lineItemDto.SkuId.Should().Be(0);
            lineItemDto.Qty.Should().Be(0);
        }

        [TestMethod]
        public void LineItemDto_OrderId_ShouldMatchOrderId()
        {
            // Arrange
            var orderId = 123L;
            var lineItemDto = new LineItemDto { OrderId = orderId };

            // Act & Assert
            lineItemDto.OrderId.Should().Be(orderId);
        }

        [TestMethod]
        public void LineItemDto_Quantity_ShouldBePositive()
        {
            // Arrange
            var lineItemDto = new LineItemDto { Qty = 5 };

            // Act & Assert
            lineItemDto.Qty.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void LineItemDto_SkuId_ShouldBeValid()
        {
            // Arrange
            var lineItemDto = new LineItemDto { SkuId = 456 };

            // Act & Assert
            lineItemDto.SkuId.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void LineItemDto_Id_ShouldBeValid()
        {
            // Arrange
            var lineItemDto = new LineItemDto { Id = 1 };

            // Act & Assert
            lineItemDto.Id.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void LineItemDto_AllProperties_ShouldBeMutable()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act
            lineItemDto.Id = 100;
            lineItemDto.OrderId = 200;
            lineItemDto.SkuId = 300;
            lineItemDto.Qty = 4;

            // Assert
            lineItemDto.Id.Should().Be(100);
            lineItemDto.OrderId.Should().Be(200);
            lineItemDto.SkuId.Should().Be(300);
            lineItemDto.Qty.Should().Be(4);
        }

        [TestMethod]
        public void LineItemDto_WithLargeValues_ShouldHandleCorrectly()
        {
            // Arrange
            var largeId = long.MaxValue;
            var largeOrderId = long.MaxValue - 1;
            var largeSkuId = long.MaxValue - 2;
            var largeQty = int.MaxValue;

            // Act
            var lineItemDto = new LineItemDto
            {
                Id = largeId,
                OrderId = largeOrderId,
                SkuId = largeSkuId,
                Qty = largeQty
            };

            // Assert
            lineItemDto.Id.Should().Be(largeId);
            lineItemDto.OrderId.Should().Be(largeOrderId);
            lineItemDto.SkuId.Should().Be(largeSkuId);
            lineItemDto.Qty.Should().Be(largeQty);
        }
    }
}
