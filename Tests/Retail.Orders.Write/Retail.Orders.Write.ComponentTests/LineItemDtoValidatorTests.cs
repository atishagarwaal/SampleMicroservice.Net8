using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItemDtoValidator class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class LineItemDtoValidatorTests
    {
        private LineItemDtoValidator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new LineItemDtoValidator();
        }

        [TestMethod]
        [TestCategory("LineItemDtoValidator")]
        public void Validate_WithValidLineItemDto_ReturnsValid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                OrderId = 100,
                SkuId = 200,
                Qty = 5
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("LineItemDtoValidator")]
        public void Validate_WithNullLineItemDto_ReturnsInvalid()
        {
            // Act
            var result = _validator.Validate(null!);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.ValidatorName.Should().Be("LineItemDtoValidator");
            result.FailureReason.Should().Contain("null");
        }

        [TestMethod]
        [TestCategory("LineItemDtoValidator")]
        public void Validate_WithZeroOrderId_ReturnsValid()
        {
            // Arrange - For new line items, orderId can be 0 (will be set when order is saved)
            var lineItemDto = new LineItemDto
            {
                Id = 0, // Valid for new line items
                OrderId = 0, // Valid for new line items
                SkuId = 200, // Must be greater than 0
                Qty = 5
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("LineItemDtoValidator")]
        public void Validate_WithInvalidSkuId_ReturnsInvalid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                OrderId = 0, // Valid for new line items
                SkuId = 0, // Invalid - must be greater than 0
                Qty = 5
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.ValidatorName.Should().Be("LineItemDtoValidator");
            result.FailureReason.Should().Contain("SkuId");
            result.FailureReason.Should().Contain("greater than zero");
        }

        [TestMethod]
        [TestCategory("LineItemDtoValidator")]
        public void Validate_WithZeroQuantity_ReturnsInvalid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                OrderId = 100,
                SkuId = 200,
                Qty = 0 // Invalid
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.ValidatorName.Should().Be("LineItemDtoValidator");
            result.FailureReason.Should().Contain("Quantity");
        }

        [TestMethod]
        [TestCategory("LineItemDtoValidator")]
        public void Validate_WithNegativeQuantity_ReturnsInvalid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                OrderId = 100,
                SkuId = 200,
                Qty = -5 // Invalid
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.ValidatorName.Should().Be("LineItemDtoValidator");
            result.FailureReason.Should().Contain("Quantity");
        }

        [TestMethod]
        [TestCategory("LineItemDtoValidator")]
        public void Validate_WithPositiveQuantity_ReturnsValid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                OrderId = 100,
                SkuId = 200,
                Qty = 1 // Valid
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }
    }
}

