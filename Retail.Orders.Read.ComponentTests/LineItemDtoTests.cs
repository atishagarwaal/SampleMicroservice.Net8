using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItemDto class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class LineItemDtoTests
    {
        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Constructor_CreatesInstance()
        {
            // Act
            var lineItemDto = new LineItemDto();

            // Assert
            lineItemDto.Should().NotBeNull();
            lineItemDto.Should().BeOfType<LineItemDto>();
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var lineItemDto = new LineItemDto();
            var id = 1L;
            var orderId = 123L;
            var skuId = 456L;
            var qty = 2;

            // Act
            lineItemDto.Id = id;
            lineItemDto.OrderId = orderId;
            lineItemDto.SkuId = skuId;
            lineItemDto.Qty = qty;

            // Assert
            lineItemDto.Id.Should().Be(id);
            lineItemDto.OrderId.Should().Be(orderId);
            lineItemDto.SkuId.Should().Be(skuId);
            lineItemDto.Qty.Should().Be(qty);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_DefaultValues_AreCorrect()
        {
            // Act
            var lineItemDto = new LineItemDto();

            // Assert
            lineItemDto.Id.Should().Be(0);
            lineItemDto.OrderId.Should().Be(0);
            lineItemDto.SkuId.Should().Be(0);
            lineItemDto.Qty.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_WithValidData_PropertiesAreSet()
        {
            // Arrange
            var id = 789L;
            var orderId = 101L;
            var skuId = 202L;
            var qty = 5;

            // Act
            var lineItemDto = new LineItemDto
            {
                Id = id,
                OrderId = orderId,
                SkuId = skuId,
                Qty = qty
            };

            // Assert
            lineItemDto.Id.Should().Be(id);
            lineItemDto.OrderId.Should().Be(orderId);
            lineItemDto.SkuId.Should().Be(skuId);
            lineItemDto.Qty.Should().Be(qty);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Quantity_CanBeZero()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act
            lineItemDto.Qty = 0;

            // Assert
            lineItemDto.Qty.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Quantity_CanBeNegative()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act
            lineItemDto.Qty = -1;

            // Assert
            lineItemDto.Qty.Should().Be(-1);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Quantity_CanBeLargeNumber()
        {
            // Arrange
            var lineItemDto = new LineItemDto();
            var largeQty = int.MaxValue;

            // Act
            lineItemDto.Qty = largeQty;

            // Assert
            lineItemDto.Qty.Should().Be(largeQty);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Id_CanBeLargeNumber()
        {
            // Arrange
            var lineItemDto = new LineItemDto();
            var largeId = long.MaxValue;

            // Act
            lineItemDto.Id = largeId;

            // Assert
            lineItemDto.Id.Should().Be(largeId);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_OrderId_CanBeLargeNumber()
        {
            // Arrange
            var lineItemDto = new LineItemDto();
            var largeOrderId = long.MaxValue;

            // Act
            lineItemDto.OrderId = largeOrderId;

            // Assert
            lineItemDto.OrderId.Should().Be(largeOrderId);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_SkuId_CanBeLargeNumber()
        {
            // Arrange
            var lineItemDto = new LineItemDto();
            var largeSkuId = long.MaxValue;

            // Act
            lineItemDto.SkuId = largeSkuId;

            // Assert
            lineItemDto.SkuId.Should().Be(largeSkuId);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_AllProperties_CanBeSetIndependently()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act
            lineItemDto.Id = 1L;
            lineItemDto.OrderId = 2L;
            lineItemDto.SkuId = 3L;
            lineItemDto.Qty = 4;

            // Assert
            lineItemDto.Id.Should().Be(1L);
            lineItemDto.OrderId.Should().Be(2L);
            lineItemDto.SkuId.Should().Be(3L);
            lineItemDto.Qty.Should().Be(4);
        }
    }
}
