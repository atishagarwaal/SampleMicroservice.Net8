using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for Notification entity.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class NotificationEntityTests
    {
        [TestMethod]
        [TestCategory("NotificationEntity")]
        public void Notification_Constructor_CreatesInstance()
        {
            // Act
            var notification = new Notification();

            // Assert
            notification.Should().NotBeNull();
            notification.Should().BeOfType<Notification>();
        }

        [TestMethod]
        [TestCategory("NotificationEntity")]
        public void Notification_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var notification = new Notification();
            var orderId = 123L;
            var customerId = 456L;
            var message = "Test notification message";
            var orderDate = DateTime.UtcNow;

            // Act
            notification.OrderId = orderId;
            notification.CustomerId = customerId;
            notification.Message = message;
            notification.OrderDate = orderDate;

            // Assert
            notification.OrderId.Should().Be(orderId);
            notification.CustomerId.Should().Be(customerId);
            notification.Message.Should().Be(message);
            notification.OrderDate.Should().Be(orderDate);
        }

        [TestMethod]
        [TestCategory("NotificationEntity")]
        public void Notification_DefaultValues_AreCorrect()
        {
            // Act
            var notification = new Notification();

            // Assert
            notification.OrderId.Should().Be(0);
            notification.CustomerId.Should().Be(0);
            notification.Message.Should().BeNull();
            notification.OrderDate.Should().Be(default(DateTime));
        }

        [TestMethod]
        [TestCategory("NotificationEntity")]
        public void Notification_WithValidData_PropertiesAreSet()
        {
            // Arrange
            var orderId = 789L;
            var customerId = 101L;
            var message = "Order processed successfully";
            var orderDate = DateTime.UtcNow.AddDays(-1);

            // Act
            var notification = new Notification
            {
                OrderId = orderId,
                CustomerId = customerId,
                Message = message,
                OrderDate = orderDate
            };

            // Assert
            notification.OrderId.Should().Be(orderId);
            notification.CustomerId.Should().Be(customerId);
            notification.Message.Should().Be(message);
            notification.OrderDate.Should().Be(orderDate);
        }

        [TestMethod]
        [TestCategory("NotificationEntity")]
        public void Notification_Message_CanBeNull()
        {
            // Arrange
            var notification = new Notification();

            // Act
            notification.Message = null;

            // Assert
            notification.Message.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("NotificationEntity")]
        public void Notification_Equality_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var date = DateTime.UtcNow;
            var notification1 = new Notification
            {
                OrderId = 1,
                CustomerId = 2,
                Message = "Test",
                OrderDate = date
            };
            var notification2 = new Notification
            {
                OrderId = 1,
                CustomerId = 2,
                Message = "Test",
                OrderDate = date
            };

            // Act & Assert
            notification1.Should().BeEquivalentTo(notification2);
        }

        [TestMethod]
        [TestCategory("NotificationEntity")]
        public void Notification_Equality_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var notification1 = new Notification
            {
                OrderId = 1,
                CustomerId = 2,
                Message = "Test",
                OrderDate = DateTime.UtcNow
            };
            var notification2 = new Notification
            {
                OrderId = 2,
                CustomerId = 3,
                Message = "Different",
                OrderDate = DateTime.UtcNow.AddDays(1)
            };

            // Act & Assert
            notification1.Should().NotBeEquivalentTo(notification2);
        }

        [TestMethod]
        [TestCategory("NotificationEntity")]
        public void Notification_OrderDate_CanBeSetToFuture()
        {
            // Arrange
            var notification = new Notification();
            var futureDate = DateTime.UtcNow.AddDays(1);

            // Act
            notification.OrderDate = futureDate;

            // Assert
            notification.OrderDate.Should().Be(futureDate);
        }

        [TestMethod]
        [TestCategory("NotificationEntity")]
        public void Notification_OrderDate_CanBeSetToPast()
        {
            // Arrange
            var notification = new Notification();
            var pastDate = DateTime.UtcNow.AddDays(-1);

            // Act
            notification.OrderDate = pastDate;

            // Assert
            notification.OrderDate.Should().Be(pastDate);
        }
    }
}
