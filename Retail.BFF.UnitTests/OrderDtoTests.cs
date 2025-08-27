using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.BFFWeb.Api.Model;

namespace Retail.BFF.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class OrderDtoTests
    {
        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Constructor_CreatesInstance()
        {
            // Act
            var orderDto = new OrderDto();

            // Assert
            orderDto.Should().NotBeNull();
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var orderDto = new OrderDto();
            var expectedId = 1L;
            var expectedCustomerId = 100L;
            var expectedOrderDate = DateTime.Now;
            var expectedTotalAmount = 99.99;

            // Act
            orderDto.Id = expectedId;
            orderDto.CustomerId = expectedCustomerId;
            orderDto.OrderDate = expectedOrderDate;
            orderDto.TotalAmount = expectedTotalAmount;

            // Assert
            orderDto.Id.Should().Be(expectedId);
            orderDto.CustomerId.Should().Be(expectedCustomerId);
            orderDto.OrderDate.Should().Be(expectedOrderDate);
            orderDto.TotalAmount.Should().Be(expectedTotalAmount);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetToZero()
        {
            // Arrange
            var orderDto = new OrderDto();

            // Act
            orderDto.Id = 0;
            orderDto.CustomerId = 0;
            orderDto.TotalAmount = 0.0;

            // Assert
            orderDto.Id.Should().Be(0);
            orderDto.CustomerId.Should().Be(0);
            orderDto.TotalAmount.Should().Be(0.0);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetToNegativeValues()
        {
            // Arrange
            var orderDto = new OrderDto();

            // Act
            orderDto.Id = -1;
            orderDto.CustomerId = -100;
            orderDto.TotalAmount = -50.0;

            // Assert
            orderDto.Id.Should().Be(-1);
            orderDto.CustomerId.Should().Be(-100);
            orderDto.TotalAmount.Should().Be(-50.0);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetToLargeValues()
        {
            // Arrange
            var orderDto = new OrderDto();
            var largeId = long.MaxValue;
            var largeAmount = double.MaxValue;

            // Act
            orderDto.Id = largeId;
            orderDto.TotalAmount = largeAmount;

            // Assert
            orderDto.Id.Should().Be(largeId);
            orderDto.TotalAmount.Should().Be(largeAmount);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetToMinDateTime()
        {
            // Arrange
            var orderDto = new OrderDto();
            var minDate = DateTime.MinValue;

            // Act
            orderDto.OrderDate = minDate;

            // Assert
            orderDto.OrderDate.Should().Be(minDate);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetToMaxDateTime()
        {
            // Arrange
            var orderDto = new OrderDto();
            var maxDate = DateTime.MaxValue;

            // Act
            orderDto.OrderDate = maxDate;

            // Assert
            orderDto.OrderDate.Should().Be(maxDate);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetToUtcDateTime()
        {
            // Arrange
            var orderDto = new OrderDto();
            var utcDate = DateTime.UtcNow;

            // Act
            orderDto.OrderDate = utcDate;

            // Assert
            orderDto.OrderDate.Should().Be(utcDate);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetToDecimalAmounts()
        {
            // Arrange
            var orderDto = new OrderDto();
            var decimalAmount = 123.45;

            // Act
            orderDto.TotalAmount = decimalAmount;

            // Assert
            orderDto.TotalAmount.Should().Be(decimalAmount);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetToPreciseAmounts()
        {
            // Arrange
            var orderDto = new OrderDto();
            var preciseAmount = 0.01;

            // Act
            orderDto.TotalAmount = preciseAmount;

            // Assert
            orderDto.TotalAmount.Should().Be(preciseAmount);
        }

        [TestMethod]
        [TestCategory("OrderDto")]
        public void OrderDto_Properties_CanBeSetToZeroAmount()
        {
            // Arrange
            var orderDto = new OrderDto();

            // Act
            orderDto.TotalAmount = 0.0;

            // Assert
            orderDto.TotalAmount.Should().Be(0.0);
        }
    }
}
