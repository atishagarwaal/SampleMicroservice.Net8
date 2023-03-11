using Retail.Api.Customers.Repositories;

namespace Retail.Api.Customers.Interface
{
    /// <summary>
    /// Interface definition for unit of work.
    /// </summary>
    public interface IEntityUnitOfWork
    {
        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        ICustomerEntityRepository CustomerEntityRepository { get; }

        /// <summary>
        /// Method to begin transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Method to begin transaction asynchronously.
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Method to commit changes.
        /// </summary>
        void Commit();

        /// <summary>
        /// Method to commit changes asynchronously.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Method to commit changes.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Method to commit changes asynchronously.
        /// </summary>
        Task RollbackAsync();
    }
}
