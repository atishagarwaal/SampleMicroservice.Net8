// <copyright file="CustomerConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="CustomerDto"/> message to <see cref="Customer"/> entity.
    /// </summary>
    public class CustomerConverter : IConverter<CustomerDto, Customer>
    {
        /// <summary>
        /// Convert from <see cref="CustomerDto"/> to <see cref="Customer"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="CustomerDto"/>.</param>
        /// <returns><see cref="Customer"/> entity.</returns>
        public Customer Convert(CustomerDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new Customer
            {
                Id = sourceType.Id,
                FirstName = sourceType.FirstName,
                LastName = sourceType.LastName,
            };
        }
    }
}
