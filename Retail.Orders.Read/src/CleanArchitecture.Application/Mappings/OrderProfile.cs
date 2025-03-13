// <copyright file="OrderProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Orders.Read.src.CleanArchitecture.Application.Mappings
{
    using CommonLibrary.MessageContract;
    using Retail.Api.Orders.Read.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Orders.Read.src.CleanArchitecture.Domain.Entities;

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
