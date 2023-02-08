// <copyright file="SeedData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.Data
{
    using Microsoft.EntityFrameworkCore;
    using Retail.Api.Products.Model;

    /// <summary>
    /// Seed data class.
    /// </summary>
    public class SeedData
    {
        /// <summary>
        /// Gets or sets ApplicationDb context.
        /// </summary>
        private ApplicationDbContext context;

        /// <summary>
        /// Initiaizes a new instance of the <see cref="SeedData"/> class.
        /// </summary>
        /// <param name="context">ApplicationDb context.</param>
        public SeedData(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Method to create seed data.
        /// </summary>
        public void Seed()
        {
            // Ensure database is created
            this.context.Database.EnsureCreated();

            // Check if data is already created
            if (context.Skus.Any())
            {
                return;
            }

            // Initialize data
            this.context.Skus.AddRange(
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
                }
            );

            // Save data
            this.context.Database.ExecuteSqlRaw(@"SET IDENTITY_INSERT dbo.Settings ON");
            this.context.SaveChanges();
            this.context.Database.ExecuteSqlRaw(@"SET IDENTITY_INSERT dbo.Settings OFF");
        }
    }
}
