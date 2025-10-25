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
    public class CustomerService : ICustomerService
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
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        /// <summary>
        /// Method to fetch customer record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> GetCustomerByIdAsync(long id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            return _mapper.Map<CustomerDto>(customer);
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
            try
            {
                var result = await _unitOfWork.Customers.AddAsync(custObj);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<CustomerDto>(result);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Method to update customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> UpdateCustomerAsync(long id, CustomerDto custDto)
        {
            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {id} not found.");
            }

            _mapper.Map(custDto, existingCustomer);

            // Update record
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Customers.Update(existingCustomer);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<CustomerDto>(existingCustomer);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
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

            if (record == null)
            {
                return false;
            }

            // Delete record
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Customers.Remove(record);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task HandleOrderCreatedEvent(InventoryUpdatedEvent inventoryUpdatedEvent)
        {
            Console.WriteLine($"Customer Service: Received InventoryUpdatedEvent - OrderId: {inventoryUpdatedEvent.OrderId}, CustomerId: {inventoryUpdatedEvent.CustomerId}");
            
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var notification = new Notification
            {
                OrderId = inventoryUpdatedEvent.OrderId,
                CustomerId = inventoryUpdatedEvent.CustomerId,
                Message = "Order created successfully",
                OrderDate = DateTime.UtcNow,
            };

            Console.WriteLine($"Customer Service: Creating notification - OrderId: {notification.OrderId}, CustomerId: {notification.CustomerId}");

            await unitOfWork.BeginTransactionAsync();
            try
            {
                await unitOfWork.Notifications.AddAsync(notification);
                await unitOfWork.CompleteAsync();
                await unitOfWork.CommitTransactionAsync();
                Console.WriteLine($"Customer Service: Notification created successfully - ID: {notification.NotificationId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Customer Service: Error creating notification - {ex.Message}");
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
