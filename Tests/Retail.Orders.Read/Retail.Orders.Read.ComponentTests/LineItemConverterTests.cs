using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItemConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("LineItemConverter")]
    public sealed class LineItemConverterTests
    {
        private LineItemConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new LineItemConverter();
        }

        [TestMethod]
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
        public void Convert_WithNullLineItemDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => _converter.Convert(null!));
        }

        [TestMethod]
        public void Convert_WithZeroValues_ReturnsLineItemWithZeroValues()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 0,
                OrderId = 0,
                SkuId = 0,
                Qty = 0
            };

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
        public void Convert_WithMaxValues_ReturnsLineItemWithMaxValues()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = long.MaxValue,
                OrderId = long.MaxValue,
                SkuId = long.MaxValue,
                Qty = int.MaxValue
            };

            // Act
            var result = _converter.Convert(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(long.MaxValue);
            result.OrderId.Should().Be(long.MaxValue);
            result.SkuId.Should().Be(long.MaxValue);
            result.Qty.Should().Be(int.MaxValue);
        }
    }
}

