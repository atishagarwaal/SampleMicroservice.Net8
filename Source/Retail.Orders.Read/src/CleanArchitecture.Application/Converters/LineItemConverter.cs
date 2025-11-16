// <copyright file="LineItemConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Converters
{
    using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
    using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="LineItemDto"/> message to <see cref="LineItem"/> entity.
    /// </summary>
    public class LineItemConverter : IConverter<LineItemDto, LineItem>
    {
        /// <summary>
        /// Convert from <see cref="LineItemDto"/> to <see cref="LineItem"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="LineItemDto"/>.</param>
        /// <returns><see cref="LineItem"/> entity.</returns>
        public LineItem Convert(LineItemDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return new LineItem
            {
                Id = sourceType.Id,
                OrderId = sourceType.OrderId,
                SkuId = sourceType.SkuId,
                Qty = sourceType.Qty,
            };
        }
    }
}

