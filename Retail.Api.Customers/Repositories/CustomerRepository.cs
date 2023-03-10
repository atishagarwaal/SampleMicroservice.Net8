using Retail.Api.Customers.Data;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Model;

namespace Retail.Api.Customers.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class CustomerRepository : GenericRepository<Customer> , ICustomerRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
