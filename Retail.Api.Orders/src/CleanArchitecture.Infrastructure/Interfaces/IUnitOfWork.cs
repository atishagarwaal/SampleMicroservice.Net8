using Retail.Api.Orders.src.CleanArchitecture.Domain.Model;
using System.Data;

namespace Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces
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
