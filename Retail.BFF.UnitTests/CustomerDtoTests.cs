using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.BFFWeb.Api.Model;

namespace Retail.BFF.UnitTests
{
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
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var customerDto = new CustomerDto();
            var expectedId = 1L;
            var expectedFirstName = "John";
            var expectedLastName = "Doe";

            // Act
            customerDto.Id = expectedId;
            customerDto.FirstName = expectedFirstName;
            customerDto.LastName = expectedLastName;

            // Assert
            customerDto.Id.Should().Be(expectedId);
            customerDto.FirstName.Should().Be(expectedFirstName);
            customerDto.LastName.Should().Be(expectedLastName);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToNull()
        {
            // Arrange
            var customerDto = new CustomerDto();

            // Act
            customerDto.FirstName = null;
            customerDto.LastName = null;

            // Assert
            customerDto.FirstName.Should().BeNull();
            customerDto.LastName.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToZero()
        {
            // Arrange
            var customerDto = new CustomerDto();

            // Act
            customerDto.Id = 0;

            // Assert
            customerDto.Id.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToNegativeValues()
        {
            // Arrange
            var customerDto = new CustomerDto();

            // Act
            customerDto.Id = -1;

            // Assert
            customerDto.Id.Should().Be(-1);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToLargeValues()
        {
            // Arrange
            var customerDto = new CustomerDto();
            var largeId = long.MaxValue;

            // Act
            customerDto.Id = largeId;

            // Assert
            customerDto.Id.Should().Be(largeId);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToEmptyString()
        {
            // Arrange
            var customerDto = new CustomerDto();

            // Act
            customerDto.FirstName = string.Empty;
            customerDto.LastName = string.Empty;

            // Assert
            customerDto.FirstName.Should().Be(string.Empty);
            customerDto.LastName.Should().Be(string.Empty);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToWhitespace()
        {
            // Arrange
            var customerDto = new CustomerDto();

            // Act
            customerDto.FirstName = "   ";
            customerDto.LastName = "   ";

            // Assert
            customerDto.FirstName.Should().Be("   ");
            customerDto.LastName.Should().Be("   ");
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToSpecialCharacters()
        {
            // Arrange
            var customerDto = new CustomerDto();
            var specialFirstName = "John@#$%^&*()";
            var specialLastName = "Doe@#$%^&*()";

            // Act
            customerDto.FirstName = specialFirstName;
            customerDto.LastName = specialLastName;

            // Assert
            customerDto.FirstName.Should().Be(specialFirstName);
            customerDto.LastName.Should().Be(specialLastName);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToUnicodeCharacters()
        {
            // Arrange
            var customerDto = new CustomerDto();
            var unicodeFirstName = "José";
            var unicodeLastName = "García";

            // Act
            customerDto.FirstName = unicodeFirstName;
            customerDto.LastName = unicodeLastName;

            // Assert
            customerDto.FirstName.Should().Be(unicodeFirstName);
            customerDto.LastName.Should().Be(unicodeLastName);
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToSingleCharacter()
        {
            // Arrange
            var customerDto = new CustomerDto();

            // Act
            customerDto.FirstName = "J";
            customerDto.LastName = "D";

            // Assert
            customerDto.FirstName.Should().Be("J");
            customerDto.LastName.Should().Be("D");
        }

        [TestMethod]
        [TestCategory("CustomerDto")]
        public void CustomerDto_Properties_CanBeSetToVeryLongNames()
        {
            // Arrange
            var customerDto = new CustomerDto();
            var longFirstName = new string('A', 1000);
            var longLastName = new string('B', 1000);

            // Act
            customerDto.FirstName = longFirstName;
            customerDto.LastName = longLastName;

            // Assert
            customerDto.FirstName.Should().Be(longFirstName);
            customerDto.LastName.Should().Be(longLastName);
        }
    }
}
