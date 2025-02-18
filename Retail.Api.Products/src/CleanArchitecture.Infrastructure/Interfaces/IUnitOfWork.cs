using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories;
using System.Data;

namespace Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface definition for unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
        ISkuRepository Skus { get; }
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
