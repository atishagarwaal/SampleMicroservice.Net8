using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for Notification entity validation attributes.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("NotificationEntity")]
    public sealed class NotificationEntityTests
    {
        [TestMethod]
        public void Notification_WithValidData_IsValid()
        {
            // Arrange
            var notification = new Notification
            {
                OrderId = 123L,
                CustomerId = 456L,
                Message = "Test notification message",
                OrderDate = DateTime.UtcNow
            };
            var validationContext = new ValidationContext(notification);

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(notification, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }
    }
}
