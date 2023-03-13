using Retail.Api.Customers.DefaultInterface;
using Retail.Api.Customers.Model;
using System.Linq.Expressions;

namespace Retail.Api.Customers.Interface
{
    /// <summary>
    /// Interface definition for customer repository.
    /// </summary>
    public interface ICustomerEntityRepository : IEntityRepository<Customer>
    {
      
    }
}
