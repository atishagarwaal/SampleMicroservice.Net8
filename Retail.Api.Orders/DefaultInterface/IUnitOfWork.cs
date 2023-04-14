using Retail.Api.Orders.CustomInterface;
using Retail.Api.Orders.Interface;
using Retail.Api.Orders.Model;
using Retail.Api.Orders.Repositories;
using System.Data;

namespace Retail.Api.Orders.DefaultInterface
{
    /// <summary>
    /// Interface definition for unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        IOrderRepository OrderRepository { get; }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        IRepository<LineItem> LineItemRepository { get; }

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
