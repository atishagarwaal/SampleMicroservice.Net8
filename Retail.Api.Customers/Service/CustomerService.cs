using AutoMapper;
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
        private readonly IEntityUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public CustomerService(IEntityUnitOfWork unitOfWork, IMapper mapper)
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
            var list = await _unitOfWork.CustomerEntityRepository.GetAllAsync();

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
            var record = await _unitOfWork.CustomerEntityRepository.GetByIdAsync(id);

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
            await _unitOfWork.BeginTransactionAsync();
            var result = await _unitOfWork.CustomerEntityRepository.AddAsync(custObj);
            await _unitOfWork.CommitAsync();

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
            var record = await _unitOfWork.CustomerEntityRepository.GetByIdAsync(id);

            record = _mapper.Map<Customer>(custDto);

            // Update record
            await _unitOfWork.BeginTransactionAsync();
            var result = _unitOfWork.CustomerEntityRepository.Update(record);
            await _unitOfWork.CommitAsync();

            // Transform data
            custDto = _mapper.Map<CustomerDto>(result);

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
            var record = await _unitOfWork.CustomerEntityRepository.GetByIdAsync(id);

            if (record != null)
            {
                // Delete record
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.CustomerEntityRepository.Remove(record);
                await _unitOfWork.CommitAsync();

                return true;
            }

            return false;
        }
    }
}
