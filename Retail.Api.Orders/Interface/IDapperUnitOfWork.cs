using Retail.Api.Orders.Repositories;

namespace Retail.Api.Orders.Interface
{
    /// <summary>
    /// Interface definition for unit of work.
    /// </summary>
    public interface IDapperUnitOfWork
    {
        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        IOrderDapperRepository OrderDapperRepository { get; }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        ILineItemDapperRepository LineItemDapperRepository { get; }

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
