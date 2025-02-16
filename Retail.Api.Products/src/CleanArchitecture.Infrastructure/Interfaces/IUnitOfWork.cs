using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using System.Data;

namespace Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface definition for unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets or sets product repository.
        /// </summary>
        IRepository<Sku> ProductRepository { get; }

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
