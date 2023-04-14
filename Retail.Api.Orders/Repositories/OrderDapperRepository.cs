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
    public class OrderDapperRepository : DapperRepository, IOrderDapperRepository
    {
        private readonly DapperContext _dapperContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDapperRepository"/> class.
        /// </summary>
        /// <param name="dapperContext">Db context.</param>
        public OrderDapperRepository(DapperContext dapperContext) : base(dapperContext)
        {
            _dapperContext = dapperContext;
        }

        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public async Task<int> AddAsync(Order entity)
        {
            var sql = "INSERT INTO [dbo].[Orders] ([CustomerId],[OrderDate],[TotalAmount]) VALUES (@CustomerId, @OrderDate,@TotalAmount)";
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
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            var sql = "SELECT [Id], [CustomerId], [OrderDate], [TotalAmount] FROM [dbo].[Orders]";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.QueryAsync<Order>(sql);
                return result.ToList();
            }
        }

        /// <summary>
        /// Get an object by Id asynchronously
        /// </summary>
        /// <param name="id">Primary key of the object.</param>
        /// <returns>Returns an object.</returns>
        public async Task<Order> GetByIdAsync(long id)
        {
            var sql = "SELECT [Id], [CustomerId], [OrderDate], [TotalAmount] FROM [dbo].[Orders] WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.QuerySingleOrDefaultAsync<Order>(sql, new { Id = id });
                return result;
            }
        }

        /// <summary>
        /// Removes an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public async Task<int> RemoveAsync(Order entity)
        {
            var sql = "DELETE FROM [dbo].[Orders] WHERE Id = @Id";
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
        public async Task<int> UpdateAsync(Order entity)
        {
            var sql = "UPDATE [dbo].[Orders] SET [CustomerId] = @CustomerId, [OrderDate] = @OrderDate, [TotalAmount] = @TotalAmount  WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.ExecuteAsync(sql, entity);
                return result;
            }
        }
    }
}
