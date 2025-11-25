// <copyright file="CustomerConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="Dto.CustomerDto"/> message to <see cref="DomainEntities.Customer"/> entity.
    /// </summary>
    public class CustomerConverter : IConverter<Dto.CustomerDto, DomainEntities.Customer>
    {
        /// <summary>
        /// Convert from <see cref="Dto.CustomerDto"/> to <see cref="DomainEntities.Customer"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="Dto.CustomerDto"/>.</param>
        /// <returns><see cref="DomainEntities.Customer"/> entity.</returns>
        public DomainEntities.Customer Convert(Dto.CustomerDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new DomainEntities.Customer
            {
                Id = sourceType.Id,
                FirstName = sourceType.FirstName,
                LastName = sourceType.LastName,
            };
        }
    }
}
