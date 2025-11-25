// <copyright file="OrderDtoConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Converters
{
    using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
    using Dto = Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
    using DomainEntities = Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Converts <see cref="DomainEntities.Order"/> entity to <see cref="Dto.OrderDto"/> message.
    /// </summary>
    public class OrderDtoConverter : IConverter<DomainEntities.Order, Dto.OrderDto>
    {
        private readonly IConverter<DomainEntities.LineItem, Dto.LineItemDto> _lineItemDtoConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDtoConverter"/> class.
        /// </summary>
        /// <param name="lineItemDtoConverter">Instance of line item DTO converter.</param>
        public OrderDtoConverter(IConverter<DomainEntities.LineItem, Dto.LineItemDto> lineItemDtoConverter)
        {
            _lineItemDtoConverter = lineItemDtoConverter ?? throw new ArgumentNullException(nameof(lineItemDtoConverter));
        }

        /// <summary>
        /// Convert from <see cref="DomainEntities.Order"/> to <see cref="Dto.OrderDto"/>.
        /// </summary>
        /// <param name="sourceType">The entity type for <see cref="DomainEntities.Order"/>.</param>
        /// <returns><see cref="Dto.OrderDto"/> message.</returns>
        public Dto.OrderDto Convert(DomainEntities.Order sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            var orderDto = new Dto.OrderDto
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

