using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItemConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class LineItemConverterTests
    {
        private LineItemConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new LineItemConverter();
        }

        [TestMethod]
        [TestCategory("LineItemConverter")]
        public void Convert_WithValidLineItemDto_ReturnsLineItemEntity()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = 100,
                SkuId = 200,
                Qty = 5
            };

            // Act
            var result = _converter.Convert(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(lineItemDto.Id);
            result.OrderId.Should().Be(lineItemDto.OrderId);
            result.SkuId.Should().Be(lineItemDto.SkuId);
            result.Qty.Should().Be(lineItemDto.Qty);
        }

        [TestMethod]
        [TestCategory("LineItemConverter")]
        public void Convert_WithNullLineItemDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => _converter.Convert(null!));
        }

        [TestMethod]
        [TestCategory("LineItemConverter")]
        public void Convert_WithDefaultValues_ReturnsLineItemWithDefaults()
        {
            // Arrange
            var lineItemDto = new LineItemDto();

            // Act
            var result = _converter.Convert(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(0);
            result.OrderId.Should().Be(0);
            result.SkuId.Should().Be(0);
            result.Qty.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("LineItemConverter")]
        public void Convert_WithLargeValues_HandlesCorrectly()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = long.MaxValue,
                OrderId = long.MaxValue - 1,
                SkuId = long.MaxValue - 2,
                Qty = int.MaxValue
            };

            // Act
            var result = _converter.Convert(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(long.MaxValue);
            result.OrderId.Should().Be(long.MaxValue - 1);
            result.SkuId.Should().Be(long.MaxValue - 2);
            result.Qty.Should().Be(int.MaxValue);
        }
    }
}

