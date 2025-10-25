using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.BFFWeb.Api.Model;

namespace Retail.BFF.UnitTests
{
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
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var skuDto = new SkuDto();
            var expectedId = 1L;
            var expectedName = "Test Product";
            var expectedUnitPrice = 29.99;

            // Act
            skuDto.Id = expectedId;
            skuDto.Name = expectedName;
            skuDto.UnitPrice = expectedUnitPrice;

            // Assert
            skuDto.Id.Should().Be(expectedId);
            skuDto.Name.Should().Be(expectedName);
            skuDto.UnitPrice.Should().Be(expectedUnitPrice);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetToNull()
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
        public void SkuDto_Properties_CanBeSetToZero()
        {
            // Arrange
            var skuDto = new SkuDto();

            // Act
            skuDto.Id = 0;
            skuDto.UnitPrice = 0.0;

            // Assert
            skuDto.Id.Should().Be(0);
            skuDto.UnitPrice.Should().Be(0.0);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetToNegativeValues()
        {
            // Arrange
            var skuDto = new SkuDto();

            // Act
            skuDto.Id = -1;
            skuDto.UnitPrice = -50.0;

            // Assert
            skuDto.Id.Should().Be(-1);
            skuDto.UnitPrice.Should().Be(-50.0);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetToLargeValues()
        {
            // Arrange
            var skuDto = new SkuDto();
            var largeId = long.MaxValue;
            var largePrice = double.MaxValue;

            // Act
            skuDto.Id = largeId;
            skuDto.UnitPrice = largePrice;

            // Assert
            skuDto.Id.Should().Be(largeId);
            skuDto.UnitPrice.Should().Be(largePrice);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetToEmptyString()
        {
            // Arrange
            var skuDto = new SkuDto();

            // Act
            skuDto.Name = string.Empty;

            // Assert
            skuDto.Name.Should().Be(string.Empty);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetToWhitespace()
        {
            // Arrange
            var skuDto = new SkuDto();

            // Act
            skuDto.Name = "   ";

            // Assert
            skuDto.Name.Should().Be("   ");
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetToSpecialCharacters()
        {
            // Arrange
            var skuDto = new SkuDto();
            var specialName = "Product@#$%^&*()";

            // Act
            skuDto.Name = specialName;

            // Assert
            skuDto.Name.Should().Be(specialName);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetToUnicodeCharacters()
        {
            // Arrange
            var skuDto = new SkuDto();
            var unicodeName = "Product with émojis 🚀";

            // Act
            skuDto.Name = unicodeName;

            // Assert
            skuDto.Name.Should().Be(unicodeName);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetToDecimalPrices()
        {
            // Arrange
            var skuDto = new SkuDto();
            var decimalPrice = 123.45;

            // Act
            skuDto.UnitPrice = decimalPrice;

            // Assert
            skuDto.UnitPrice.Should().Be(decimalPrice);
        }

        [TestMethod]
        [TestCategory("SkuDto")]
        public void SkuDto_Properties_CanBeSetToPrecisePrices()
        {
            // Arrange
            var skuDto = new SkuDto();
            var precisePrice = 0.01;

            // Act
            skuDto.UnitPrice = precisePrice;

            // Assert
            skuDto.UnitPrice.Should().Be(precisePrice);
        }
    }
}
