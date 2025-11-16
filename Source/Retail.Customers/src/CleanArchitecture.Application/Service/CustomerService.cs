using CommonLibrary.MessageContract;
using InventoryUpdatedEventNameSpace;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CustomerService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="customerDtoValidator">Instance of customer DTO validator.</param>
        /// <param name="customerConverter">Instance of customer converter.</param>
        /// <param name="customerDtoConverter">Instance of customer DTO converter.</param>
        /// <param name="logger">Instance of logger.</param>
        public CustomerService(
            IUnitOfWork unitOfWork,
            IServiceScopeFactory serviceScopeFactory,
            IMessageValidator<CustomerDto> customerDtoValidator,
            IConverter<CustomerDto, Customer> customerConverter,
            IConverter<Customer, CustomerDto> customerDtoConverter,
            ILogger<CustomerService> logger)
        {
            _unitOfWork = unitOfWork;
            _serviceScopeFactory = serviceScopeFactory;
            _customerDtoValidator = customerDtoValidator;
            _customerConverter = customerConverter;
            _customerDtoConverter = customerDtoConverter;
            _logger = logger;
        }

        /// <summary>
        /// Method to fetch all customers asynchronously.
        /// </summary>
        /// <returns>List of customers.</returns>
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            _logger.LogInformation("Fetching all customers");
            
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var customerCount = customers.Count();
                
                _logger.LogDebug("Retrieved {CustomerCount} customers from repository", customerCount);
                
                var result = customers
                    .Where(customer => customer != null)
                    .Select(customer => _customerDtoConverter.Convert(customer))
                    .ToList();
                
                var validCustomerCount = result.Count;
                if (validCustomerCount < customerCount)
                {
                    _logger.LogWarning("Filtered out {FilteredCount} null customers from {TotalCount} total customers", 
                        customerCount - validCustomerCount, customerCount);
                }
                
                _logger.LogInformation("Successfully fetched {CustomerCount} customers", validCustomerCount);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all customers");
                throw;
            }
        }

        /// <summary>
        /// Method to fetch customer record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> GetCustomerByIdAsync(long id)
        {
            _logger.LogInformation("Fetching customer with Id {CustomerId}", id);
            
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                
                if (customer == null)
                {
                    _logger.LogWarning("Customer with Id {CustomerId} not found in repository", id);
                    return null!;
                }

                _logger.LogDebug("Customer with Id {CustomerId} found. Converting to DTO", id);
                var result = _customerDtoConverter.Convert(customer);
                
                _logger.LogInformation("Successfully fetched customer with Id {CustomerId}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer with Id {CustomerId}", id);
                throw;
            }
        }

        /// <summary>
        /// Method to add a new customer record asynchronously.
        /// </summary>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> AddCustomerAsync(CustomerDto custDto)
        {
            _logger.LogInformation("Adding new customer. FirstName: {FirstName}, LastName: {LastName}", 
                custDto.FirstName, custDto.LastName);
            
            var validationResult = _customerDtoValidator.Validate(custDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Customer validation failed. Validator: {ValidatorName}, Reason: {Reason}",
                    validationResult.ValidatorName, validationResult.FailureReason);
                throw new ArgumentException(validationResult.FailureReason ?? "Validation failed", nameof(custDto));
            }

            var custObj = _customerConverter.Convert(custDto);
            _logger.LogDebug("Customer DTO converted to entity");

            try
            {
                var result = await _unitOfWork.Customers.AddAsync(custObj);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Customer added successfully. CustomerId: {CustomerId}", result.Id);
                return _customerDtoConverter.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer. FirstName: {FirstName}, LastName: {LastName}", 
                    custDto.FirstName, custDto.LastName);
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
            _logger.LogInformation("Updating customer with Id {CustomerId}. FirstName: {FirstName}, LastName: {LastName}", 
                id, custDto.FirstName, custDto.LastName);
            
            var validationResult = _customerDtoValidator.Validate(custDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Customer validation failed for update. CustomerId: {CustomerId}, Validator: {ValidatorName}, Reason: {Reason}",
                    id, validationResult.ValidatorName, validationResult.FailureReason);
                throw new ArgumentException(validationResult.FailureReason ?? "Validation failed", nameof(custDto));
            }

            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                _logger.LogWarning("Customer with Id {CustomerId} not found for update", id);
                throw new KeyNotFoundException($"Customer with ID {id} not found.");
            }

            var updatedCustomer = _customerConverter.Convert(custDto);
            existingCustomer.FirstName = updatedCustomer.FirstName;
            existingCustomer.LastName = updatedCustomer.LastName;
            _logger.LogDebug("Customer entity updated with new values");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Customers.Update(existingCustomer);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Customer updated successfully. CustomerId: {CustomerId}", id);
                return _customerDtoConverter.Convert(existingCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with Id {CustomerId}", id);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Method to delete customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>True if customer was deleted, false if not found.</returns>
        public async Task<bool> DeleteCustomerAsync(long id)
        {
            _logger.LogInformation("Deleting customer with Id {CustomerId}", id);
            
            var record = await _unitOfWork.Customers.GetByIdAsync(id);

            if (record == null)
            {
                _logger.LogWarning("Customer with Id {CustomerId} not found for deletion", id);
                return false;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Customers.Remove(record);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Customer deleted successfully. CustomerId: {CustomerId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with Id {CustomerId}", id);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Handles order created event by creating a notification.
        /// </summary>
        /// <param name="inventoryUpdatedEvent">Inventory updated event.</param>
        public async Task HandleOrderCreatedEvent(InventoryUpdatedEvent inventoryUpdatedEvent)
        {
            _logger.LogInformation("Received InventoryUpdatedEvent. OrderId: {OrderId}, CustomerId: {CustomerId}", 
                inventoryUpdatedEvent.OrderId, inventoryUpdatedEvent.CustomerId);
            
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var notification = new Notification
                {
                    OrderId = inventoryUpdatedEvent.OrderId,
                    CustomerId = inventoryUpdatedEvent.CustomerId,
                    Message = "Order created successfully",
                    OrderDate = DateTime.UtcNow,
                };

                _logger.LogDebug("Creating notification for OrderId: {OrderId}, CustomerId: {CustomerId}", 
                    notification.OrderId, notification.CustomerId);

                await unitOfWork.BeginTransactionAsync();
                try
                {
                    await unitOfWork.Notifications.AddAsync(notification);
                    await unitOfWork.CompleteAsync();
                    await unitOfWork.CommitTransactionAsync();
                    
                    _logger.LogInformation("Notification created successfully. NotificationId: {NotificationId}, OrderId: {OrderId}, CustomerId: {CustomerId}", 
                        notification.NotificationId, notification.OrderId, notification.CustomerId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating notification for OrderId: {OrderId}, CustomerId: {CustomerId}", 
                        inventoryUpdatedEvent.OrderId, inventoryUpdatedEvent.CustomerId);
                    await unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling InventoryUpdatedEvent. OrderId: {OrderId}, CustomerId: {CustomerId}", 
                    inventoryUpdatedEvent.OrderId, inventoryUpdatedEvent.CustomerId);
                throw;
            }
        }
    }
}
