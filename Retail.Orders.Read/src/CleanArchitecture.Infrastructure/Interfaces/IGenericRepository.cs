// <copyright file="IRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces
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

        Task<IEnumerable<T>> ExecuteQueryAsync(Expression<Func<T, bool>> predicate);
    }
}

