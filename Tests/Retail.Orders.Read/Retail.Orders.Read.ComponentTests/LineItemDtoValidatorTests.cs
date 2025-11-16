using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Application.Validation;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for LineItemDtoValidator class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("LineItemDtoValidator")]
    public sealed class LineItemDtoValidatorTests
    {
        private LineItemDtoValidator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new LineItemDtoValidator();
        }

        [TestMethod]
        public void Validate_WithValidLineItemDto_ReturnsValid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = 100,
                SkuId = 200,
                Qty = 5
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.IsValid.Should().BeTrue();
            result.FailureReason.Should().BeNull();
        }

        [TestMethod]
        public void Validate_WithNullLineItemDto_ReturnsInvalid()
        {
            // Arrange
            LineItemDto nullLineItemDto = null!;

            // Act
            var result = _validator.Validate(nullLineItemDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("message being validated is null");
            result.ValidatorName.Should().Be(nameof(LineItemDtoValidator));
        }

        [TestMethod]
        public void Validate_WithZeroOrderId_ReturnsInvalid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = 0,
                SkuId = 200,
                Qty = 5
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("identifier does not have a valid value");
            result.ValidatorName.Should().Be(nameof(LineItemDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNegativeOrderId_ReturnsInvalid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = -1,
                SkuId = 200,
                Qty = 5
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("identifier does not have a valid value");
            result.ValidatorName.Should().Be(nameof(LineItemDtoValidator));
        }

        [TestMethod]
        public void Validate_WithZeroSkuId_ReturnsInvalid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = 100,
                SkuId = 0,
                Qty = 5
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("identifier does not have a valid value");
            result.ValidatorName.Should().Be(nameof(LineItemDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNegativeSkuId_ReturnsInvalid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = 100,
                SkuId = -1,
                Qty = 5
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("identifier does not have a valid value");
            result.ValidatorName.Should().Be(nameof(LineItemDtoValidator));
        }

        [TestMethod]
        public void Validate_WithZeroQty_ReturnsInvalid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = 100,
                SkuId = 200,
                Qty = 0
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("Qty");
            result.FailureReason.Should().Contain("greater than zero");
            result.ValidatorName.Should().Be(nameof(LineItemDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNegativeQty_ReturnsInvalid()
        {
            // Arrange
            var lineItemDto = new LineItemDto
            {
                Id = 1,
                OrderId = 100,
                SkuId = 200,
                Qty = -5
            };

            // Act
            var result = _validator.Validate(lineItemDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("Qty");
            result.FailureReason.Should().Contain("greater than zero");
            result.ValidatorName.Should().Be(nameof(LineItemDtoValidator));
        }
    }
}

