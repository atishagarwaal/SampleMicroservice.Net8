using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItemDtoConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("LineItemDtoConverter")]
    public sealed class LineItemDtoConverterTests
    {
        private LineItemDtoConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new LineItemDtoConverter();
        }

        [TestMethod]
        public void Convert_WithValidLineItem_ReturnsLineItemDto()
        {
            // Arrange
            var lineItem = new LineItem
            {
                Id = 1,
                OrderId = 100,
                SkuId = 200,
                Qty = 5
            };

            // Act
            var result = _converter.Convert(lineItem);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(lineItem.Id);
            result.OrderId.Should().Be(lineItem.OrderId);
            result.SkuId.Should().Be(lineItem.SkuId);
            result.Qty.Should().Be(lineItem.Qty);
        }

        [TestMethod]
        public void Convert_WithNullLineItem_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => _converter.Convert(null!));
        }

        [TestMethod]
        public void Convert_WithZeroValues_ReturnsLineItemDtoWithZeroValues()
        {
            // Arrange
            var lineItem = new LineItem
            {
                Id = 0,
                OrderId = 0,
                SkuId = 0,
                Qty = 0
            };

            // Act
            var result = _converter.Convert(lineItem);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(0);
            result.OrderId.Should().Be(0);
            result.SkuId.Should().Be(0);
            result.Qty.Should().Be(0);
        }

        [TestMethod]
        public void Convert_WithMaxValues_ReturnsLineItemDtoWithMaxValues()
        {
            // Arrange
            var lineItem = new LineItem
            {
                Id = long.MaxValue,
                OrderId = long.MaxValue,
                SkuId = long.MaxValue,
                Qty = int.MaxValue
            };

            // Act
            var result = _converter.Convert(lineItem);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(long.MaxValue);
            result.OrderId.Should().Be(long.MaxValue);
            result.SkuId.Should().Be(long.MaxValue);
            result.Qty.Should().Be(int.MaxValue);
        }
    }
}

