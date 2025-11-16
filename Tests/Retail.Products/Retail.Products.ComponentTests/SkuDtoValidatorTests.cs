using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Validation;

namespace Retail.Products.ComponentTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class SkuDtoValidatorTests
    {
        private SkuDtoValidator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new SkuDtoValidator();
        }

        [TestMethod]
        public void Validate_WithValidSkuDto_ReturnsValid()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0, // Valid for new products
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _validator.Validate(skuDto);

            // Assert
            result.IsValid.Should().BeTrue();
            result.FailureReason.Should().BeNull();
        }

        [TestMethod]
        public void Validate_WithNullSkuDto_ReturnsInvalid()
        {
            // Arrange
            SkuDto nullSkuDto = null!;

            // Act
            var result = _validator.Validate(nullSkuDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("message being validated is null");
            result.ValidatorName.Should().Be(nameof(SkuDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNullName_ReturnsInvalid()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0,
                Name = null,
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _validator.Validate(skuDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("Name");
            result.FailureReason.Should().Contain("null or whitespace");
            result.ValidatorName.Should().Be(nameof(SkuDtoValidator));
        }

        [TestMethod]
        public void Validate_WithEmptyName_ReturnsInvalid()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0,
                Name = string.Empty,
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _validator.Validate(skuDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("Name");
            result.ValidatorName.Should().Be(nameof(SkuDtoValidator));
        }

        [TestMethod]
        public void Validate_WithWhitespaceName_ReturnsInvalid()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0,
                Name = "   ",
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _validator.Validate(skuDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("Name");
            result.ValidatorName.Should().Be(nameof(SkuDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNameExceedingMaxLength_ReturnsInvalid()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0,
                Name = new string('A', 101), // Exceeds max length of 100
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = _validator.Validate(skuDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("Name");
            result.FailureReason.Should().Contain("maximum length");
            result.ValidatorName.Should().Be(nameof(SkuDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNegativeUnitPrice_ReturnsInvalid()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0,
                Name = "Test Product",
                UnitPrice = -10.00,
                Inventory = 100
            };

            // Act
            var result = _validator.Validate(skuDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("UnitPrice");
            result.FailureReason.Should().Contain("greater than or equal to zero");
            result.ValidatorName.Should().Be(nameof(SkuDtoValidator));
        }

        [TestMethod]
        public void Validate_WithZeroUnitPrice_ReturnsValid()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0,
                Name = "Test Product",
                UnitPrice = 0.00,
                Inventory = 100
            };

            // Act
            var result = _validator.Validate(skuDto);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_WithNegativeInventory_ReturnsInvalid()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0,
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = -10
            };

            // Act
            var result = _validator.Validate(skuDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("Inventory");
            result.FailureReason.Should().Contain("greater than or equal to zero");
            result.ValidatorName.Should().Be(nameof(SkuDtoValidator));
        }

        [TestMethod]
        public void Validate_WithZeroInventory_ReturnsValid()
        {
            // Arrange
            var skuDto = new SkuDto
            {
                Id = 0,
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = 0
            };

            // Act
            var result = _validator.Validate(skuDto);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}

