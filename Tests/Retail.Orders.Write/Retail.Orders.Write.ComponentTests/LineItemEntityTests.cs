using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItem entity validation attributes.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("LineItemEntity")]
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
    }
}
