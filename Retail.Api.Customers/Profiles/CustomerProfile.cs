// <copyright file="CustomerProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.Profiles
{
    using Retail.Api.Customers.Dto;
    using Retail.Api.Customers.Model;

    /// <summary>
    /// Defines Customer profile for AutoMapper.
    /// </summary>
    public class CustomerProfile : AutoMapper.Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerProfile"/> class.
        /// </summary>
        public CustomerProfile()
        {
            this.CreateMap<Customer, CustomerDto>().ReverseMap();
        }
    }
}
