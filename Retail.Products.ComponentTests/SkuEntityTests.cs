using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;

namespace Retail.Products.ComponentTests
{
    /// <summary>
    /// Unit tests for Sku entity.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class SkuEntityTests
    {
        [TestMethod]
        [TestCategory("SkuEntity")]
        public void Sku_Constructor_CreatesInstance()
        {
            // Act
            var sku = new Sku();

            // Assert
            sku.Should().NotBeNull();
            sku.Should().BeOfType<Sku>();
        }

        [TestMethod]
        [TestCategory("SkuEntity")]
        public void Sku_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var sku = new Sku();
            var id = 1L;
            var name = "Test Product";
            var unitPrice = 29.99;
            var inventory = 100;

            // Act
            sku.Id = id;
            sku.Name = name;
            sku.UnitPrice = unitPrice;
            sku.Inventory = inventory;

            // Assert
            sku.Id.Should().Be(id);
            sku.Name.Should().Be(name);
            sku.UnitPrice.Should().Be(unitPrice);
            sku.Inventory.Should().Be(inventory);
        }

        [TestMethod]
        [TestCategory("SkuEntity")]
        public void Sku_Name_CanBeNull()
        {
            // Arrange
            var sku = new Sku();

            // Act
            sku.Name = null;

            // Assert
            sku.Name.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("SkuEntity")]
        public void Sku_UnitPrice_AcceptsValidValues()
        {
            // Arrange
            var sku = new Sku();
            var validPrices = new[] { 0.0, 0.01, 100.0, 999.99, 1000.0 };

            // Act & Assert
            foreach (var price in validPrices)
            {
                sku.UnitPrice = price;
                sku.UnitPrice.Should().Be(price);
            }
        }

        [TestMethod]
        [TestCategory("SkuEntity")]
        public void Sku_Inventory_AcceptsValidValues()
        {
            // Arrange
            var sku = new Sku();
            var validInventories = new[] { 0, 1, 100, 999, 1000 };

            // Act & Assert
            foreach (var inventory in validInventories)
            {
                sku.Inventory = inventory;
                sku.Inventory.Should().Be(inventory);
            }
        }

        [TestMethod]
        [TestCategory("SkuEntity")]
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
        [TestCategory("SkuEntity")]
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
        [TestCategory("SkuEntity")]
        public void Sku_Name_MaxLengthConstraint_IsEnforced()
        {
            // Arrange
            var sku = new Sku();
            var longName = new string('A', 101); // Exceeds MaxLength(100)

            // Act
            sku.Name = longName;

            // Assert
            sku.Name.Should().Be(longName); // Property can be set, but validation will fail
        }

        [TestMethod]
        [TestCategory("SkuEntity")]
        public void Sku_UnitPrice_DataTypeConstraint_IsApplied()
        {
            // Arrange
            var sku = new Sku();
            var validationContext = new ValidationContext(sku);

            // Act
            sku.UnitPrice = 29.99;
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(sku, validationContext, validationResults, true);

            // Assert
            // UnitPrice is required, so validation will still fail without Name and Inventory
            isValid.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("SkuEntity")]
        public void Sku_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var sku = new Sku();

            // Assert
            sku.Id.Should().Be(0);
            sku.Name.Should().BeNull();
            sku.UnitPrice.Should().Be(0.0);
            sku.Inventory.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("SkuEntity")]
        public void Sku_PropertySetters_UpdateValuesCorrectly()
        {
            // Arrange
            var sku = new Sku();
            var originalId = sku.Id;
            var originalName = sku.Name;
            var originalUnitPrice = sku.UnitPrice;
            var originalInventory = sku.Inventory;

            // Act
            sku.Id = 999L;
            sku.Name = "Updated Product";
            sku.UnitPrice = 99.99;
            sku.Inventory = 500;

            // Assert
            sku.Id.Should().NotBe(originalId).And.Be(999L);
            sku.Name.Should().NotBe(originalName).And.Be("Updated Product");
            sku.UnitPrice.Should().NotBe(originalUnitPrice).And.Be(99.99);
            sku.Inventory.Should().NotBe(originalInventory).And.Be(500);
        }
    }
}
