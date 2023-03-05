// <copyright file="ApplicationDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.Data
{
    using Microsoft.EntityFrameworkCore;
    using Retail.Api.Products.Model;

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
        /// Gets or sets customers.
        /// </summary>
        public DbSet<Sku>? Skus { get; set; }

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
            modelBuilder.Entity<Sku>().HasData(
                new Sku
                {
                    Id = 1,
                    Name = "Rice",
                    UnitPrice = 50,
                },
                new Sku
                {
                    Id = 2,
                    Name = "Salt",
                    UnitPrice = 30,
                },
                new Sku
                {
                    Id = 3,
                    Name = "Sugar",
                    UnitPrice = 60,
                });
        }
    }
}