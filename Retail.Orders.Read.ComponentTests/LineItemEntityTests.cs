using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItem entity.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class LineItemEntityTests
    {
        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_Constructor_CreatesInstance()
        {
            // Act
            var lineItem = new LineItem();

            // Assert
            lineItem.Should().NotBeNull();
            lineItem.Should().BeOfType<LineItem>();
        }

        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var lineItem = new LineItem();
            var id = 1L;
            var orderId = 123L;
            var skuId = 456L;
            var qty = 2;

            // Act
            lineItem.Id = id;
            lineItem.OrderId = orderId;
            lineItem.SkuId = skuId;
            lineItem.Qty = qty;

            // Assert
            lineItem.Id.Should().Be(id);
            lineItem.OrderId.Should().Be(orderId);
            lineItem.SkuId.Should().Be(skuId);
            lineItem.Qty.Should().Be(qty);
        }

        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_Order_CanBeSetAndRetrieved()
        {
            // Arrange
            var lineItem = new LineItem();
            var order = new Order { Id = 123L, CustomerId = 456L };

            // Act
            lineItem.Order = order;

            // Assert
            lineItem.Order.Should().Be(order);
            lineItem.Order.Should().BeOfType<Order>();
        }

        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_Order_CanBeNull()
        {
            // Arrange
            var lineItem = new LineItem();

            // Act
            lineItem.Order = null;

            // Assert
            lineItem.Order.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_DefaultValues_AreCorrect()
        {
            // Act
            var lineItem = new LineItem();

            // Assert
            lineItem.Id.Should().Be(0);
            lineItem.OrderId.Should().Be(0);
            lineItem.SkuId.Should().Be(0);
            lineItem.Qty.Should().Be(0);
            lineItem.Order.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_WithValidData_PropertiesAreSet()
        {
            // Arrange
            var id = 789L;
            var orderId = 101L;
            var skuId = 202L;
            var qty = 5;

            // Act
            var lineItem = new LineItem
            {
                Id = id,
                OrderId = orderId,
                SkuId = skuId,
                Qty = qty
            };

            // Assert
            lineItem.Id.Should().Be(id);
            lineItem.OrderId.Should().Be(orderId);
            lineItem.SkuId.Should().Be(skuId);
            lineItem.Qty.Should().Be(qty);
        }

        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_OrderNavigation_WorksCorrectly()
        {
            // Arrange
            var lineItem = new LineItem();
            var order = new Order
            {
                Id = 123L,
                CustomerId = 456L,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99
            };

            // Act
            lineItem.Order = order;
            lineItem.OrderId = order.Id;

            // Assert
            lineItem.Order.Should().Be(order);
            lineItem.OrderId.Should().Be(order.Id);
        }

        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_Quantity_CanBeZero()
        {
            // Arrange
            var lineItem = new LineItem();

            // Act
            lineItem.Qty = 0;

            // Assert
            lineItem.Qty.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_Quantity_CanBeNegative()
        {
            // Arrange
            var lineItem = new LineItem();

            // Act
            lineItem.Qty = -1;

            // Assert
            lineItem.Qty.Should().Be(-1);
        }

        [TestMethod]
        [TestCategory("LineItemEntity")]
        public void LineItem_Quantity_CanBeLargeNumber()
        {
            // Arrange
            var lineItem = new LineItem();
            var largeQty = int.MaxValue;

            // Act
            lineItem.Qty = largeQty;

            // Assert
            lineItem.Qty.Should().Be(largeQty);
        }
    }
}
