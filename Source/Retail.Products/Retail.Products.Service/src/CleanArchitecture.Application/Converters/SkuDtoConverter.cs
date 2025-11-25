// <copyright file="SkuDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Products.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Api.Products.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Api.Products.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="DomainEntities.Sku"/> entity to <see cref="Dto.SkuDto"/> message.
    /// </summary>
    public class SkuDtoConverter : IConverter<DomainEntities.Sku, Dto.SkuDto>
    {
        /// <summary>
        /// Convert from <see cref="DomainEntities.Sku"/> to <see cref="Dto.SkuDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="DomainEntities.Sku"/>.</param>
        /// <returns><see cref="Dto.SkuDto"/> message.</returns>
        public Dto.SkuDto Convert(DomainEntities.Sku sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new Dto.SkuDto
            {
                Id = sourceType.Id,
                Name = sourceType.Name,
                UnitPrice = sourceType.UnitPrice,
                Inventory = sourceType.Inventory,
            };
        }
    }
}

