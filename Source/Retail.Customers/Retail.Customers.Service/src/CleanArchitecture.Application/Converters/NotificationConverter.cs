// <copyright file="NotificationConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="Dto.NotificationDto"/> message to <see cref="DomainEntities.Notification"/> entity.
    /// </summary>
    public class NotificationConverter : IConverter<Dto.NotificationDto, DomainEntities.Notification>
    {
        /// <summary>
        /// Convert from <see cref="Dto.NotificationDto"/> to <see cref="DomainEntities.Notification"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="Dto.NotificationDto"/>.</param>
        /// <returns><see cref="DomainEntities.Notification"/> entity.</returns>
        public DomainEntities.Notification Convert(Dto.NotificationDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new DomainEntities.Notification
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

