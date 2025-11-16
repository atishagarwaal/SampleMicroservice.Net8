using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Application.Validation;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for OrderDtoValidator class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("OrderDtoValidator")]
    public sealed class OrderDtoValidatorTests
    {
        private OrderDtoValidator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new OrderDtoValidator();
        }

        [TestMethod]
        public void Validate_WithValidOrderDto_ReturnsValid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00
            };

            // Act
            var result = _validator.Validate(orderDto);

            // Assert
            result.IsValid.Should().BeTrue();
            result.FailureReason.Should().BeNull();
        }

        [TestMethod]
        public void Validate_WithNullOrderDto_ReturnsInvalid()
        {
            // Arrange
            OrderDto nullOrderDto = null!;

            // Act
            var result = _validator.Validate(nullOrderDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("message being validated is null");
            result.ValidatorName.Should().Be(nameof(OrderDtoValidator));
        }

        [TestMethod]
        public void Validate_WithZeroCustomerId_ReturnsInvalid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 0,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00
            };

            // Act
            var result = _validator.Validate(orderDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("identifier does not have a valid value");
            result.ValidatorName.Should().Be(nameof(OrderDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNegativeCustomerId_ReturnsInvalid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = -1,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00
            };

            // Act
            var result = _validator.Validate(orderDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("identifier does not have a valid value");
            result.ValidatorName.Should().Be(nameof(OrderDtoValidator));
        }

        [TestMethod]
        public void Validate_WithDefaultOrderDate_ReturnsInvalid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = default(DateTime),
                TotalAmount = 150.00
            };

            // Act
            var result = _validator.Validate(orderDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("OrderDate");
            result.FailureReason.Should().Contain("valid date value");
            result.ValidatorName.Should().Be(nameof(OrderDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNegativeTotalAmount_ReturnsInvalid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = -10.00
            };

            // Act
            var result = _validator.Validate(orderDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("TotalAmount");
            result.FailureReason.Should().Contain("greater than or equal to zero");
            result.ValidatorName.Should().Be(nameof(OrderDtoValidator));
        }

        [TestMethod]
        public void Validate_WithZeroTotalAmount_ReturnsValid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 0.00
            };

            // Act
            var result = _validator.Validate(orderDto);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}

