// <copyright file="NotificationDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="DomainEntities.Notification"/> entity to <see cref="Dto.NotificationDto"/> message.
    /// </summary>
    public class NotificationDtoConverter : IConverter<DomainEntities.Notification, Dto.NotificationDto>
    {
        /// <summary>
        /// Convert from <see cref="DomainEntities.Notification"/> to <see cref="Dto.NotificationDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="DomainEntities.Notification"/>.</param>
        /// <returns><see cref="Dto.NotificationDto"/> message.</returns>
        public Dto.NotificationDto Convert(DomainEntities.Notification sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new Dto.NotificationDto
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

