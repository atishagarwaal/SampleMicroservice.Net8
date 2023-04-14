using Retail.Api.Orders.Interface;
using Retail.Api.Orders.Repositories;
using System.Data;

namespace Retail.Api.Orders.DefaultInterface
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
