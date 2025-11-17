// <copyright file="CustomerDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="Customer"/> entity to <see cref="CustomerDto"/> message.
    /// </summary>
    public class CustomerDtoConverter : IConverter<Customer, CustomerDto>
    {
        /// <summary>
        /// Convert from <see cref="Customer"/> to <see cref="CustomerDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="Customer"/>.</param>
        /// <returns><see cref="CustomerDto"/> message.</returns>
        public CustomerDto Convert(Customer sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new CustomerDto
            {
                Id = sourceType.Id,
                FirstName = sourceType.FirstName,
                LastName = sourceType.LastName,
            };
        }
    }
}
