// <copyright file="OrderDtoValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Validation
{
    using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Validation.Interfaces;

    /// <summary>
    /// Validates incoming <see cref="OrderDto"/> message.
    /// </summary>
    public class OrderDtoValidator : IMessageValidator<OrderDto>
    {
        /// <summary>
        /// Validates <see cref="OrderDto"/> message.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <returns>The <see cref="ValidationData"/> to signify if the validation is successful or not.</returns>
        public ValidationData Validate(OrderDto message)
        {
            if (GenericValidator.IsResponseNull(message, nameof(OrderDtoValidator), out var validationData))
            {
                return validationData;
            }

            if (GenericValidator.HasInvalidIdentifier(message.CustomerId, nameof(OrderDtoValidator), out validationData))
            {
                return validationData;
            }

            if (message.OrderDate == default(DateTime))
            {
                validationData = new ValidationData(
                    validatorName: nameof(OrderDtoValidator),
                    failureReason: $"The OrderDate field must have a valid date value.");
                return validationData;
            }

            if (message.TotalAmount < 0)
            {
                validationData = new ValidationData(
                    validatorName: nameof(OrderDtoValidator),
                    failureReason: $"The TotalAmount field must be greater than or equal to zero.");
                return validationData;
            }

            return new ValidationData();
        }
    }
}

