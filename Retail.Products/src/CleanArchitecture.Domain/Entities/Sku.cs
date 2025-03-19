// <copyright file="Sku.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.src.CleanArchitecture.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines Order entity.
    /// </summary>
    public class Sku
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(100)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        [Required]
        public double UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the inventory.
        /// </summary>
        [Required]
        public int Inventory { get; set; }
    }
}
