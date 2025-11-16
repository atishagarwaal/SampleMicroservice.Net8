// <copyright file="LineItemDtoValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Validation
{
    using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;

    /// <summary>
    /// Validates incoming <see cref="LineItemDto"/> message.
    /// </summary>
    public class LineItemDtoValidator : IMessageValidator<LineItemDto>
    {
        /// <summary>
        /// Validates <see cref="LineItemDto"/> message.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <returns>The <see cref="ValidationData"/> to signify if the validation is successful or not.</returns>
        public ValidationData Validate(LineItemDto message)
        {
            if (GenericValidator.IsResponseNull(message, nameof(LineItemDtoValidator), out var validationData))
            {
                return validationData;
            }

            // For new line items, id and orderId can be 0 (will be set when order is saved)
            // But skuId must be greater than 0 (required field)
            if (GenericValidator.HasInvalidRequiredIdentifier(message.SkuId, nameof(LineItemDtoValidator), nameof(message.SkuId), out validationData))
            {
                return validationData;
            }

            if (GenericValidator.IsLessThanOrEqualToZero(message.Qty, nameof(LineItemDtoValidator), nameof(message.Qty), out validationData))
            {
                return validationData;
            }

            return new ValidationData();
        }
    }
}

