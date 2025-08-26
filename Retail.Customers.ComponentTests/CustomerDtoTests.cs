using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for CustomerDto class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class CustomerDtoTests
    {
        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Constructor_CreatesInstance()
        {
            // Act
            var customerDto = new CustomerDto();

            // Assert
            customerDto.Should().NotBeNull();
            customerDto.Should().BeOfType<CustomerDto>();
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var customerDto = new CustomerDto();
            var id = 1L;
            var firstName = "John";
            var lastName = "Doe";

            // Act
            customerDto.Id = id;
            customerDto.FirstName = firstName;
            customerDto.LastName = lastName;

            // Assert
            customerDto.Id.Should().Be(id);
            customerDto.FirstName.Should().Be(firstName);
            customerDto.LastName.Should().Be(lastName);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_FirstName_CanBeNull()
        {
            // Arrange
            var customerDto = new CustomerDto();

            // Act
            customerDto.FirstName = null;

            // Assert
            customerDto.FirstName.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_LastName_CanBeNull()
        {
            // Arrange
            var customerDto = new CustomerDto();

            // Act
            customerDto.LastName = null;

            // Assert
            customerDto.LastName.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_DefaultValues_AreCorrect()
        {
            // Act
            var customerDto = new CustomerDto();

            // Assert
            customerDto.Id.Should().Be(0);
            customerDto.FirstName.Should().BeNull();
            customerDto.LastName.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_WithValidData_PropertiesAreSet()
        {
            // Arrange
            var id = 123L;
            var firstName = "Jane";
            var lastName = "Smith";

            // Act
            var customerDto = new CustomerDto
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName
            };

            // Assert
            customerDto.Id.Should().Be(id);
            customerDto.FirstName.Should().Be(firstName);
            customerDto.LastName.Should().Be(lastName);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Equality_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var customerDto1 = new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" };
            var customerDto2 = new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" };

            // Act & Assert
            customerDto1.Should().BeEquivalentTo(customerDto2);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Equality_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var customerDto1 = new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" };
            var customerDto2 = new CustomerDto { Id = 2, FirstName = "Jane", LastName = "Smith" };

            // Act & Assert
            customerDto1.Should().NotBeEquivalentTo(customerDto2);
        }
    }
}
