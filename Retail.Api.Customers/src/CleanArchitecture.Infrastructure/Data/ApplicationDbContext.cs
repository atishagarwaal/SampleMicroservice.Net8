// <copyright file="ApplicationDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data
{
    using System.Data;
    using System.Xml;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
    using Microsoft.Extensions.Hosting;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

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
        public DbSet<Customer>? Customers { get; set; }

        /// <summary>
        /// Gets or sets customer notifications.
        /// </summary>
        public DbSet<Notification>? Notifications { get; set; }

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
                });
        }
    }
}