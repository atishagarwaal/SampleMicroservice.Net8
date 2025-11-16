using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Application.Converters;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for CustomerConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("CustomerConverter")]
    public sealed class CustomerConverterTests
    {
        private CustomerConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new CustomerConverter();
        }

        [TestMethod]
        public void Convert_WithValidCustomerDto_ReturnsCustomerEntity()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _converter.Convert(customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(customerDto.Id);
            result.FirstName.Should().Be(customerDto.FirstName);
            result.LastName.Should().Be(customerDto.LastName);
        }

        [TestMethod]
        public void Convert_WithNullCustomerDto_ThrowsArgumentNullException()
        {
            // Arrange
            CustomerDto nullCustomerDto = null!;

            // Act
            Action act = () => _converter.Convert(nullCustomerDto);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sourceType");
        }

        [TestMethod]
        public void Convert_WithZeroId_ReturnsCustomerEntityWithZeroId()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 0,
                FirstName = "New",
                LastName = "Customer"
            };

            // Act
            var result = _converter.Convert(customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(0);
            result.FirstName.Should().Be(customerDto.FirstName);
            result.LastName.Should().Be(customerDto.LastName);
        }

        [TestMethod]
        public void Convert_WithNullName_ReturnsCustomerEntityWithNullName()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = null,
                LastName = null
            };

            // Act
            var result = _converter.Convert(customerDto);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().BeNull();
            result.LastName.Should().BeNull();
        }

        [TestMethod]
        public void Convert_WithEmptyName_ReturnsCustomerEntityWithEmptyName()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = string.Empty,
                LastName = string.Empty
            };

            // Act
            var result = _converter.Convert(customerDto);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be(string.Empty);
            result.LastName.Should().Be(string.Empty);
        }
    }
}

