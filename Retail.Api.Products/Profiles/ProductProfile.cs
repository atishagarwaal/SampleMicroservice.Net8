// <copyright file="ProductProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.Profiles
{
    using Retail.Api.Products.Dto;
    using Retail.Api.Products.Model;

    /// <summary>
    /// Defines Product profile for AutoMapper.
    /// </summary>
    public class ProductProfile : AutoMapper.Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductProfile"/> class.
        /// </summary>
        public ProductProfile()
        {
            this.CreateMap<Sku, SkuDto>();
        }
    }
}
