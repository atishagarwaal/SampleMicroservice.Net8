// <copyright file="NotificationConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="NotificationDto"/> message to <see cref="Notification"/> entity.
    /// </summary>
    public class NotificationConverter : IConverter<NotificationDto, Notification>
    {
        /// <summary>
        /// Convert from <see cref="NotificationDto"/> to <see cref="Notification"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="NotificationDto"/>.</param>
        /// <returns><see cref="Notification"/> entity.</returns>
        public Notification Convert(NotificationDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new Notification
            {
                NotificationId = sourceType.Id,
                OrderId = sourceType.OrderId,
                CustomerId = sourceType.CustomerId,
                Message = sourceType.Message,
                OrderDate = sourceType.OrderDate,
            };
        }
    }
}

