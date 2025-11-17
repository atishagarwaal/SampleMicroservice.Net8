// <copyright file="SkuConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Products.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="SkuDto"/> message to <see cref="Sku"/> entity.
    /// </summary>
    public class SkuConverter : IConverter<SkuDto, Sku>
    {
        /// <summary>
        /// Convert from <see cref="SkuDto"/> to <see cref="Sku"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="SkuDto"/>.</param>
        /// <returns><see cref="Sku"/> entity.</returns>
        public Sku Convert(SkuDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new Sku
            {
                Id = sourceType.Id,
                Name = sourceType.Name,
                UnitPrice = sourceType.UnitPrice,
                Inventory = sourceType.Inventory,
            };
        }
    }
}

