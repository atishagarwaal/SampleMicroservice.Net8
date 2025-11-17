using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
using Retail.Customers.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    internal class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
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
