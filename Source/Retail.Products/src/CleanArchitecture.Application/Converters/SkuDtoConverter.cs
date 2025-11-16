// <copyright file="SkuDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Products.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="Sku"/> entity to <see cref="SkuDto"/> message.
    /// </summary>
    public class SkuDtoConverter : IConverter<Sku, SkuDto>
    {
        /// <summary>
        /// Convert from <see cref="Sku"/> to <see cref="SkuDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="Sku"/>.</param>
        /// <returns><see cref="SkuDto"/> message.</returns>
        public SkuDto Convert(Sku sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new SkuDto
            {
                Id = sourceType.Id,
                Name = sourceType.Name,
                UnitPrice = sourceType.UnitPrice,
                Inventory = sourceType.Inventory,
            };
        }
    }
}

