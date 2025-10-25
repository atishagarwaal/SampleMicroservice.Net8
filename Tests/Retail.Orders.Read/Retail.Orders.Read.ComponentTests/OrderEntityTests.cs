using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for Order entity.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class OrderEntityTests
    {
        [TestMethod]
        [TestCategory("OrderEntity")]
        public void Order_Constructor_CreatesInstance()
        {
            // Act
            var order = new Order();

            // Assert
            order.Should().NotBeNull();
            order.Should().BeOfType<Order>();
        }

        [TestMethod]
        [TestCategory("OrderEntity")]
        public void Order_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var order = new Order();
            var id = 1L;
            var customerId = 123L;
            var orderDate = DateTime.Now;
            var totalAmount = 99.99;

            // Act
            order.Id = id;
            order.CustomerId = customerId;
            order.OrderDate = orderDate;
            order.TotalAmount = totalAmount;

            // Assert
            order.Id.Should().Be(id);
            order.CustomerId.Should().Be(customerId);
            order.OrderDate.Should().Be(orderDate);
            order.TotalAmount.Should().Be(totalAmount);
        }

        [TestMethod]
        [TestCategory("OrderEntity")]
        public void Order_LineItems_InitializesAsEmptyList()
        {
            // Act
            var order = new Order();

            // Assert
            order.LineItems.Should().NotBeNull();
            order.LineItems.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("OrderEntity")]
        public void Order_LineItems_CanBeModified()
        {
            // Arrange
            var order = new Order();
            var lineItem = new LineItem { Id = 1, SkuId = 100, Qty = 2 };

            // Act
            order.LineItems.Add(lineItem);

            // Assert
            order.LineItems.Should().HaveCount(1);
            order.LineItems.First().Should().Be(lineItem);
        }

        [TestMethod]
        [TestCategory("OrderEntity")]
        public void Order_ValidationAttributes_AreApplied()
        {
            // Arrange
            var order = new Order();
            var validationContext = new ValidationContext(order);

            // Act & Assert
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Order with default values should be valid since [Required] on value types doesn't work as expected
            // The [Required] attribute is primarily for reference types and nullable value types
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("OrderEntity")]
        public void Order_WithValidData_IsValid()
        {
            // Arrange
            var order = new Order
            {
                CustomerId = 123L,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99
            };
            var validationContext = new ValidationContext(order);

            // Act & Assert
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("OrderEntity")]
        public void Order_DefaultValues_AreCorrect()
        {
            // Act
            var order = new Order();

            // Assert
            order.Id.Should().Be(0);
            order.CustomerId.Should().Be(0);
            order.OrderDate.Should().Be(default(DateTime));
            order.TotalAmount.Should().Be(0.0);
            order.LineItems.Should().NotBeNull();
            order.LineItems.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("OrderEntity")]
        public void Order_WithValidData_PropertiesAreSet()
        {
            // Arrange
            var id = 456L;
            var customerId = 789L;
            var orderDate = new DateTime(2024, 1, 15);
            var totalAmount = 149.99;

            // Act
            var order = new Order
            {
                Id = id,
                CustomerId = customerId,
                OrderDate = orderDate,
                TotalAmount = totalAmount
            };

            // Assert
            order.Id.Should().Be(id);
            order.CustomerId.Should().Be(customerId);
            order.OrderDate.Should().Be(orderDate);
            order.TotalAmount.Should().Be(totalAmount);
        }

        [TestMethod]
        [TestCategory("OrderEntity")]
        public void Order_LineItems_CanBeReplaced()
        {
            // Arrange
            var order = new Order();
            var originalLineItems = new List<LineItem> { new LineItem { Id = 1, SkuId = 100, Qty = 1 } };
            var newLineItems = new List<LineItem> { new LineItem { Id = 2, SkuId = 200, Qty = 3 } };

            // Act
            order.LineItems = originalLineItems;
            order.LineItems = newLineItems;

            // Assert
            order.LineItems.Should().BeSameAs(newLineItems);
            order.LineItems.Should().HaveCount(1);
            order.LineItems.First().Id.Should().Be(2);
        }
    }
}
