using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItem entity validation attributes.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("LineItemEntity")]
    public sealed class LineItemEntityTests
    {
        [TestMethod]
        public void LineItem_WithValidData_IsValid()
        {
            // Arrange
            var lineItem = new LineItem
            {
                Id = 1,
                OrderId = 123,
                SkuId = 456,
                Qty = 2
            };
            var validationContext = new ValidationContext(lineItem);

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(lineItem, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }
    }
}
