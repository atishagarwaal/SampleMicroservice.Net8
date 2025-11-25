// <copyright file="SkuConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.src.CleanArchitecture.Application.Converters
{
    using Retail.Api.Products.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Api.Products.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Api.Products.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="Dto.SkuDto"/> message to <see cref="DomainEntities.Sku"/> entity.
    /// </summary>
    public class SkuConverter : IConverter<Dto.SkuDto, DomainEntities.Sku>
    {
        /// <summary>
        /// Convert from <see cref="Dto.SkuDto"/> to <see cref="DomainEntities.Sku"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="Dto.SkuDto"/>.</param>
        /// <returns><see cref="DomainEntities.Sku"/> entity.</returns>
        public DomainEntities.Sku Convert(Dto.SkuDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new DomainEntities.Sku
            {
                Id = sourceType.Id,
                Name = sourceType.Name,
                UnitPrice = sourceType.UnitPrice,
                Inventory = sourceType.Inventory,
            };
        }
    }
}

