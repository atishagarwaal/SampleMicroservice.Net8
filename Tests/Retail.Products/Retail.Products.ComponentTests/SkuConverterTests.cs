using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Products.src.CleanArchitecture.Application.Converters;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;

namespace Retail.Products.ComponentTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class SkuConverterTests
    {
        private SkuConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new SkuConverter();
        }

        [TestMethod]
        public void Convert_WithValidSkuDto_ReturnsSkuEntity()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 1,
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _converter.Convert(skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(skuDto.Id);
            result.Name.Should().Be(skuDto.Name);
            result.UnitPrice.Should().Be(skuDto.UnitPrice);
            result.Inventory.Should().Be(skuDto.Inventory);
        }

        [TestMethod]
        public void Convert_WithNullSkuDto_ThrowsArgumentNullException()
        {
            // Arrange
            SkuDto nullSkuDto = null!;

            // Act
            Action act = () => _converter.Convert(nullSkuDto);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sourceType");
        }

        [TestMethod]
        public void Convert_WithZeroId_ReturnsSkuEntityWithZeroId()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0,
                Name = "New Product",
                UnitPrice = 19.99,
                Inventory = 50
            };

            // Act
            var result = _converter.Convert(skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(0);
            result.Name.Should().Be(skuDto.Name);
        }

        [TestMethod]
        public void Convert_WithNullName_ReturnsSkuEntityWithNullName()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 1,
                Name = null,
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _converter.Convert(skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().BeNull();
        }

        [TestMethod]
        public void Convert_WithEmptyName_ReturnsSkuEntityWithEmptyName()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 1,
                Name = string.Empty,
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _converter.Convert(skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(string.Empty);
        }

        [TestMethod]
        public void Convert_WithMaxValues_ReturnsSkuEntityWithMaxValues()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = long.MaxValue,
                Name = "Test Product",
                UnitPrice = double.MaxValue,
                Inventory = int.MaxValue
            };

            // Act
            var result = _converter.Convert(skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(long.MaxValue);
            result.UnitPrice.Should().Be(double.MaxValue);
            result.Inventory.Should().Be(int.MaxValue);
        }

        [TestMethod]
        public void Convert_WithNegativeInventory_ReturnsSkuEntityWithNegativeInventory()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 1,
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = -10
            };

            // Act
            var result = _converter.Convert(skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Inventory.Should().Be(-10);
        }
    }
}

