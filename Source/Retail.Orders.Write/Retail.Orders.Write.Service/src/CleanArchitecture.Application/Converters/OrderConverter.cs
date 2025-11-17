// <copyright file="OrderConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Converters
{
    using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
    using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
    using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="OrderDto"/> message to <see cref="Order"/> entity.
    /// </summary>
    public class OrderConverter : IConverter<OrderDto, Order>
    {
        private readonly IConverter<LineItemDto, LineItem> _lineItemConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderConverter"/> class.
        /// </summary>
        /// <param name="lineItemConverter">Instance of line item converter.</param>
        public OrderConverter(IConverter<LineItemDto, LineItem> lineItemConverter)
        {
            _lineItemConverter = lineItemConverter ?? throw new ArgumentNullException(nameof(lineItemConverter));
        }

        /// <summary>
        /// Convert from <see cref="OrderDto"/> to <see cref="Order"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="OrderDto"/>.</param>
        /// <returns><see cref="Order"/> entity.</returns>
        public Order Convert(OrderDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            var order = new Order
            {
                Id = sourceType.Id,
                CustomerId = sourceType.CustomerId,
                OrderDate = sourceType.OrderDate,
                TotalAmount = sourceType.TotalAmount,
            };

            if (sourceType.LineItems != null)
            {
                foreach (var lineItemDto in sourceType.LineItems)
                {
                    var lineItem = _lineItemConverter.Convert(lineItemDto);
                    lineItem.Order = order; // Set navigation property
                    order.LineItems.Add(lineItem);
                }
            }

            return order;
        }
    }
}

