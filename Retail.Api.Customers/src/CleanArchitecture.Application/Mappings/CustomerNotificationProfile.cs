// <copyright file="CustomerProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Mappings
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

    /// <summary>
    /// Defines Customer profile for AutoMapper.
    /// </summary>
    public class CustomerNotificationProfile : AutoMapper.Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerNotificationProfile"/> class.
        /// </summary>
        public CustomerNotificationProfile()
        {
            CreateMap<CustomerNotification, CustomerNotificationDto>().ReverseMap();
        }
    }
}
