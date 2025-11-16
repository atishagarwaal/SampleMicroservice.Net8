using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Application.Converters;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for NotificationDtoConverter class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("NotificationDtoConverter")]
    public sealed class NotificationDtoConverterTests
    {
        private NotificationDtoConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new NotificationDtoConverter();
        }

        [TestMethod]
        public void Convert_WithValidNotification_ReturnsNotificationDto()
        {
            // Arrange
            var notification = new Notification
            {
                NotificationId = 1,
                OrderId = 123,
                CustomerId = 456,
                Message = "Test notification",
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _converter.Convert(notification);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(notification.NotificationId);
            result.OrderId.Should().Be(notification.OrderId);
            result.CustomerId.Should().Be(notification.CustomerId);
            result.Message.Should().Be(notification.Message);
            result.OrderDate.Should().Be(notification.OrderDate);
        }

        [TestMethod]
        public void Convert_WithNullNotification_ThrowsArgumentNullException()
        {
            // Arrange
            Notification nullNotification = null!;

            // Act
            Action act = () => _converter.Convert(nullNotification);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sourceType");
        }

        [TestMethod]
        public void Convert_WithZeroNotificationId_ReturnsNotificationDtoWithZeroId()
        {
            // Arrange
            var notification = new Notification
            {
                NotificationId = 0,
                OrderId = 123,
                CustomerId = 456,
                Message = "Test",
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _converter.Convert(notification);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(0);
        }

        [TestMethod]
        public void Convert_WithNullMessage_ReturnsNotificationDtoWithNullMessage()
        {
            // Arrange
            var notification = new Notification
            {
                NotificationId = 1,
                OrderId = 123,
                CustomerId = 456,
                Message = null!,
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _converter.Convert(notification);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().BeNull();
        }

        [TestMethod]
        public void Convert_WithEmptyMessage_ReturnsNotificationDtoWithEmptyMessage()
        {
            // Arrange
            var notification = new Notification
            {
                NotificationId = 1,
                OrderId = 123,
                CustomerId = 456,
                Message = string.Empty,
                OrderDate = DateTime.UtcNow
            };

            // Act
            var result = _converter.Convert(notification);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(string.Empty);
        }
    }
}

