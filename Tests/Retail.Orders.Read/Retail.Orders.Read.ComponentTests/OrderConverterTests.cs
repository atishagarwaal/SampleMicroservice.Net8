using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for OrderConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("OrderConverter")]
    public sealed class OrderConverterTests
    {
        private Mock<IConverter<LineItemDto, LineItem>> _mockLineItemConverter = null!;
        private OrderConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLineItemConverter = new Mock<IConverter<LineItemDto, LineItem>>();
            _converter = new OrderConverter(_mockLineItemConverter.Object);
        }

        [TestMethod]
        public void Convert_WithValidOrderDto_ReturnsOrderEntity()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
                LineItems = new List<LineItemDto>
                {
                    new LineItemDto { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 }
                }
            };

            var expectedLineItem = new LineItem
            {
                Id = 1,
                OrderId = 1,
                SkuId = 100,
                Qty = 2
            };

            _mockLineItemConverter
                .Setup(x => x.Convert(It.IsAny<LineItemDto>()))
                .Returns(expectedLineItem);

            // Act
            var result = _converter.Convert(orderDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(orderDto.Id);
            result.CustomerId.Should().Be(orderDto.CustomerId);
            result.OrderDate.Should().Be(orderDto.OrderDate);
            result.TotalAmount.Should().Be(orderDto.TotalAmount);
            result.LineItems.Should().HaveCount(1);
            result.LineItems.Should().ContainSingle(li => li.SkuId == 100 && li.Qty == 2);
        }

        [TestMethod]
        public void Convert_WithNullOrderDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => _converter.Convert(null!));
        }

        [TestMethod]
        public void Convert_WithNullLineItems_ReturnsOrderWithoutLineItems()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
                LineItems = null
            };

            // Act
            var result = _converter.Convert(orderDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(orderDto.Id);
            result.CustomerId.Should().Be(orderDto.CustomerId);
            result.LineItems.Should().BeEmpty();
            _mockLineItemConverter.Verify(x => x.Convert(It.IsAny<LineItemDto>()), Times.Never);
        }

        [TestMethod]
        public void Convert_WithEmptyLineItems_ReturnsOrderWithoutLineItems()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
                LineItems = new List<LineItemDto>()
            };

            // Act
            var result = _converter.Convert(orderDto);

            // Assert
            result.Should().NotBeNull();
            result.LineItems.Should().BeEmpty();
            _mockLineItemConverter.Verify(x => x.Convert(It.IsAny<LineItemDto>()), Times.Never);
        }

        [TestMethod]
        public void Convert_WithMultipleLineItems_ConvertsAllLineItems()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
                LineItems = new List<LineItemDto>
                {
                    new LineItemDto { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 },
                    new LineItemDto { Id = 2, OrderId = 1, SkuId = 200, Qty = 3 }
                }
            };

            _mockLineItemConverter
                .Setup(x => x.Convert(It.Is<LineItemDto>(li => li.SkuId == 100)))
                .Returns(new LineItem { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 });

            _mockLineItemConverter
                .Setup(x => x.Convert(It.Is<LineItemDto>(li => li.SkuId == 200)))
                .Returns(new LineItem { Id = 2, OrderId = 1, SkuId = 200, Qty = 3 });

            // Act
            var result = _converter.Convert(orderDto);

            // Assert
            result.Should().NotBeNull();
            result.LineItems.Should().HaveCount(2);
            _mockLineItemConverter.Verify(x => x.Convert(It.IsAny<LineItemDto>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Constructor_WithNullLineItemConverter_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new OrderConverter(null!));
        }
    }
}

