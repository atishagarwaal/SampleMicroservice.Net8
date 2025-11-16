using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Application.Converters;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for NotificationConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("NotificationConverter")]
    public sealed class NotificationConverterTests
    {
        private NotificationConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new NotificationConverter();
        }

        [TestMethod]
        public void Convert_WithValidNotificationDto_ReturnsNotificationEntity()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                Id = 1,
                OrderId = 123,
                CustomerId = 456,
                Message = "Test notification",
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _converter.Convert(notificationDto);

            // Assert
            result.Should().NotBeNull();
            result.NotificationId.Should().Be(notificationDto.Id);
            result.OrderId.Should().Be(notificationDto.OrderId);
            result.CustomerId.Should().Be(notificationDto.CustomerId);
            result.Message.Should().Be(notificationDto.Message);
            result.OrderDate.Should().Be(notificationDto.OrderDate);
        }

        [TestMethod]
        public void Convert_WithNullNotificationDto_ThrowsArgumentNullException()
        {
            // Arrange
            NotificationDto nullNotificationDto = null!;

            // Act
            Action act = () => _converter.Convert(nullNotificationDto);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sourceType");
        }

        [TestMethod]
        public void Convert_WithZeroId_ReturnsNotificationEntityWithZeroNotificationId()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                Id = 0,
                OrderId = 123,
                CustomerId = 456,
                Message = "Test",
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _converter.Convert(notificationDto);

            // Assert
            result.Should().NotBeNull();
            result.NotificationId.Should().Be(0);
        }

        [TestMethod]
        public void Convert_WithNullMessage_ReturnsNotificationEntityWithNullMessage()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                Id = 1,
                OrderId = 123,
                CustomerId = 456,
                Message = null!,
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _converter.Convert(notificationDto);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().BeNull();
        }

        [TestMethod]
        public void Convert_WithEmptyMessage_ReturnsNotificationEntityWithEmptyMessage()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                Id = 1,
                OrderId = 123,
                CustomerId = 456,
                Message = string.Empty,
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _converter.Convert(notificationDto);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(string.Empty);
        }
    }
}

