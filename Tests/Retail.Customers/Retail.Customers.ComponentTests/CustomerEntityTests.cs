using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for Customer entity validation attributes.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("CustomerEntity")]
    public sealed class CustomerEntityTests
    {
        [TestMethod]
        public void Customer_ValidationAttributes_AreApplied()
        {
            // Arrange
            var customer = new Customer();
            var validationContext = new ValidationContext(customer);

            // Act & Assert
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(customer, validationContext, validationResults, true);

            // Customer without required fields should not be valid
            isValid.Should().BeFalse();
            validationResults.Should().HaveCountGreaterThan(0);
        }

        [TestMethod]
        public void Customer_WithValidData_IsValid()
        {
            // Arrange
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe"
            };
            var validationContext = new ValidationContext(customer);

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(customer, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [TestMethod]
        public void Customer_FirstNameExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var customer = new Customer
            {
                FirstName = new string('A', 101), // Exceeds MaxLength(100)
                LastName = "Doe"
            };
            var validationContext = new ValidationContext(customer);

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(customer, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Customer.FirstName)));
        }

        [TestMethod]
        public void Customer_LastNameExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var customer = new Customer
            {
                FirstName = "John",
                LastName = new string('A', 101) // Exceeds MaxLength(100)
            };
            var validationContext = new ValidationContext(customer);

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(customer, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Customer.LastName)));
        }
    }
}
