using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for OrderDtoConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class OrderDtoConverterTests
    {
        private Mock<IConverter<LineItem, LineItemDto>> _mockLineItemDtoConverter = null!;
        private OrderDtoConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLineItemDtoConverter = new Mock<IConverter<LineItem, LineItemDto>>();
            _converter = new OrderDtoConverter(_mockLineItemDtoConverter.Object);
        }

        [TestMethod]
        [TestCategory("OrderDtoConverter")]
        public void Convert_WithValidOrder_ReturnsOrderDto()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
                LineItems = new List<LineItem>
                {
                    new LineItem { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 }
                }
            };

            var expectedLineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = 1,
                SkuId = 100,
                Qty = 2
            };

            _mockLineItemDtoConverter
                .Setup(x => x.Convert(It.IsAny<LineItem>()))
                .Returns(expectedLineItemDto);

            // Act
            var result = _converter.Convert(order);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(order.Id);
            result.CustomerId.Should().Be(order.CustomerId);
            result.OrderDate.Should().Be(order.OrderDate);
            result.TotalAmount.Should().Be(order.TotalAmount);
            result.LineItems.Should().HaveCount(1);
            result.LineItems.Should().ContainSingle(li => li.SkuId == 100 && li.Qty == 2);
        }

        [TestMethod]
        [TestCategory("OrderDtoConverter")]
        public void Convert_WithNullOrder_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => _converter.Convert(null!));
        }

        [TestMethod]
        [TestCategory("OrderDtoConverter")]
        public void Convert_WithNullLineItems_ReturnsOrderDtoWithoutLineItems()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
                LineItems = null
            };

            // Act
            var result = _converter.Convert(order);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(order.Id);
            result.CustomerId.Should().Be(order.CustomerId);
            result.LineItems.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("OrderDtoConverter")]
        public void Convert_WithEmptyLineItems_ReturnsOrderDtoWithoutLineItems()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
                LineItems = new List<LineItem>()
            };

            // Act
            var result = _converter.Convert(order);

            // Assert
            result.Should().NotBeNull();
            result.LineItems.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("OrderDtoConverter")]
        public void Convert_WithMultipleLineItems_ConvertsAllLineItems()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
                LineItems = new List<LineItem>
                {
                    new LineItem { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 },
                    new LineItem { Id = 2, OrderId = 1, SkuId = 200, Qty = 3 }
                }
            };

            _mockLineItemDtoConverter
                .Setup(x => x.Convert(It.Is<LineItem>(li => li.SkuId == 100)))
                .Returns(new LineItemDto { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 });

            _mockLineItemDtoConverter
                .Setup(x => x.Convert(It.Is<LineItem>(li => li.SkuId == 200)))
                .Returns(new LineItemDto { Id = 2, OrderId = 1, SkuId = 200, Qty = 3 });

            // Act
            var result = _converter.Convert(order);

            // Assert
            result.Should().NotBeNull();
            result.LineItems.Should().HaveCount(2);
            result.LineItems.Should().ContainSingle(li => li.SkuId == 100 && li.Qty == 2);
            result.LineItems.Should().ContainSingle(li => li.SkuId == 200 && li.Qty == 3);
        }

        [TestMethod]
        [TestCategory("OrderDtoConverter")]
        public void Constructor_WithNullLineItemDtoConverter_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new OrderDtoConverter(null!));
        }
    }
}

