// <copyright file="SkuDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.BFFWeb.Api.Model
{
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
