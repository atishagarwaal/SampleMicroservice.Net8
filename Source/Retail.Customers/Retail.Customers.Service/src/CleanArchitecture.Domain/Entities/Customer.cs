// <copyright file="Customer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines Customer entity.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(100)]
        public string? LastName { get; set; }
    }
}
