using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using System.Data;

namespace Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets or sets order repository.
        /// </summary>
        IOrderRepository Orders { get; }
    }
}
