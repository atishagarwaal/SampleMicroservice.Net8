using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Application.Converters;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for CustomerDtoConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("CustomerDtoConverter")]
    public sealed class CustomerDtoConverterTests
    {
        private CustomerDtoConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new CustomerDtoConverter();
        }

        [TestMethod]
        public void Convert_WithValidCustomer_ReturnsCustomerDto()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _converter.Convert(customer);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(customer.Id);
            result.FirstName.Should().Be(customer.FirstName);
            result.LastName.Should().Be(customer.LastName);
        }

        [TestMethod]
        public void Convert_WithNullCustomer_ThrowsArgumentNullException()
        {
            // Arrange
            Customer nullCustomer = null!;

            // Act
            Action act = () => _converter.Convert(nullCustomer);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sourceType");
        }

        [TestMethod]
        public void Convert_WithZeroId_ReturnsCustomerDtoWithZeroId()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 0,
                FirstName = "New",
                LastName = "Customer"
            };

            // Act
            var result = _converter.Convert(customer);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(0);
            result.FirstName.Should().Be(customer.FirstName);
            result.LastName.Should().Be(customer.LastName);
        }

        [TestMethod]
        public void Convert_WithNullName_ReturnsCustomerDtoWithNullName()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                FirstName = null,
                LastName = null
            };

            // Act
            var result = _converter.Convert(customer);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().BeNull();
            result.LastName.Should().BeNull();
        }

        [TestMethod]
        public void Convert_WithEmptyName_ReturnsCustomerDtoWithEmptyName()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                FirstName = string.Empty,
                LastName = string.Empty
            };

            // Act
            var result = _converter.Convert(customer);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be(string.Empty);
            result.LastName.Should().Be(string.Empty);
        }
    }
}

