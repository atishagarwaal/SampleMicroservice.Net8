using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Customers.src.CleanArchitecture.Infrastructure.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
    }
}
