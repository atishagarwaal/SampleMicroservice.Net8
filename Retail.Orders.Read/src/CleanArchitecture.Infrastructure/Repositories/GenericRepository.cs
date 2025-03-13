namespace Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
    using Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
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
        /// Gets collection of object
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        /// <summary>
        /// Gets object by Id
        /// </summary>
        /// <param name="id">Generic type parameter.</param>
        /// <returns>Returns object of type parameter T.</returns>
        public async Task<T?> GetByIdAsync(long id)
            => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> ExecuteQueryAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
    }
}