// <copyright file="IRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.Interface
{
    using Dapper;

    /// <summary>
    /// Interface definition for data access layer
    /// </summary>
    public interface IDapperRepository
    { 
        /// <summary> 
        /// Runs raw sql query. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <returns>Returns data or status.</returns> 
        Task<dynamic> ExecuteScalarAsync(string sqlquery);

        /// <summary> 
        /// Runs raw sql query. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <param name="parameters">Sql parameters.</param> 
        /// <returns>Returns data or status.</returns> 
        Task<int> ExecuteAsync(string sqlquery, DynamicParameters? parameters = null);

        /// <summary> 
        /// Runs raw sql query. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <returns>Returns data or status.</returns> 
        dynamic ExecuteScalar(string sqlquery);

        /// <summary> 
        /// Runs raw sql query. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <param name="parameters">Sql parameters.</param> 
        /// <returns>Returns data or status.</returns> 
        int Execute(string sqlquery, DynamicParameters? parameters = null);

        /// <summary> 
        /// Runs raw query multiple. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <param name="parameters">Sql parameters.</param> 
        /// <returns>Returns data or status.</returns> 
        Task<IEnumerable<T>> QueryMultipleAsync<T>(string sqlquery, DynamicParameters? parameters = null);

        /// <summary> 
        /// Runs raw query single or default. 
        /// </summary> 
        /// <param name="sqlquery">Raw sql query.</param> 
        /// <param name="parameters">Sql parameters.</param> 
        /// <returns>Returns data or status.</returns> 
        Task<T> QuerySingleOrDefaultAsync<T>(string sqlquery, DynamicParameters? parameters = null);
    }
}