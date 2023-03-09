// <copyright file="IGenericRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.Interface
{
    using System.Linq.Expressions;

    /// <summary>
    /// Interface definition for data access layer
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Gets object by Id
        /// </summary>
        /// <param name="id">Generic type parameter.</param>
        /// <returns>Returns object of type parameter T.</returns>
        T GetById(long id);

        /// <summary>
        /// Gets collection of object
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Query collection of object.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Add a new object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        void Add(T entity);

        /// <summary>
        /// Add a range of objects.
        /// </summary>
        /// <param name="entities">Collection of object of type parameter T.</param>
        void AddRange(IEnumerable<T> entities);

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
    }
}
