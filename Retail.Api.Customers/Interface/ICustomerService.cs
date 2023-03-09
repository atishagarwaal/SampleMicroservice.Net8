using Retail.Api.Customers.Dto;

namespace Retail.Api.Customers.Interface
{
    public interface ICustomerService
    {
        CustomerDto GetCustomerById(int id);
    }
}
