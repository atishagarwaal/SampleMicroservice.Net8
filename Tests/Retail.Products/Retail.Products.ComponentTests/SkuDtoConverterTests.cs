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
    public class SkuDtoConverterTests
    {
        private SkuDtoConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new SkuDtoConverter();
        }

        [TestMethod]
        public void Convert_WithValidSkuEntity_ReturnsSkuDto()
        {
            // Arrange
            var sku = new Sku
            {
                Id = 1,
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _converter.Convert(sku);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(sku.Id);
            result.Name.Should().Be(sku.Name);
            result.UnitPrice.Should().Be(sku.UnitPrice);
            result.Inventory.Should().Be(sku.Inventory);
        }

        [TestMethod]
        public void Convert_WithNullSkuEntity_ThrowsArgumentNullException()
        {
            // Arrange
            Sku nullSku = null!;

            // Act
            Action act = () => _converter.Convert(nullSku);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sourceType");
        }

        [TestMethod]
        public void Convert_WithZeroId_ReturnsSkuDtoWithZeroId()
        {
            // Arrange
            var sku = new Sku
            {
                Id = 0,
                Name = "New Product",
                UnitPrice = 19.99,
                Inventory = 50
            };

            // Act
            var result = _converter.Convert(sku);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(0);
            result.Name.Should().Be(sku.Name);
        }

        [TestMethod]
        public void Convert_WithNullName_ReturnsSkuDtoWithNullName()
        {
            // Arrange
            var sku = new Sku
            {
                Id = 1,
                Name = null,
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _converter.Convert(sku);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().BeNull();
        }
    }
}

