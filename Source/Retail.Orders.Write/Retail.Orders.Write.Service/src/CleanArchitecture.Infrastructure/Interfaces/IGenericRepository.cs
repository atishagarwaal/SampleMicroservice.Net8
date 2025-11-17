// <copyright file="IRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;

    /// <summary>
    /// Interface definition for data access layer
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Add a new object asynchronously and return it with generated ID.
        /// This method saves changes immediately to ensure the ID is generated.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        Task<T> AddAndGetIdAsync(T entity);

        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Gets object by Id
        /// </summary>
        /// <param name="id">Generic type parameter.</param>
        /// <returns>Returns object of type parameter T.</returns>
        Task<T?> GetByIdAsync(long id);

        /// <summary>
        /// Remove an object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        void Remove(T entity);

        /// <summary>
        /// Update an object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        T Update(T entity);

        Task<IEnumerable<T>> ExecuteQueryAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get the generated ID for an entity after SaveChanges.
        /// This method should be called after SaveChanges to get the generated ID.
        /// </summary>
        /// <param name="entity">The entity to get the ID for.</param>
        /// <returns>The generated ID as a long.</returns>
        long GetGeneratedId(T entity);
    }
}

