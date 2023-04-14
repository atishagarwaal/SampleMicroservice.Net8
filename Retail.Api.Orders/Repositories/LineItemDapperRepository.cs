using Dapper;
using Retail.Api.Orders.Data;
using Retail.Api.Orders.DefaultRepositories;
using Retail.Api.Orders.Interface;
using Retail.Api.Orders.Model;

namespace Retail.Api.Orders.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class LineItemDapperRepository : DapperRepository, ILineItemDapperRepository
    {
        private readonly DapperContext _dapperContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineItemDapperRepository"/> class.
        /// </summary>
        /// <param name="dapperContext">Db context.</param>
        public LineItemDapperRepository(DapperContext dapperContext) : base(dapperContext)
        {
            _dapperContext = dapperContext;
        }

        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public async Task<int> AddAsync(LineItem entity)
        {
            var sql = "INSERT INTO [dbo].[LineItems] ([OrderId],[SkuId],[Qty]) VALUES (@OrderId, @SkuId,@Qty)";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.ExecuteAsync(sql, entity);
                return result;
            }
        }

        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object.</returns>
        public async Task<IEnumerable<LineItem>> GetAllAsync()
        {
            var sql = "SELECT [Id],[OrderId],[SkuId],[Qty] FROM [dbo].[LineItems]";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.QueryAsync<LineItem>(sql);
                return result.ToList();
            }
        }

        /// <summary>
        /// Get an object by Id asynchronously
        /// </summary>
        /// <param name="id">Primary key of the object.</param>
        /// <returns>Returns an object.</returns>
        public async Task<LineItem> GetByIdAsync(long id)
        {
            var sql = "SELECT [Id],[OrderId],[SkuId],[Qty] FROM [dbo].[LineItems] WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.QuerySingleOrDefaultAsync<LineItem>(sql, new { Id = id });
                return result;
            }
        }

        /// <summary>
        /// Removes an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public async Task<int> RemoveAsync(LineItem entity)
        {
            var sql = "DELETE FROM [dbo].[LineItems] WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.ExecuteAsync(sql, new { entity?.Id });
                return result;
            }
        }

        /// <summary>
        /// Updates an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public async Task<int> UpdateAsync(LineItem entity)
        {
            var sql = "UPDATE [dbo].[LineItems] SET [OrderId] = @OrderId, [SkuId] = @SkuId, [Qty] = @Qty  WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.ExecuteAsync(sql, entity);
                return result;
            }
        }
    }
}
