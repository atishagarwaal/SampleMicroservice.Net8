using Retail.Api.Customers.Data;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Model;

namespace Retail.Api.Customers.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class CustomerDapperRepository : DapperRepository, ICustomerDapperRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerDapperRepository"/> class.
        /// </summary>
        /// <param name="dapperContext">Db context.</param>
        public CustomerDapperRepository(DapperContext dapperContext) : base(dapperContext)
        {

        }
    }
}
