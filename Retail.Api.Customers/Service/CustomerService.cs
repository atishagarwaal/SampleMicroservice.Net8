using AutoMapper;
using Retail.Api.Customers.DefaultInterface;
using Retail.Api.Customers.Dto;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Model;

namespace Retail.Api.Customers.Service
{
    /// <summary>
    /// Customer service class.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _entityUnitOfWork;
        private readonly IUnitOfWork _dapperUnitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="entityUnitOfWork">Intance of unit of work class.</param>
        /// <param name="dapperUnitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public CustomerService(IUnitOfWork entityUnitOfWork, IUnitOfWork dapperUnitOfWork, IMapper mapper)
        {
            _entityUnitOfWork = entityUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Method to fetch all customers asynchronously.
        /// </summary>
        /// <returns>List of customers.</returns>
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var returnList = new List<CustomerDto>();

            // Get all customers
            var list = await _dapperUnitOfWork.CustomerRepository.GetAllAsync();

            // Transform data
            foreach(var item in list)
            {
                var custDto = _mapper.Map<CustomerDto>(item);
                returnList.Add(custDto);
            }

            return returnList;
        }

        /// <summary>
        /// Method to fetch customer record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> GetCustomerByIdAsync(long id)
        {
            // Find record
            var record = await _dapperUnitOfWork.CustomerRepository.GetByIdAsync(id);

            // Transform data
            var custDto = _mapper.Map<CustomerDto>(record);

            return custDto;
        }

        /// <summary>
        /// Method to add a new customer record asynchronously.
        /// </summary>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> AddCustomerAsync(CustomerDto custDto)
        {
            // Transform data
            var custObj = _mapper.Map<Customer>(custDto);

            // Add customer
            _dapperUnitOfWork.BeginTransaction();
            var result = await _dapperUnitOfWork.CustomerRepository.AddAsync(custObj);
            _dapperUnitOfWork.Commit();

            // Transform data
            custDto = _mapper.Map<CustomerDto>(result);

            return custDto;

        }

        /// <summary>
        /// Method to update customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> UpdateCustomerAsync(long id, CustomerDto custDto)
        {
            // Find record
            var record = await _dapperUnitOfWork.CustomerRepository.GetByIdAsync(id);

            record = _mapper.Map<Customer>(custDto);

            // Update record
            _dapperUnitOfWork.BeginTransaction();
            var result = _dapperUnitOfWork.CustomerRepository.Update(record);
            _dapperUnitOfWork.Commit();

            record = await _dapperUnitOfWork.CustomerRepository.GetByIdAsync(id);

            // Transform data
            custDto = _mapper.Map<CustomerDto>(record);

            return custDto;
        }

        /// <summary>
        /// Method to delete customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Customer object.</returns>
        public async Task<bool> DeleteCustomerAsync(long id)
        {
            // Find record
            var record = await _dapperUnitOfWork.CustomerRepository.GetByIdAsync(id);

            if (record != null)
            {
                // Delete record
                _dapperUnitOfWork.BeginTransaction();
                _dapperUnitOfWork.CustomerRepository.Remove(record);
                _dapperUnitOfWork.Commit();

                return true;
            }

            return false;
        }
    }
}
