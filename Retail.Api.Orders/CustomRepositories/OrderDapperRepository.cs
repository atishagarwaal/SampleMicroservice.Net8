using Dapper;
using Retail.Api.Orders.DefaultInterface;
using Retail.Api.Orders.Data;
using Retail.Api.Orders.Model;
using Retail.Api.Orders.CustomInterface;
using Retail.Api.Orders.Dto;
using System.Data;

namespace Retail.Api.Orders.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class OrderDapperRepository : IOrderRepository
    {
        private readonly DapperContext _dapperContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDapperRepository"/> class.
        /// </summary>
        /// <param name="dapperContext">Db context.</param>
        public OrderDapperRepository(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        public async Task<Order> AddAsync(Order entity)
        {
            var sql = "INSERT INTO [dbo].[Orders] ([CustomerId],[OrderDate],[TotalAmount]) VALUES (@CustomerId, @OrderDate,@TotalAmount)";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = await connection.ExecuteAsync(sql, entity);

                sql = "SELECT [Id], [CustomerId], [OrderDate], [TotalAmount] FROM [dbo].[Orders] WHERE [CustomerId] = @CustomerId and [OrderDate] = @OrderDate and [TotalAmount] = @TotalAmount Order By Id desc";
                var obj = await connection.QuerySingleOrDefaultAsync<Order>(sql, new { CustomerId = entity?.CustomerId, OrderDate = entity?.OrderDate, TotalAmount = entity?.TotalAmount });
                return obj;
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
        public void Remove(Order entity)
        {
            var sql = "DELETE FROM [dbo].[Orders] WHERE Id = @Id";
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
        public Order Update(Order entity)
        {
            var sql = "UPDATE [dbo].[Orders] SET [CustomerId] = @CustomerId, [OrderDate] = @OrderDate, [TotalAmount] = @TotalAmount  WHERE Id = @Id";
            using (var connection = _dapperContext.CreateConnection())
            {
                connection.Open();
                var result = connection.Execute(sql, entity);

                sql = "SELECT [Id], [CustomerId], [OrderDate], [TotalAmount] FROM [dbo].[Orders] WHERE Id = @Id";
                var record = connection.QuerySingleOrDefault<Order>(sql, new { Id = entity?.Id });
                return record;
            }
        }

        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var final = new List<OrderDto>();

            // Create query
            var sql = @"SELECT 
	                        ORD.[Id] AS OrderId, 
	                        [CustomerId] AS CustomerId,
	                        [OrderDate] AS OrderDate, 
	                        [TotalAmount] AS TotalAmount,
	                        LIN.Id AS LineId,
	                        LIN.SkuId,
	                        LIN.Qty
                        FROM [dbo].[Orders] ORD
                        JOIN [dbo].[LineItems] LIN ON ORD.Id = LIN.OrderId
                        ORDER BY ORD.[Id], LIN.Id";

            // Execute query
            using (var connection = _dapperContext.CreateConnection())
            {
                // Get all records
                var orders = await connection.QueryAsync(sql);

                // Group by order Id
                var groups = orders
                        .GroupBy(o => o.OrderId);

                // Iterate each order
                foreach (var group in groups)
                {

                    // Initialize orderDto object
                    final.Add(new OrderDto
                    {
                        Id = group.Key,
                        CustomerId = group.FirstOrDefault().CustomerId,
                        OrderDate = group.FirstOrDefault().OrderDate,
                        TotalAmount = group.FirstOrDefault().TotalAmount,
                        LineItems = orders.Where(i => i.OrderId == group.Key).Select(i => new LineItemDto
                        {
                            Id = i.LineId,
                            OrderId = i.OrderId,
                            Qty = i.Qty,
                            SkuId = i.SkuId,
                        }).ToList(),
                    });
                }

                return final;
            }
        }

        /// <summary>
        /// Gets object by Id asynchronously.
        /// </summary>
        /// <param name="id">Id of object.</param>
        /// <returns>Returns object.</returns>
        public async Task<OrderDto?> GetOrderByIdAsync(long id)
        {
            var final = new OrderDto();

            // Create query
            var sql = @"SELECT 
	                        ORD.[Id] AS OrderId, 
	                        [CustomerId] AS CustomerId,
	                        [OrderDate] AS OrderDate, 
	                        [TotalAmount] AS TotalAmount,
	                        LIN.Id AS LineId,
	                        LIN.SkuId,
	                        LIN.Qty
                        FROM [dbo].[Orders] ORD
                        JOIN [dbo].[LineItems] LIN ON ORD.Id = LIN.OrderId
                        WHERE OrderId = @OrderId
                        ORDER BY ORD.[Id], LIN.Id";

            // Execute query
            using (var connection = _dapperContext.CreateConnection())
            {
                // Get all records
                var items = await connection.QueryAsync(sql, new { OrderId = id });

                // Initialize orderDto object
                final = new OrderDto
                {
                    Id = items.FirstOrDefault().OrderId,
                    CustomerId = items.FirstOrDefault().CustomerId,
                    OrderDate = items.FirstOrDefault().OrderDate,
                    TotalAmount = items.FirstOrDefault().TotalAmount,
                    LineItems = items.Select(i => new LineItemDto
                    {
                        Id = i.LineId,
                        OrderId = i.OrderId,
                        Qty = i.Qty,
                        SkuId = i.SkuId,
                    }).ToList(),
                };

                return final;
            }
        }
    }
}
