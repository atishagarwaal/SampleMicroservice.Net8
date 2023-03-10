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
        /// Method to commit changes.
        /// </summary>
        void Commit();

        /// <summary>
        /// Method to commit changes asynchronously.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Method to rollback changes.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Method to rollback changes asynchronously.
        /// </summary>
        Task RollbackAsync();
    }
}
