// <copyright file="Order.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Orders.src.CleanArchitecture.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines Order entity.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the customer Id.
        /// </summary>
        [Required]
        public long CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        [Required]
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        public double TotalAmount { get; set; }

        public ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();
    }
}
