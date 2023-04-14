using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Model;
using Retail.Api.Customers.Repositories;
using System.Data;

namespace Retail.Api.Customers.DefaultInterface
{
    /// <summary>
    /// Interface definition for unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        IRepository<Customer> CustomerRepository { get; }

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
