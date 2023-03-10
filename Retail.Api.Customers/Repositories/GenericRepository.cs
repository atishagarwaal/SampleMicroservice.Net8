using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Retail.Api.Customers.Data;
using Retail.Api.Customers.Interface;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Retail.Api.Customers.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        /// <summary>
        /// Add a new object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public void Add(T entity)
            => _dbContext.Set<T>().Add(entity);

        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public async Task AddAsync(T entity)
            => await _dbContext.Set<T>().AddAsync(entity);

        /// <summary>
        /// Add a range of objects.
        /// </summary>
        /// <param name="entities">Collection of object of type parameter T.</param>
        public void AddRange(IEnumerable<T> entities)
            =>  _dbContext.Set<T>().AddRange(entities);

        /// <summary>
        /// Add a range of objects asynchronously.
        /// </summary>
        /// <param name="entities">Collection of object of type parameter T.</param>
        public async Task AddRangeAsync(IEnumerable<T> entities)
            => await _dbContext.Set<T>().AddRangeAsync(entities);

        /// <summary>
        /// Query collection of object.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
            => _dbContext.Set<T>().Where(expression);

        /// <summary>
        /// Query collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
            => await _dbContext.Set<T>().Where(expression).ToListAsync();

        /// <summary>
        /// Gets collection of object
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        public IEnumerable<T> GetAll()
            => _dbContext.Set<T>().ToList();

        /// <summary>
        /// Gets collection of object
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbContext.Set<T>().ToListAsync();

        /// <summary>
        /// Gets object by Id
        /// </summary>
        /// <param name="id">Generic type parameter.</param>
        /// <returns>Returns object of type parameter T.</returns>
        public T GetById(long id)
            => _dbContext.Set<T>().Find(id);

        /// <summary>
        /// Gets object by Id
        /// </summary>
        /// <param name="id">Generic type parameter.</param>
        /// <returns>Returns object of type parameter T.</returns>
        public async Task<T> GetByIdAsync(long id)
            => await _dbContext.Set<T>().FindAsync(id);

        /// <summary>
        /// Remove an object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public void Remove(T entity)
            => _dbContext.Set<T>().Remove(entity);

        /// <summary>
        /// Remove a range of objects.
        /// </summary>
        /// <param name="entities"></param>
        public void RemoveRange(IEnumerable<T> entities)
            => _dbContext.Set<T>().RemoveRange(entities);

        /// <summary>
        /// Update an object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public void Update(T entity)
           => _dbContext.Update(entity);

        /// <summary>
        /// Update a range of objects.
        /// </summary>
        /// <param name="entities"></param>
        public void UpdateRange(IEnumerable<T> entities)
            => _dbContext.UpdateRange(entities);
    }
}
