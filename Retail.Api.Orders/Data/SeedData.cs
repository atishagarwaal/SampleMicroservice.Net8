// <copyright file="SeedData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Orders.Data
{
    using Microsoft.EntityFrameworkCore;
    using Retail.Api.Orders.Model;

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
            if (this.context.Orders.Any())
            {
                return;
            }

            // Initialize data
            this.context.Orders.AddRange(
                new Order
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 80,
                    LineItems = new List<LineItem>()
                        {
                             new LineItem { Id = 1, OrderId = 1, SkuId= 1, Qty = 1},
                             new LineItem { Id = 2, OrderId = 1, SkuId= 2, Qty = 1},
                        }
                },
                new Order
                {
                    Id = 2,
                    CustomerId = 2,
                    OrderDate = DateTime.Now,
                    TotalAmount = 90,
                    LineItems = new List<LineItem>()
                        {
                             new LineItem { Id = 3, OrderId = 2, SkuId= 2, Qty = 1},
                             new LineItem { Id = 4, OrderId = 2, SkuId= 3, Qty = 1},
                        }
                },
                new Order
                {
                    Id = 3,
                    CustomerId = 3,
                    OrderDate = DateTime.Now,
                    TotalAmount = 140,
                    LineItems = new List<LineItem>()
                        {
                             new LineItem { Id = 5, OrderId = 3, SkuId= 1, Qty = 1},
                             new LineItem { Id = 6, OrderId = 3, SkuId= 2, Qty = 1},
                             new LineItem { Id = 7, OrderId = 3, SkuId= 3, Qty = 1},
                        }
                }
            );

            // Save data
            this.context.Database.ExecuteSqlRaw(@"SET IDENTITY_INSERT dbo.Orders ON");
            this.context.SaveChanges();
            this.context.Database.ExecuteSqlRaw(@"SET IDENTITY_INSERT dbo.Orders OFF");
        }
    }
}
