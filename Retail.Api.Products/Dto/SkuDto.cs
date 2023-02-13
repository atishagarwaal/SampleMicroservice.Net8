// <copyright file="Customer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.Dto
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines Order entity.
    /// </summary>
    public class SkuDto
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        public double UnitPrice { get; set; }
    }
}
