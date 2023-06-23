using Retail.Api.Products.Interface;
using Retail.Api.Products.Model;
using Retail.Api.Products.Repositories;
using System.Data;

namespace Retail.Api.Products.DefaultInterface
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
