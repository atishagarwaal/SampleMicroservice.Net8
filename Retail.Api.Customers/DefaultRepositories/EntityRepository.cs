using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Retail.Api.Customers.Data;
using Retail.Api.Customers.DefaultInterface;
using Retail.Api.Customers.Model;
using System.Linq.Expressions;

namespace Retail.Api.Customers.DefaultRepositories
{
    /// <summary>
    /// Generic repository class.
    /// </summary>
    public class EntityRepository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the GenericRepository class.
        /// </summary>
        /// <param name="dbcontext">Db context.</param>
        public EntityRepository(ApplicationDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public async Task<T> AddAsync(T entity)
        {
            var entry = await _dbContext.Set<T>().AddAsync(entity);
            return entry.Entity;
        }

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
        public async Task<T?> GetByIdAsync(long id)
            => await _dbContext.Set<T>().FindAsync(id);

        /// <summary>
        /// Remove an object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public void Remove(T entity)
            => _dbContext.Set<T>().Remove(entity);

        /// <summary>
        /// Update an object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public void Update(T entity)
            => _dbContext.Update(entity);
        
    }
}
