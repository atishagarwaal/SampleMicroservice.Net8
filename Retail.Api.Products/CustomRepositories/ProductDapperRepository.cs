using Dapper;
using Retail.Api.Products.Data;
using Retail.Api.Products.DefaultInterface;
using Retail.Api.Products.DefaultRepositories;
using Retail.Api.Products.Interface;
using Retail.Api.Products.Model;

namespace Retail.Api.Products.Repositories
{
    /// <summary>
    /// Product repository class.
    /// </summary>
    public class ProductDapperRepository : IRepository<Sku>
    {
        private readonly DapperContext _dapperContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDapperRepository"/> class.
        /// </summary>
        /// <param name="dapperContext">Db context.</param>
        public ProductDapperRepository(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public async Task<Sku> AddAsync(Sku entity)
        {
            var sql = "INSERT INTO [dbo].[Skus] ([Name], [UnitPrice]) VALUES (@Name, @UnitPrice)";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.ExecuteAsync(sql, entity);

                sql = "SELECT [Id], [Name], [UnitPrice] FROM [dbo].[Skus] WHERE [Name] = @Name Order By Id desc";
                var obj = await connection.QuerySingleOrDefaultAsync<Sku>(sql, new { Name = entity?.Name });
                return obj;
            }
        }

        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object.</returns>
        public async Task<IEnumerable<Sku>> GetAllAsync()
        {
            var sql = "SELECT [Id], [Name], [UnitPrice] FROM [dbo].[Skus]";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.QueryAsync<Sku>(sql);
                return result.ToList();
            }
        }

        /// <summary>
        /// Get an object by Id asynchronously
        /// </summary>
        /// <param name="id">Primary key of the object.</param>
        /// <returns>Returns an object.</returns>
        public async Task<Sku?> GetByIdAsync(long id)
        {
            var sql = "SELECT [Id], [Name], [UnitPrice] FROM [dbo].[Skus] WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.QuerySingleOrDefaultAsync<Sku>(sql, new { Id = id });
                return result;
            }
        }

        /// <summary>
        /// Removes an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public void Remove(Sku entity)
        {
            var sql = "DELETE FROM [dbo].[Skus] WHERE Id = @Id";
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
        public Sku Update(Sku entity)
        {
            var sql = "UPDATE [dbo].[Skus] SET [Name] = @Name, [UnitPrice] = @UnitPrice  WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = connection.Execute(sql, entity);

                sql = "SELECT [Name], [UnitPrice] FROM [dbo].[Skus] WHERE Id = @Id";
                var record = connection.QuerySingleOrDefault<Sku>(sql, new { Id = entity?.Id });
                return record;
            }
        }
    }
}
