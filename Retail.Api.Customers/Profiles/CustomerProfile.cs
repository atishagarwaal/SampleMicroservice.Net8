// <copyright file="CustomerProfiles.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.Profiles
{
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
            CreateMap<Customer, CustomerDto>();
        }
    }
}
