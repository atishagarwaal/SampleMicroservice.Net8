// <copyright file="NotificationDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Dto
{
    /// <summary>
    /// Notification DTO for API responses.
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the Order Id.
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Gets or sets the Customer Id.
        /// </summary>
        public long CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        public DateTime OrderDate { get; set; }
    }
}

