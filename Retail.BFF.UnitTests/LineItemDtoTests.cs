using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.BFFWeb.Api.Model;

namespace Retail.BFF.UnitTests
{
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
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var lineItemDto = new LineItemDto();
            var expectedId = 1L;
            var expectedOrderId = 100L;
            var expectedSkuId = 200L;
            var expectedQty = 5;

            // Act
            lineItemDto.Id = expectedId;
            lineItemDto.OrderId = expectedOrderId;
            lineItemDto.SkuId = expectedSkuId;
            lineItemDto.Qty = expectedQty;

            // Assert
            lineItemDto.Id.Should().Be(expectedId);
            lineItemDto.OrderId.Should().Be(expectedOrderId);
            lineItemDto.SkuId.Should().Be(expectedSkuId);
            lineItemDto.Qty.Should().Be(expectedQty);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetToZero()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act
            lineItemDto.Id = 0;
            lineItemDto.OrderId = 0;
            lineItemDto.SkuId = 0;
            lineItemDto.Qty = 0;

            // Assert
            lineItemDto.Id.Should().Be(0);
            lineItemDto.OrderId.Should().Be(0);
            lineItemDto.SkuId.Should().Be(0);
            lineItemDto.Qty.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetToNegativeValues()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act
            lineItemDto.Id = -1;
            lineItemDto.OrderId = -100;
            lineItemDto.SkuId = -200;
            lineItemDto.Qty = -5;

            // Assert
            lineItemDto.Id.Should().Be(-1);
            lineItemDto.OrderId.Should().Be(-100);
            lineItemDto.SkuId.Should().Be(-200);
            lineItemDto.Qty.Should().Be(-5);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetToLargeValues()
        {
            // Arrange
            var lineItemDto = new LineItemDto();
            var largeId = long.MaxValue;
            var largeQty = int.MaxValue;

            // Act
            lineItemDto.Id = largeId;
            lineItemDto.OrderId = largeId;
            lineItemDto.SkuId = largeId;
            lineItemDto.Qty = largeQty;

            // Assert
            lineItemDto.Id.Should().Be(largeId);
            lineItemDto.OrderId.Should().Be(largeId);
            lineItemDto.SkuId.Should().Be(largeId);
            lineItemDto.Qty.Should().Be(largeQty);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetToMinValues()
        {
            // Arrange
            var lineItemDto = new LineItemDto();
            var minId = long.MinValue;
            var minQty = int.MinValue;

            // Act
            lineItemDto.Id = minId;
            lineItemDto.OrderId = minId;
            lineItemDto.SkuId = minId;
            lineItemDto.Qty = minQty;

            // Assert
            lineItemDto.Id.Should().Be(minId);
            lineItemDto.OrderId.Should().Be(minId);
            lineItemDto.SkuId.Should().Be(minId);
            lineItemDto.Qty.Should().Be(minQty);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetToSmallQuantities()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act
            lineItemDto.Qty = 1;

            // Assert
            lineItemDto.Qty.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetToLargeQuantities()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act
            lineItemDto.Qty = 999999;

            // Assert
            lineItemDto.Qty.Should().Be(999999);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetToSameValues()
        {
            // Arrange
            var lineItemDto = new LineItemDto();
            var sameValue = 123L;

            // Act
            lineItemDto.Id = sameValue;
            lineItemDto.OrderId = sameValue;
            lineItemDto.SkuId = sameValue;

            // Assert
            lineItemDto.Id.Should().Be(sameValue);
            lineItemDto.OrderId.Should().Be(sameValue);
            lineItemDto.SkuId.Should().Be(sameValue);
        }

        [TestMethod]
        [TestCategory("LineItemDto")]
        public void LineItemDto_Properties_CanBeSetToDifferentValues()
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
