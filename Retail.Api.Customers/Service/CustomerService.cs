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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
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
            var list = await _unitOfWork.CustomerRepository.GetAllAsync();

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
            var record = await _unitOfWork.CustomerRepository.GetByIdAsync(id);

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
            _unitOfWork.BeginTransaction();
            var result = await _unitOfWork.CustomerRepository.AddAsync(custObj);
            _unitOfWork.Commit();

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
            var record = _mapper.Map<Customer>(custDto);

            // Update record
            _unitOfWork.BeginTransaction();
            _unitOfWork.CustomerRepository.Update(record);
            _unitOfWork.Commit();

            record = await _unitOfWork.CustomerRepository.GetByIdAsync(id);

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
            var record = await _unitOfWork.CustomerRepository.GetByIdAsync(id);

            if (record != null)
            {
                // Delete record
                _unitOfWork.BeginTransaction();
                _unitOfWork.CustomerRepository.Remove(record);
                _unitOfWork.Commit();

                return true;
            }

            return false;
        }
    }
}
