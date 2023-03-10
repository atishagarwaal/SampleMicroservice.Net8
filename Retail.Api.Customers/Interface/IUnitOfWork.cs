namespace Retail.Api.Customers.Interface
{
    public interface IUnitOfWork
    {
        ICustomerRepository customerRepository { get; }
        void Commit();
        void Rollback();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
