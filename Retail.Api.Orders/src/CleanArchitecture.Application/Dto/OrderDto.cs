// <copyright file="OrderDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Orders.src.CleanArchitecture.Application.Dto
{
    /// <summary>
    /// Defines Order entity.
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the customer Id.
        /// </summary>
        public long CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the Line Items.
        /// </summary>
        public List<LineItemDto>? LineItems { get; set; }
    }
}
