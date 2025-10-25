namespace Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;
    using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
    using System.Linq.Expressions;

    /// <summary>
    /// Generic repository class.
    /// </summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the GenericRepository class.
        /// </summary>
        /// <param name="dbcontext">Db context.</param>
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public virtual async Task<T> AddAsync(T entity)
        {
            var entry = await _dbSet.AddAsync(entity);
            return entry.Entity;
        }

        /// <summary>
        /// Add a new object asynchronously and return it with generated ID.
        /// This method saves changes immediately to ensure the ID is generated.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public virtual async Task<T> AddAndGetIdAsync(T entity)
        {
            var entry = await _dbSet.AddAsync(entity);
            
            // Get the entity entry to access the generated ID
            var entityEntry = _context.Entry(entity);
            
            return entry.Entity;
        }

        /// <summary>
        /// Get the generated ID for an entity after SaveChanges.
        /// This method should be called after SaveChanges to get the generated ID.
        /// </summary>
        /// <param name="entity">The entity to get the ID for.</param>
        /// <returns>The generated ID as a long.</returns>
        public long GetGeneratedId(T entity)
        {
            var entry = _context.Entry(entity);
            var idProperty = entry.Property("Id");
            if (idProperty != null)
            {
                return (long)idProperty.CurrentValue;
            }
            return 0;
        }

        /// <summary>
        /// Gets collection of object
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        /// <summary>
        /// Gets object by Id
        /// </summary>
        /// <param name="id">Generic type parameter.</param>
        /// <returns>Returns object of type parameter T.</returns>
        public virtual async Task<T?> GetByIdAsync(long id)
            => await _dbSet.FindAsync(id);

        /// <summary>
        /// Remove an object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public virtual void Remove(T entity)
            => _dbSet.Remove(entity);

        /// <summary>
        /// Update an object.
        /// </summary>
        /// <param name="entity">An object type parameter T.</param>
        public virtual T Update(T entity)
        {
            var entry = _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return entry.Entity;
        }

        public virtual async Task<IEnumerable<T>> ExecuteQueryAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
    }
}