﻿// <copyright file="LineItem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Orders.Read.src.CleanArchitecture.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Defines Order item entity.
    /// </summary>
    public class LineItem
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the Order Id.
        /// </summary>
        [ForeignKey("OrderId")]
        public long OrderId { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        public Order? Order { get; set; }

        /// <summary>
        /// Gets or sets the Sku Id.
        /// </summary>
        public long SkuId { get; set; }

        /// <summary>
        /// Gets or sets the Qty.
        /// </summary>
        public int Qty { get; set; }
    }
}