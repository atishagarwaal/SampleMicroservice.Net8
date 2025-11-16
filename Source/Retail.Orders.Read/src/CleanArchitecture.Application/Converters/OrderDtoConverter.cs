// <copyright file="OrderDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Converters
{
    using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
    using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="Order"/> entity to <see cref="OrderDto"/> message.
    /// </summary>
    public class OrderDtoConverter : IConverter<Order, OrderDto>
    {
        private readonly IConverter<LineItem, LineItemDto> _lineItemDtoConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDtoConverter"/> class.
        /// </summary>
        /// <param name="lineItemDtoConverter">Instance of line item DTO converter.</param>
        public OrderDtoConverter(IConverter<LineItem, LineItemDto> lineItemDtoConverter)
        {
            _lineItemDtoConverter = lineItemDtoConverter ?? throw new ArgumentNullException(nameof(lineItemDtoConverter));
        }

        /// <summary>
        /// Convert from <see cref="Order"/> to <see cref="OrderDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="Order"/>.</param>
        /// <returns><see cref="OrderDto"/> message.</returns>
        public OrderDto Convert(Order sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            var orderDto = new OrderDto
            {
                Id = sourceType.Id,
                CustomerId = sourceType.CustomerId,
                OrderDate = sourceType.OrderDate,
                TotalAmount = sourceType.TotalAmount,
            };

            if (sourceType.LineItems != null && sourceType.LineItems.Any())
            {
                orderDto.LineItems = sourceType.LineItems
                    .Select(lineItem => _lineItemDtoConverter.Convert(lineItem))
                    .ToList();
            }

            return orderDto;
        }
    }
}

