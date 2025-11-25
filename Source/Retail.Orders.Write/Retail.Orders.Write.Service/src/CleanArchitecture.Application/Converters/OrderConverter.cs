// <copyright file="OrderConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Converters
{
    using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="Dto.OrderDto"/> message to <see cref="DomainEntities.Order"/> entity.
    /// </summary>
    public class OrderConverter : IConverter<Dto.OrderDto, DomainEntities.Order>
    {
        private readonly IConverter<Dto.LineItemDto, DomainEntities.LineItem> _lineItemConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderConverter"/> class.
        /// </summary>
        /// <param name="lineItemConverter">Instance of line item converter.</param>
        public OrderConverter(IConverter<Dto.LineItemDto, DomainEntities.LineItem> lineItemConverter)
        {
            _lineItemConverter = lineItemConverter ?? throw new ArgumentNullException(nameof(lineItemConverter));
        }

        /// <summary>
        /// Convert from <see cref="Dto.OrderDto"/> to <see cref="DomainEntities.Order"/>.
        /// </summary>
        /// <param name="sourceType">The contract type message for <see cref="Dto.OrderDto"/>.</param>
        /// <returns><see cref="DomainEntities.Order"/> entity.</returns>
        public DomainEntities.Order Convert(Dto.OrderDto sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            var order = new DomainEntities.Order
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

