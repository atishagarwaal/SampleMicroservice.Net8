// <copyright file="ApplicationDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Orders.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Retail.Api.Orders.Model;
    using System.Data;

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
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 80,
                },
                new Order
                {
                    Id = 2,
                    CustomerId = 2,
                    OrderDate = DateTime.Now,
                    TotalAmount = 90,
                },
                new Order
                {
                    Id = 3,
                    CustomerId = 3,
                    OrderDate = DateTime.Now,
                    TotalAmount = 140,
                }
            );

            // Initialize data
            modelBuilder.Entity<LineItem>().HasData(
                new LineItem { Id = 1, OrderId = 1, SkuId = 1, Qty = 1 },
                new LineItem { Id = 2, OrderId = 1, SkuId = 2, Qty = 1 },
                new LineItem { Id = 3, OrderId = 2, SkuId = 2, Qty = 1 },
                new LineItem { Id = 4, OrderId = 2, SkuId = 3, Qty = 1 },
                new LineItem { Id = 5, OrderId = 3, SkuId = 1, Qty = 1 },
                new LineItem { Id = 6, OrderId = 3, SkuId = 2, Qty = 1 },
                new LineItem { Id = 7, OrderId = 3, SkuId = 3, Qty = 1 }
            );
        }

        /// <summary>
        /// Gets or sets customers.
        /// </summary>
        public DbSet<Order> Orders { get; set; }
    }
}