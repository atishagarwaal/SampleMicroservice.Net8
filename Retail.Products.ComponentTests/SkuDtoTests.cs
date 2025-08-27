using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;

namespace Retail.Products.ComponentTests
{
    /// <summary>
    /// Unit tests for SkuDto class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class SkuDtoTests
    {
        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Constructor_CreatesInstance()
        {
            // Act
            var skuDto = new SkuDto();

            // Assert
            skuDto.Should().NotBeNull();
            skuDto.Should().BeOfType<SkuDto>();
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var skuDto = new SkuDto();
            var id = 1L;
            var name = "Test Product";
            var unitPrice = 29.99;
            var inventory = 100;

            // Act
            skuDto.Id = id;
            skuDto.Name = name;
            skuDto.UnitPrice = unitPrice;
            skuDto.Inventory = inventory;

            // Assert
            skuDto.Id.Should().Be(id);
            skuDto.Name.Should().Be(name);
            skuDto.UnitPrice.Should().Be(unitPrice);
            skuDto.Inventory.Should().Be(inventory);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Name_CanBeNull()
        {
            // Arrange
            var skuDto = new SkuDto();

            // Act
            skuDto.Name = null;

            // Assert
            skuDto.Name.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_UnitPrice_AcceptsValidValues()
        {
            // Arrange
            var skuDto = new SkuDto();
            var validPrices = new[] { 0.0, 0.01, 100.0, 999.99, 1000.0 };

            // Act & Assert
            foreach (var price in validPrices)
            {
                skuDto.UnitPrice = price;
                skuDto.UnitPrice.Should().Be(price);
            }
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Inventory_AcceptsValidValues()
        {
            // Arrange
            var skuDto = new SkuDto();
            var validInventories = new[] { 0, 1, 100, 999, 1000 };

            // Act & Assert
            foreach (var inventory in validInventories)
            {
                skuDto.Inventory = inventory;
                skuDto.Inventory.Should().Be(inventory);
            }
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var skuDto = new SkuDto();

            // Assert
            skuDto.Id.Should().Be(0);
            skuDto.Name.Should().BeNull();
            skuDto.UnitPrice.Should().Be(0.0);
            skuDto.Inventory.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_PropertySetters_UpdateValuesCorrectly()
        {
            // Arrange
            var skuDto = new SkuDto();
            var originalId = skuDto.Id;
            var originalName = skuDto.Name;
            var originalUnitPrice = skuDto.UnitPrice;
            var originalInventory = skuDto.Inventory;

            // Act
            skuDto.Id = 999L;
            skuDto.Name = "Updated Product";
            skuDto.UnitPrice = 99.99;
            skuDto.Inventory = 500;

            // Assert
            skuDto.Id.Should().NotBe(originalId).And.Be(999L);
            skuDto.Name.Should().NotBe(originalName).And.Be("Updated Product");
            skuDto.UnitPrice.Should().NotBe(originalUnitPrice).And.Be(99.99);
            skuDto.Inventory.Should().NotBe(originalInventory).And.Be(500);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_ObjectInitialization_WorksCorrectly()
        {
            // Arrange
            var id = 1L;
            var name = "Test Product";
            var unitPrice = 29.99;
            var inventory = 100;

            // Act
            var skuDto = new SkuDto
            {
                Id = id,
                Name = name,
                UnitPrice = unitPrice,
                Inventory = inventory
            };

            // Assert
            skuDto.Should().NotBeNull();
            skuDto.Id.Should().Be(id);
            skuDto.Name.Should().Be(name);
            skuDto.UnitPrice.Should().Be(unitPrice);
            skuDto.Inventory.Should().Be(inventory);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Equality_WorksCorrectly()
        {
            // Arrange
            var skuDto1 = new SkuDto
            {
                Id = 1L,
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = 100
            };

            var skuDto2 = new SkuDto
            {
                Id = 1L,
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = 100
            };

            var skuDto3 = new SkuDto
            {
                Id = 2L,
                Name = "Different Product",
                UnitPrice = 39.99,
                Inventory = 200
            };

            // Act & Assert
            skuDto1.Should().NotBeSameAs(skuDto2); // Different instances
            skuDto1.Id.Should().Be(skuDto2.Id);
            skuDto1.Name.Should().Be(skuDto2.Name);
            skuDto1.UnitPrice.Should().Be(skuDto2.UnitPrice);
            skuDto1.Inventory.Should().Be(skuDto2.Inventory);

            skuDto1.Id.Should().NotBe(skuDto3.Id);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_EdgeCases_HandleCorrectly()
        {
            // Arrange
            var skuDto = new SkuDto();

            // Act & Assert
            // Test with very large numbers
            skuDto.Id = long.MaxValue;
            skuDto.UnitPrice = double.MaxValue;
            skuDto.Inventory = int.MaxValue;

            skuDto.Id.Should().Be(long.MaxValue);
            skuDto.UnitPrice.Should().Be(double.MaxValue);
            skuDto.Inventory.Should().Be(int.MaxValue);

            // Test with very small numbers
            skuDto.Id = long.MinValue;
            skuDto.UnitPrice = double.MinValue;
            skuDto.Inventory = int.MinValue;

            skuDto.Id.Should().Be(long.MinValue);
            skuDto.UnitPrice.Should().Be(double.MinValue);
            skuDto.Inventory.Should().Be(int.MinValue);
        }
    }
}
