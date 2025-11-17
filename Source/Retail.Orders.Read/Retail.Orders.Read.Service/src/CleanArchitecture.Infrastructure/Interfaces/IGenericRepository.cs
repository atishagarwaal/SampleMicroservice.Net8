// <copyright file="IRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces
{
    using System.Linq.Expressions;

    public interface IGenericRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(long id);
        Task RemoveAsync(long id);
        Task UpdateAsync(long id, T entity);
        Task<IEnumerable<T>> ExecuteQueryAsync(Expression<Func<T, bool>> predicate);
    }
}

