// <copyright file="ApplicationDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data
{
    using MongoDB.Driver;
    using Microsoft.Extensions.Configuration;
    using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Constants;

    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _database;

        public ApplicationDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString(MessageConstants.DefaultConnection));
            _database = client.GetDatabase("OrdersDb");
        }

        public IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name);
    }
}