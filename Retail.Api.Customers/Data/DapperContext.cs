using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Retail.Api.Customers.Data
{
    /// <summary>
    /// Dapper context class.
    /// </summary>
    public class DapperContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DapperContext"/> class.
        /// </summary>
        /// <param name="configuration">Configuration object.</param>
        public DapperContext(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Sets SQL Connection.
        /// </summary>
        /// <returns>Returns Sql connection.</returns>
        public IDbConnection CreateConnection() => new SqlConnection(this.Configuration.GetConnectionString("DefaultConnection"));
    }
}
