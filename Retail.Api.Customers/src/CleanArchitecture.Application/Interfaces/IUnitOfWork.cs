using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using System.Data;

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces
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
