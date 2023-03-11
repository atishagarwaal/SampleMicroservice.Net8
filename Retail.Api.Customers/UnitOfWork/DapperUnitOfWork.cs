using Retail.Api.Customers.Data;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Repositories;

namespace Retail.Api.Customers.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class DapperUnitOfWork : IDapperUnitOfWork
    {
        private readonly DapperContext _dapperContext;
        private ICustomerDapperRepository? _customerDapperRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="dapperContext">Dapper Db context.</param>
        public DapperUnitOfWork(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        public ICustomerDapperRepository CustomerDapperRepository
        {
            get
            {
                if (_customerDapperRepository == null)
                {
                    _customerDapperRepository = new CustomerDapperRepository(_dapperContext);
                }

                return _customerDapperRepository;
            }
        }
    }
}
