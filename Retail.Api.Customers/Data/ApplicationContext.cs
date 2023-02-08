// <copyright file="ApplicationDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
    using Microsoft.Extensions.Hosting;
    using Retail.Api.Customers.Model;
    using System.Data;
    using System.Xml;

    /// <summary>
    /// Application db context class.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">db context options.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Initialize data
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    Id = 1,
                    FirstName = "Mahesh",
                    LastName = "Kumar",
                },
                new Customer
                {
                    Id = 2,
                    FirstName = "Ramesh",
                    LastName = "Kumar",
                },
                new Customer
                {
                    Id = 3,
                    FirstName = "Himesh",
                    LastName = "Kumar",
                }
            );
        }

        /// <summary>
        /// Gets or sets customers.
        /// </summary>
        public DbSet<Customer> Customers { get; set; }
    }
}