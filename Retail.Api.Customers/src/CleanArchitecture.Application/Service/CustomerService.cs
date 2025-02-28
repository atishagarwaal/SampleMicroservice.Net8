using AutoMapper;
using CommonLibrary.MessageContract;
using Microsoft.Extensions.DependencyInjection;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Service
{
    /// <summary>
    /// Customer service class.
    /// </summary>
    internal class CustomerService : ICustomerService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Method to fetch all customers asynchronously.
        /// </summary>
        /// <returns>List of customers.</returns>
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var returnList = new List<CustomerDto>();

            // Get all customers
            var list = await _unitOfWork.Customers.GetAllAsync();

            // Transform data
            foreach (var item in list)
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
            var record = await _unitOfWork.Customers.GetByIdAsync(id);

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
            var result = await _unitOfWork.Customers.AddAsync(custObj);
            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

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
            await _unitOfWork.BeginTransactionAsync();
            _unitOfWork.Customers.Update(record);
            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            record = await _unitOfWork.Customers.GetByIdAsync(id);

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
            var record = await _unitOfWork.Customers.GetByIdAsync(id);

            if (record != null)
            {
                // Delete record
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Customers.Remove(record);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }

            return false;
        }

        public async Task HandleOrderCreatedEvent(InventoryUpdatedEvent inventoryUpdatedEvent)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var orderId = inventoryUpdatedEvent.OrderId;
                    var customerId = inventoryUpdatedEvent.CustomerId;
                    var notification = new Notification
                    {
                        OrderId = orderId,
                        CustomerId = customerId,
                        Message = "Order created successfully",
                        OrderDate = DateTime.Now,
                    };

                    notification = await unitOfWork.Notifications.AddAsync(notification);

                    Console.WriteLine(notification.Message);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
