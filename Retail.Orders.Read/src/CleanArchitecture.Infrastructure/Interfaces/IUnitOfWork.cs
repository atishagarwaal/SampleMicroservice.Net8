using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using System.Data;

namespace Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface definition for unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets or sets order repository.
        /// </summary>
        IOrderRepository Orders { get; }

        /// <summary>
        /// Gets or sets line item repository.
        /// </summary>
        ILineItemRepository LineItems { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
