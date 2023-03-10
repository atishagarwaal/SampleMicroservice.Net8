
using Retail.Api.Customers.Data;
using Retail.Api.Customers.Interface;
using Dapper;

namespace Retail.Api.Customers.Repositories
{
    /// <summary>
    /// Generic repository class.
    /// </summary>
    public class DapperRepository : IDapperRepository
    {
        private readonly DapperContext _dapperContext;

        /// <summary>
        /// Initializes a new instance of the GenericRepository class.
        /// </summary>
        /// <param name="dapperContext">Db context.</param>
        public DapperRepository(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        /// <summary> 
        /// Runs raw sql query. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <returns>Returns data or status.</returns> 
        public async Task<dynamic> ExecuteScalarAsync(string sqlquery)
        {
            using (var connection = _dapperContext.CreateConnection())
            {
                var result = await connection.ExecuteScalarAsync<dynamic>(sqlquery); return result;
            }
        }

        /// <summary> 
        /// Runs raw sql query. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <param name="parameters">Sql parameters.</param> 
        /// <returns>Returns data or status.</returns> 
        public async Task<int> ExecuteAsync(string sqlquery, DynamicParameters? parameters = null) 
        { 
            using (var connection = _dapperContext.CreateConnection()) 
            { 
                int result;

                if (parameters == null)
                {
                    result = await connection.ExecuteAsync(sqlquery);
                }
                else
                {
                    result = await connection.ExecuteAsync(sqlquery, parameters);
                }

                return result; 
            } 
        }

        /// <summary> 
        /// Runs raw sql query. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <returns>Returns data or status.</returns> 
        public dynamic ExecuteScalar(string sqlquery) 
        { 
            using (var connection = _dapperContext.CreateConnection()) 
            { 
                var result = connection.ExecuteScalar<dynamic>(sqlquery); return result; 
            }
        }

        /// <summary> 
        /// Runs raw sql query. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <param name="parameters">Sql parameters.</param> 
        /// <returns>Returns data or status.</returns> 
        public int Execute(string sqlquery, DynamicParameters? parameters = null) 
        { 
            using (var connection = _dapperContext.CreateConnection()) 
            { 
                int result;

                if (parameters == null) 
                { 
                    result = connection.Execute(sqlquery); 
                } 
                else 
                { 
                    result = connection.Execute(sqlquery, parameters); 
                }

                return result; 
            }
        }

        /// <summary> 
        /// Runs raw query multiple. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <param name="parameters">Sql parameters.</param> 
        /// <returns>Returns data or status.</returns> 
        public async Task<IEnumerable<T>> QueryMultipleAsync<T>(string sqlquery, DynamicParameters? parameters = null) 
        { 
            using (var connection = _dapperContext.CreateConnection()) 
            { 
                IEnumerable<T> result;

                if (parameters == null) 
                { 
                    var query = await connection.QueryMultipleAsync(sqlquery); 
                    result = await query.ReadAsync<T>();
                } 
                else 
                { 
                    var query = await connection.QueryMultipleAsync(sqlquery, parameters); 
                    result = await query.ReadAsync<T>();
                }

                return result; 
            }
        }

        /// <summary> 
        /// Runs raw query single or default. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <param name="parameters">Sql parameters.</param> 
        /// <returns>Returns data or status.</returns> 
        public async Task<T> QuerySingleOrDefaultAsync<T>(string sqlquery, DynamicParameters? parameters = null) 
        { 
            using (var connection = _dapperContext.CreateConnection()) 
            { 
                dynamic? result;

                if (parameters == null) 
                { 
                    result = await connection.QuerySingleOrDefaultAsync<T>(sqlquery);
                } 
                else 
                { 
                    result = await connection.QuerySingleOrDefaultAsync<T>(sqlquery, parameters);
                }

                return Task.FromResult<T>(result);
            } 
        }
    }
}
