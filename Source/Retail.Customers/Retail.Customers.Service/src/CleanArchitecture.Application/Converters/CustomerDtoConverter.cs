// <copyright file="CustomerDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="DomainEntities.Customer"/> entity to <see cref="Dto.CustomerDto"/> message.
    /// </summary>
    public class CustomerDtoConverter : IConverter<DomainEntities.Customer, Dto.CustomerDto>
    {
        /// <summary>
        /// Convert from <see cref="DomainEntities.Customer"/> to <see cref="Dto.CustomerDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="DomainEntities.Customer"/>.</param>
        /// <returns><see cref="Dto.CustomerDto"/> message.</returns>
        public Dto.CustomerDto Convert(DomainEntities.Customer sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new Dto.CustomerDto
            {
                Id = sourceType.Id,
                FirstName = sourceType.FirstName,
                LastName = sourceType.LastName,
            };
        }
    }
}
