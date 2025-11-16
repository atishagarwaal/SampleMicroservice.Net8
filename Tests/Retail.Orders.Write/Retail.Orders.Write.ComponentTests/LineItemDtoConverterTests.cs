using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItemDtoConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class LineItemDtoConverterTests
    {
        private LineItemDtoConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new LineItemDtoConverter();
        }

        [TestMethod]
        [TestCategory("LineItemDtoConverter")]
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
        [TestCategory("LineItemDtoConverter")]
        public void Convert_WithNullLineItem_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => _converter.Convert(null!));
        }

        [TestMethod]
        [TestCategory("LineItemDtoConverter")]
        public void Convert_WithDefaultValues_ReturnsLineItemDtoWithDefaults()
        {
            // Arrange
            var lineItem = new LineItem();

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
        [TestCategory("LineItemDtoConverter")]
        public void Convert_WithLargeValues_HandlesCorrectly()
        {
            // Arrange
            var lineItem = new LineItem
            {
                Id = long.MaxValue,
                OrderId = long.MaxValue - 1,
                SkuId = long.MaxValue - 2,
                Qty = int.MaxValue
            };

            // Act
            var result = _converter.Convert(lineItem);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(long.MaxValue);
            result.OrderId.Should().Be(long.MaxValue - 1);
            result.SkuId.Should().Be(long.MaxValue - 2);
            result.Qty.Should().Be(int.MaxValue);
        }
    }
}

