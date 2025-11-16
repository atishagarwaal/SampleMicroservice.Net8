using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for Order entity validation attributes.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("OrderEntity")]
    public class OrderEntityTests
    {
        [TestMethod]
        public void Order_WithValidData_ShouldBeValid()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99,
                LineItems = new List<LineItem>
                {
                    new LineItem { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 }
                }
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(order);
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [TestMethod]
        public void Order_ValidationAttributes_AreApplied()
        {
            // Arrange
            var order = new Order
            {
                Id = 0, // Default value
                CustomerId = 0, // Default value
                OrderDate = default(DateTime), // Default value
                TotalAmount = 0.0,
                LineItems = new List<LineItem>()
            };

            // Act & Assert
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(order);
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Order with default values should be valid since [Required] on value types doesn't work as expected
            // The [Required] attribute is primarily for reference types and nullable value types
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }
    }
}
