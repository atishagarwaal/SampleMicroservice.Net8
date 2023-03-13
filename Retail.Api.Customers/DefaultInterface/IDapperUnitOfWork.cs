using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Repositories;
using System.Data;

namespace Retail.Api.Customers.DefaultInterface
{
    /// <summary>
    /// Interface definition for unit of work.
    /// </summary>
    public interface IDapperUnitOfWork
    {
        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        ICustomerDapperRepository CustomerDapperRepository { get; }

        /// <summary>
        /// Gets or sets connection.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Method to begin transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Method to commit changes.
        /// </summary>
        void Commit();

        /// <summary>
        /// Method to commit changes.
        /// </summary>
        void Rollback();
    }
}
