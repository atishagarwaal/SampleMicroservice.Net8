// <copyright file="OrderProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.Profiles
{
    using Retail.Api.Orders.Dto;
    using Retail.Api.Orders.Model;

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
            this.CreateMap<Order, OrderDto>();
            this.CreateMap<LineItem, LineItemDto>();
        }
    }
}
