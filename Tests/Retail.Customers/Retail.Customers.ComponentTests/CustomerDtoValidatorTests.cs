using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Application.Validation;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for CustomerDtoValidator class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("CustomerDtoValidator")]
    public sealed class CustomerDtoValidatorTests
    {
        private CustomerDtoValidator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new CustomerDtoValidator();
        }

        [TestMethod]
        public void Validate_WithValidCustomerDto_ReturnsValid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(customerDto);

            // Assert
            result.IsValid.Should().BeTrue();
            result.FailureReason.Should().BeNull();
        }

        [TestMethod]
        public void Validate_WithNullCustomerDto_ReturnsInvalid()
        {
            // Arrange
            CustomerDto nullCustomerDto = null!;

            // Act
            var result = _validator.Validate(nullCustomerDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("message being validated is null");
            result.ValidatorName.Should().Be(nameof(CustomerDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNullFirstName_ReturnsInvalid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = null!,
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(customerDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("FirstName");
            result.FailureReason.Should().Contain("null or whitespace");
            result.ValidatorName.Should().Be(nameof(CustomerDtoValidator));
        }

        [TestMethod]
        public void Validate_WithEmptyFirstName_ReturnsInvalid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = string.Empty,
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(customerDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("FirstName");
            result.ValidatorName.Should().Be(nameof(CustomerDtoValidator));
        }

        [TestMethod]
        public void Validate_WithWhitespaceFirstName_ReturnsInvalid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = "   ",
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(customerDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("FirstName");
            result.ValidatorName.Should().Be(nameof(CustomerDtoValidator));
        }

        [TestMethod]
        public void Validate_WithFirstNameExceedingMaxLength_ReturnsInvalid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = new string('A', 101), // Exceeds max length of 100
                LastName = "Doe"
            };

            // Act
            var result = _validator.Validate(customerDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("FirstName");
            result.FailureReason.Should().Contain("maximum length");
            result.ValidatorName.Should().Be(nameof(CustomerDtoValidator));
        }

        [TestMethod]
        public void Validate_WithNullLastName_ReturnsInvalid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = "John",
                LastName = null!
            };

            // Act
            var result = _validator.Validate(customerDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("LastName");
            result.FailureReason.Should().Contain("null or whitespace");
            result.ValidatorName.Should().Be(nameof(CustomerDtoValidator));
        }

        [TestMethod]
        public void Validate_WithEmptyLastName_ReturnsInvalid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = "John",
                LastName = string.Empty
            };

            // Act
            var result = _validator.Validate(customerDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("LastName");
            result.ValidatorName.Should().Be(nameof(CustomerDtoValidator));
        }

        [TestMethod]
        public void Validate_WithWhitespaceLastName_ReturnsInvalid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "   "
            };

            // Act
            var result = _validator.Validate(customerDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("LastName");
            result.ValidatorName.Should().Be(nameof(CustomerDtoValidator));
        }

        [TestMethod]
        public void Validate_WithLastNameExceedingMaxLength_ReturnsInvalid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = "John",
                LastName = new string('A', 101) // Exceeds max length of 100
            };

            // Act
            var result = _validator.Validate(customerDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.FailureReason.Should().Contain("LastName");
            result.FailureReason.Should().Contain("maximum length");
            result.ValidatorName.Should().Be(nameof(CustomerDtoValidator));
        }
    }
}

