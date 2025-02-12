using Retail.Api.Customers.Data;
using Retail.Api.Customers.DefaultRepositories;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Model;

namespace Retail.Api.Customers.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class CustomerNotificationRepository : EntityRepository<Customer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerEntityRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public CustomerNotificationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
