namespace Retail.Api.Customers.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository customerRepository { get; }
        int Complete();
    }
}
