using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Repositories;
using Retail.Customers.src.CleanArchitecture.Infrastructure.Interfaces;
using System.Data;

namespace Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface definition for unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        ICustomerRepository Customers { get; }

        /// <summary>
        /// Gets or sets notification repository.
        /// </summary>
        INotificationRepository Notifications { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
