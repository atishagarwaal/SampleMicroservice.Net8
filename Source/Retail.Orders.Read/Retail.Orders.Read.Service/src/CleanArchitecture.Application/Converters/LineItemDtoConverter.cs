// <copyright file="LineItemDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Converters
{
    using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
    using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="LineItem"/> entity to <see cref="LineItemDto"/> message.
    /// </summary>
    public class LineItemDtoConverter : IConverter<LineItem, LineItemDto>
    {
        /// <summary>
        /// Convert from <see cref="LineItem"/> to <see cref="LineItemDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="LineItem"/>.</param>
        /// <returns><see cref="LineItemDto"/> message.</returns>
        public LineItemDto Convert(LineItem sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new LineItemDto
            {
                Id = sourceType.Id,
                OrderId = sourceType.OrderId,
                SkuId = sourceType.SkuId,
                Qty = sourceType.Qty,
            };
        }
    }
}

