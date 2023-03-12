using Retail.Api.Orders.Model;
using System.Linq.Expressions;

namespace Retail.Api.Orders.Interface
{
    /// <summary>
    /// Interface definition for customer repository.
    /// </summary>
    public interface ILineItemEntityRepository : IEntityRepository<LineItem>
    {
      
    }
}
