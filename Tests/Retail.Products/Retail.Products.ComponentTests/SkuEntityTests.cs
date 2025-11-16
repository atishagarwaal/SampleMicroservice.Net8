using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;

namespace Retail.Products.ComponentTests
{
    /// <summary>
    /// Unit tests for Sku entity validation attributes.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("SkuEntity")]
    public sealed class SkuEntityTests
    {
        [TestMethod]
        public void Sku_ValidationAttributes_AreApplied()
        {
            // Arrange
            var sku = new Sku();
            var validationContext = new ValidationContext(sku);

            // Act & Assert
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(sku, validationContext, validationResults, true);

            // Sku without required fields should not be valid
            isValid.Should().BeFalse();
            validationResults.Should().HaveCountGreaterThan(0);
        }

        [TestMethod]
        public void Sku_WithValidData_IsValid()
        {
            // Arrange
            var sku = new Sku
            {
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = 100
            };
            var validationContext = new ValidationContext(sku);

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(sku, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().HaveCount(0);
        }

        [TestMethod]
        public void Sku_Name_MaxLengthConstraint_IsEnforced()
        {
            // Arrange
            var sku = new Sku
            {
                Name = new string('A', 101), // Exceeds MaxLength(100)
                UnitPrice = 29.99,
                Inventory = 100
            };
            var validationContext = new ValidationContext(sku);

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(sku, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Sku.Name)));
        }

        [TestMethod]
        public void Sku_WithMissingRequiredFields_IsInvalid()
        {
            // Arrange
            var sku = new Sku
            {
                UnitPrice = 29.99
                // Missing Name and Inventory
            };
            var validationContext = new ValidationContext(sku);

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(sku, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().HaveCountGreaterThan(0);
        }
    }
}
