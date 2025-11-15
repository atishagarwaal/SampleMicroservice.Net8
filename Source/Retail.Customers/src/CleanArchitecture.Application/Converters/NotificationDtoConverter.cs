// <copyright file="NotificationDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="Notification"/> entity to <see cref="NotificationDto"/> message.
    /// </summary>
    public class NotificationDtoConverter : IConverter<Notification, NotificationDto>
    {
        /// <summary>
        /// Convert from <see cref="Notification"/> to <see cref="NotificationDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="Notification"/>.</param>
        /// <returns><see cref="NotificationDto"/> message.</returns>
        public NotificationDto Convert(Notification sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new NotificationDto
            {
                Id = sourceType.NotificationId,
                OrderId = sourceType.OrderId,
                CustomerId = sourceType.CustomerId,
                Message = sourceType.Message,
                OrderDate = sourceType.OrderDate,
            };
        }
    }
}

