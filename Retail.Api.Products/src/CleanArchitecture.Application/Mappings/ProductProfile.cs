// <copyright file="ProductProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.src.CleanArchitecture.Application.Profiles
{
    using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;

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
            CreateMap<Sku, SkuDto>().ReverseMap();
        }
    }
}
