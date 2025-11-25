// <copyright file="LineItemDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Converters
{
    using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="DomainEntities.LineItem"/> entity to <see cref="Dto.LineItemDto"/> message.
    /// </summary>
    public class LineItemDtoConverter : IConverter<DomainEntities.LineItem, Dto.LineItemDto>
    {
        /// <summary>
        /// Convert from <see cref="DomainEntities.LineItem"/> to <see cref="Dto.LineItemDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="DomainEntities.LineItem"/>.</param>
        /// <returns><see cref="Dto.LineItemDto"/> message.</returns>
        public Dto.LineItemDto Convert(DomainEntities.LineItem sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new Dto.LineItemDto
            {
                Id = sourceType.Id,
                OrderId = sourceType.OrderId,
                SkuId = sourceType.SkuId,
                Qty = sourceType.Qty,
            };
        }
    }
}

