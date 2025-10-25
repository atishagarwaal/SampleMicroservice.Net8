namespace Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Repositories
{
    using MongoDB.Driver;
    using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
    using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
    using System.Linq.Expressions;

    /// <summary>
    /// Generic repository class.
    /// </summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly IMongoCollection<T> _collection;

        public GenericRepository(ApplicationDbContext context, string collectionName)
        {
            _collection = context.GetCollection<T>(collectionName);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(long id)
        {
            return await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }

        public async Task RemoveAsync(long id)
        {
            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
        }

        public async Task UpdateAsync(long id, T entity)
        {
            await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).ToListAsync();
        }
    }
}