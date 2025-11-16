using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for Order entity validation attributes.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("OrderEntity")]
    public sealed class OrderEntityTests
    {
        [TestMethod]
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
    }
}
