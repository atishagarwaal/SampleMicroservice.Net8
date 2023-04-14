using Dapper;
using Retail.Api.Customers.Data;
using Retail.Api.Customers.DefaultInterface;
using Retail.Api.Customers.DefaultRepositories;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Model;

namespace Retail.Api.Customers.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class CustomerDapperRepository : IRepository<Customer>
    {
        private readonly DapperContext _dapperContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerDapperRepository"/> class.
        /// </summary>
        /// <param name="dapperContext">Db context.</param>
        public CustomerDapperRepository(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public async Task<Customer> AddAsync(Customer entity)
        {
            var sql = "INSERT INTO [dbo].[Customers] ([FirstName], [LastName]) VALUES (@FirstName, @LastName)";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.ExecuteAsync(sql, entity);

                sql = "SELECT [Id], [FirstName], [LastName] FROM [dbo].[Customers] WHERE [FirstName] = @FirstName and [LastName]  = @LastName Order By Id desc";
                var obj = await connection.QuerySingleOrDefaultAsync<Customer>(sql, new { FirstName = entity?.FirstName, LastName = entity?.LastName });
                return obj;
            }
        }

        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object.</returns>
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            var sql = "SELECT [Id], [FirstName], [LastName] FROM [dbo].[Customers]";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.QueryAsync<Customer>(sql);
                return result.ToList();
            }
        }

        /// <summary>
        /// Get an object by Id asynchronously
        /// </summary>
        /// <param name="id">Primary key of the object.</param>
        /// <returns>Returns an object.</returns>
        public async Task<Customer?> GetByIdAsync(long id)
        {
            var sql = "SELECT [Id], [FirstName], [LastName] FROM [dbo].[Customers] WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.QuerySingleOrDefaultAsync<Customer>(sql, new { Id = id });
                return result;
            }
        }

        /// <summary>
        /// Removes an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public void Remove(Customer entity)
        {
            var sql = "DELETE FROM [dbo].[Customers] WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = connection.Execute(sql, new { entity?.Id });
            }
        }

        /// <summary>
        /// Updates an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public void Update(Customer entity)
        {
            var sql = "UPDATE [dbo].[Customers] SET [FirstName] = @FirstName, [LastName] = @LastName  WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = connection.Execute(sql, entity);

                sql = "SELECT [Id], [FirstName], [LastName] FROM [dbo].[Customers] WHERE Id = @Id";
                var record = connection.QuerySingleOrDefault<Customer>(sql, new { Id = entity?.Id });
                ////return record;
            }
        }
    }
}
