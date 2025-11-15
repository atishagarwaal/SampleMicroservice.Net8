// <copyright file="CustomerDtoValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Validation
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Validation.Interfaces;

    /// <summary>
    /// Validates incoming <see cref="CustomerDto"/> message.
    /// </summary>
    public class CustomerDtoValidator : IMessageValidator<CustomerDto>
    {
        private const int MaxNameLength = 100;

        /// <summary>
        /// Validates <see cref="CustomerDto"/> message.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <returns>The <see cref="ValidationData"/> to signify if the validation is successful or not.</returns>
        public ValidationData Validate(CustomerDto message)
        {
            if (GenericValidator.IsResponseNull(message, nameof(CustomerDtoValidator), out var validationData))
            {
                return validationData;
            }

            if (GenericValidator.IsStringNullOrWhitespace(message.FirstName!, nameof(CustomerDtoValidator), nameof(message.FirstName), out validationData))
            {
                return validationData;
            }

            if (GenericValidator.ExceedsMaxLength(message.FirstName!, MaxNameLength, nameof(CustomerDtoValidator), nameof(message.FirstName), out validationData))
            {
                return validationData;
            }

            if (GenericValidator.IsStringNullOrWhitespace(message.LastName!, nameof(CustomerDtoValidator), nameof(message.LastName), out validationData))
            {
                return validationData;
            }

            if (GenericValidator.ExceedsMaxLength(message.LastName!, MaxNameLength, nameof(CustomerDtoValidator), nameof(message.LastName), out validationData))
            {
                return validationData;
            }

            return new ValidationData();
        }
    }
}
