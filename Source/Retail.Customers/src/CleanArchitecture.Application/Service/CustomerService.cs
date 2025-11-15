using CommonLibrary.MessageContract;
using InventoryUpdatedEventNameSpace;
using Microsoft.Extensions.DependencyInjection;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Validation.Interfaces;
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
        private readonly IMessageValidator<CustomerDto> _customerDtoValidator;
        private readonly IConverter<CustomerDto, Customer> _customerConverter;
        private readonly IConverter<Customer, CustomerDto> _customerDtoConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="customerDtoValidator">Instance of customer DTO validator.</param>
        /// <param name="customerConverter">Instance of customer converter.</param>
        /// <param name="customerDtoConverter">Instance of customer DTO converter.</param>
        public CustomerService(
            IUnitOfWork unitOfWork,
            IServiceScopeFactory serviceScopeFactory,
            IMessageValidator<CustomerDto> customerDtoValidator,
            IConverter<CustomerDto, Customer> customerConverter,
            IConverter<Customer, CustomerDto> customerDtoConverter)
        {
            _unitOfWork = unitOfWork;
            _serviceScopeFactory = serviceScopeFactory;
            _customerDtoValidator = customerDtoValidator;
            _customerConverter = customerConverter;
            _customerDtoConverter = customerDtoConverter;
        }

        /// <summary>
        /// Method to fetch all customers asynchronously.
        /// </summary>
        /// <returns>List of customers.</returns>
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return customers
                .Where(customer => customer != null)
                .Select(customer => _customerDtoConverter.Convert(customer));
        }

        /// <summary>
        /// Method to fetch customer record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> GetCustomerByIdAsync(long id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                return null!;
            }

            return _customerDtoConverter.Convert(customer);
        }

        /// <summary>
        /// Method to add a new customer record asynchronously.
        /// </summary>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> AddCustomerAsync(CustomerDto custDto)
        {
            // Validate using validator
            var validationResult = _customerDtoValidator.Validate(custDto);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(validationResult.FailureReason ?? "Validation failed", nameof(custDto));
            }

            // Transform data using converter
            var custObj = _customerConverter.Convert(custDto);

            // Add customer
            try
            {
                var result = await _unitOfWork.Customers.AddAsync(custObj);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _customerDtoConverter.Convert(result);
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
            // Validate using validator
            var validationResult = _customerDtoValidator.Validate(custDto);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(validationResult.FailureReason ?? "Validation failed", nameof(custDto));
            }

            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {id} not found.");
            }

            // Update properties using converter
            var updatedCustomer = _customerConverter.Convert(custDto);
            existingCustomer.FirstName = updatedCustomer.FirstName;
            existingCustomer.LastName = updatedCustomer.LastName;

            // Update record
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Customers.Update(existingCustomer);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _customerDtoConverter.Convert(existingCustomer);
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
