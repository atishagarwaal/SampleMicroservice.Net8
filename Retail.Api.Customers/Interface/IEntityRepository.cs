// <copyright file="IRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.Interface
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;

    /// <summary>
    /// Interface definition for data access layer
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public interface IEntityRepository<T> where T : class
    {
        /// <summary>
        /// Add a new object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        T Add(T entity);

        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Add a range of objects.
        /// </summary>
        /// <param name="entities">Collection of object of type parameter T.</param>
        void AddRange(IEnumerable<T> entities);

        /// <summary>
        /// Add a range of objects asynchronously.
        /// </summary>
        /// <param name="entities">Collection of object of type parameter T.</param>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Query collection of object.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Query collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Gets collection of object
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        IEnumerable<T> GetAll();

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
        T? GetById(long id);

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
        /// Remove a range of objects.
        /// </summary>
        /// <param name="entities"></param>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// Update an object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        T Update(T entity);

        /// <summary>
        /// Update a range of objects.
        /// </summary>
        /// <param name="entities"></param>
        void UpdateRange(IEnumerable<T> entities);
    }
}
