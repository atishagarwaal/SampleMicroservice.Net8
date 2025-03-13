// <copyright file="ApplicationDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Data
{
    using Microsoft.EntityFrameworkCore;
    using Retail.Api.Orders.Read.src.CleanArchitecture.Domain.Entities;

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

        /// <summary>
        /// Gets or sets Orders.
        /// </summary>
        public DbSet<Order>? Orders { get; set; }

        /// <summary>
        /// Gets or sets LineItems.
        /// </summary>
        public DbSet<LineItem>? LineItems { get; set; }

        /// <summary>
        /// Overrides the OnModelCreating method to configure the database context and model.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

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
                });

            // Initialize data
            modelBuilder.Entity<LineItem>().HasData(
                new LineItem { Id = 1, OrderId = 1, SkuId = 1, Qty = 1 },
                new LineItem { Id = 2, OrderId = 1, SkuId = 2, Qty = 1 },
                new LineItem { Id = 3, OrderId = 2, SkuId = 2, Qty = 1 },
                new LineItem { Id = 4, OrderId = 2, SkuId = 3, Qty = 1 },
                new LineItem { Id = 5, OrderId = 3, SkuId = 1, Qty = 1 },
                new LineItem { Id = 6, OrderId = 3, SkuId = 2, Qty = 1 },
                new LineItem { Id = 7, OrderId = 3, SkuId = 3, Qty = 1 });
        }
    }
}