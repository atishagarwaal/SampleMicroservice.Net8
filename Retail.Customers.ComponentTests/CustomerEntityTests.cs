using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for Customer entity.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class CustomerEntityTests
    {
        [TestMethod]
        [TestCategory("CustomerEntity")]
        public void Customer_Constructor_CreatesInstance()
        {
            // Act
            var customer = new Customer();

            // Assert
            customer.Should().NotBeNull();
            customer.Should().BeOfType<Customer>();
        }

        [TestMethod]
        [TestCategory("CustomerEntity")]
        public void Customer_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var customer = new Customer();
            var id = 1L;
            var firstName = "John";
            var lastName = "Doe";

            // Act
            customer.Id = id;
            customer.FirstName = firstName;
            customer.LastName = lastName;

            // Assert
            customer.Id.Should().Be(id);
            customer.FirstName.Should().Be(firstName);
            customer.LastName.Should().Be(lastName);
        }

        [TestMethod]
        [TestCategory("CustomerEntity")]
        public void Customer_FirstName_CanBeNull()
        {
            // Arrange
            var customer = new Customer();

            // Act
            customer.FirstName = null;

            // Assert
            customer.FirstName.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("CustomerEntity")]
        public void Customer_LastName_CanBeNull()
        {
            // Arrange
            var customer = new Customer();

            // Act
            customer.LastName = null;

            // Assert
            customer.LastName.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("CustomerEntity")]
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
        [TestCategory("CustomerEntity")]
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
        [TestCategory("CustomerEntity")]
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
            validationResults.Should().HaveCountGreaterThan(0);
        }

        [TestMethod]
        [TestCategory("CustomerEntity")]
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
            validationResults.Should().HaveCountGreaterThan(0);
        }
    }
}
