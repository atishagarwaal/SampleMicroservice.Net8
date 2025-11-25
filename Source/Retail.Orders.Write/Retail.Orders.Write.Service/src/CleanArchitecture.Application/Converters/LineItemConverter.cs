// <copyright file="LineItemConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Converters
{
    using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="Dto.LineItemDto"/> message to <see cref="DomainEntities.LineItem"/> entity.
    /// </summary>
    public class LineItemConverter : IConverter<Dto.LineItemDto, DomainEntities.LineItem>
    {
        /// <summary>
        /// Convert from <see cref="Dto.LineItemDto"/> to <see cref="DomainEntities.LineItem"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="Dto.LineItemDto"/>.</param>
        /// <returns><see cref="DomainEntities.LineItem"/> entity.</returns>
        public DomainEntities.LineItem Convert(Dto.LineItemDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new DomainEntities.LineItem
            {
                Id = sourceType.Id,
                OrderId = sourceType.OrderId,
                SkuId = sourceType.SkuId,
                Qty = sourceType.Qty,
            };
        }
    }
}

