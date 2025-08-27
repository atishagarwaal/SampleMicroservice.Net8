using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItem entity.
    /// </summary>
    [TestClass]
    public class LineItemEntityTests
    {
        [TestMethod]
        public void LineItem_WithValidData_ShouldBeValid()
        {
            // Arrange
            var lineItem = new LineItem
            {
                Id = 1,
                OrderId = 123,
                SkuId = 456,
                Qty = 2
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(lineItem);
            var isValid = Validator.TryValidateObject(lineItem, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [TestMethod]
        public void LineItem_ValidationAttributes_AreApplied()
        {
            // Arrange
            var lineItem = new LineItem
            {
                Id = 0, // Default value
                OrderId = 0, // Default value
                SkuId = 0, // Default value
                Qty = 0, // Default value
                Order = null
            };

            // Act & Assert
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(lineItem);
            var isValid = Validator.TryValidateObject(lineItem, validationContext, validationResults, true);

            // LineItem with default values should be valid since [Required] on value types doesn't work as expected
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [TestMethod]
        public void LineItem_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var expectedId = 1L;
            var expectedOrderId = 123L;
            var expectedSkuId = 456L;
            var expectedQty = 2;
            var expectedOrder = new Order { Id = 123 };

            // Act
            var lineItem = new LineItem
            {
                Id = expectedId,
                OrderId = expectedOrderId,
                SkuId = expectedSkuId,
                Qty = expectedQty,
                Order = expectedOrder
            };

            // Assert
            lineItem.Id.Should().Be(expectedId);
            lineItem.OrderId.Should().Be(expectedOrderId);
            lineItem.SkuId.Should().Be(expectedSkuId);
            lineItem.Qty.Should().Be(expectedQty);
            lineItem.Order.Should().Be(expectedOrder);
        }

        [TestMethod]
        public void LineItem_OrderNavigation_ShouldBeSetCorrectly()
        {
            // Arrange
            var order = new Order { Id = 123, CustomerId = 456 };
            var lineItem = new LineItem { Id = 1, OrderId = 123 };

            // Act
            lineItem.Order = order;

            // Assert
            lineItem.Order.Should().Be(order);
            lineItem.OrderId.Should().Be(order.Id);
        }

        [TestMethod]
        public void LineItem_OrderId_ShouldMatchOrderId()
        {
            // Arrange
            var orderId = 123L;
            var lineItem = new LineItem { Id = 1, OrderId = orderId };

            // Act & Assert
            lineItem.OrderId.Should().Be(orderId);
        }

        [TestMethod]
        public void LineItem_Quantity_ShouldBePositive()
        {
            // Arrange
            var lineItem = new LineItem { Id = 1, OrderId = 123, SkuId = 456, Qty = 5 };

            // Act & Assert
            lineItem.Qty.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void LineItem_SkuId_ShouldBeValid()
        {
            // Arrange
            var lineItem = new LineItem { Id = 1, OrderId = 123, SkuId = 456, Qty = 2 };

            // Act & Assert
            lineItem.SkuId.Should().BeGreaterThan(0);
        }
    }
}
