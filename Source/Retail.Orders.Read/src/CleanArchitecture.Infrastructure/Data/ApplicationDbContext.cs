// <copyright file="ApplicationDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data
{
    using MongoDB.Driver;
    using Microsoft.Extensions.Options;
    using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
    using CommonLibrary.Configuration;

    /// <summary>
    /// Application database context for MongoDB.
    /// </summary>
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="dbConnectionOptions">Database connection configuration options.</param>
        /// <param name="mongoDbOptions">MongoDB settings configuration options.</param>
        public ApplicationDbContext(
            IOptions<DatabaseConnectionConfiguration> dbConnectionOptions,
            IOptions<MongoDBSettings> mongoDbOptions)
        {
            var dbConnectionConfig = dbConnectionOptions.Value;
            var mongoDbConfig = mongoDbOptions.Value;

            var connectionString = dbConnectionConfig.DefaultConnection;
            var databaseName = mongoDbConfig.DatabaseName ?? "OrdersDb";

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        /// <summary>
        /// Gets a MongoDB collection by name.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="name">The collection name.</param>
        /// <returns>The MongoDB collection.</returns>
        public virtual IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name);
    }
}