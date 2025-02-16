// <copyright file="OrderProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Orders.src.CleanArchitecture.Application.Mappings
{
    using Retail.Api.Orders.MessageContract;
    using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Orders.src.CleanArchitecture.Application.MessageContracts;
    using Retail.Api.Orders.src.CleanArchitecture.Domain.Entities;
    using OrderCreatedEvent = MessageContracts.OrderCreatedEvent;

    /// <summary>
    /// Defines Order profile for AutoMapper.
    /// </summary>
    public class OrderProfile : AutoMapper.Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderProfile"/> class.
        /// </summary>
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>().ReverseMap(); ;
            CreateMap<LineItem, LineItemDto>().ReverseMap();
            CreateMap<OrderCreatedEvent, OrderDto>().ReverseMap();
        }
    }
}
