using CommonLibrary.MessageContract;
using CommonLibrary.Results;
using CommonLibrary.Telemetry;
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
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="customerDtoValidator">Instance of customer DTO validator.</param>
        /// <param name="customerConverter">Instance of customer converter.</param>
        /// <param name="customerDtoConverter">Instance of customer DTO converter.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="metrics">Instance of metrics service.</param>
        public CustomerService(
            IUnitOfWork unitOfWork,
            IServiceScopeFactory serviceScopeFactory,
            IMessageValidator<CustomerDto> customerDtoValidator,
            IConverter<CustomerDto, Customer> customerConverter,
            IConverter<Customer, CustomerDto> customerDtoConverter,
            ILogger<CustomerService> logger,
            IMetricsService metrics)
        {
            _unitOfWork = unitOfWork;
            _serviceScopeFactory = serviceScopeFactory;
            _customerDtoValidator = customerDtoValidator;
            _customerConverter = customerConverter;
            _customerDtoConverter = customerDtoConverter;
            _logger = logger;
            _metrics = metrics;
        }

        /// <summary>
        /// Method to fetch all customers asynchronously.
        /// </summary>
        /// <returns>List of customers.</returns>
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            using (_metrics.TrackDuration("customers_operation_duration_seconds", "get_all"))
            {
                _logger.LogInformation("Fetching all customers");
                
                try
                {
                    var customers = await _unitOfWork.Customers.GetAllAsync();
                    var customerCount = customers.Count();
                    
                    _metrics.IncrementCounter("customers_retrieved_total", customerCount);
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
                    _metrics.IncrementCounter("customers_errors_total", 1, "get_all");
                    _logger.LogError(ex, "Error fetching all customers");
                    throw;
                }
            }
        }

        /// <summary>
        /// Method to fetch customer record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Result containing the customer object if found, or an error message if not found.</returns>
        public async Task<Result<CustomerDto>> GetCustomerByIdAsync(long id)
        {
            using (_metrics.TrackDuration("customers_operation_duration_seconds", "get_by_id"))
            {
                this._logger.LogInformation("Fetching customer with Id {CustomerId}", id);
                
                try
                {
                    var customer = await this._unitOfWork.Customers.GetByIdAsync(id);
                    
                    if (customer == null)
                    {
                        this._metrics.IncrementCounter("customers_not_found_total", 1);
                        this._logger.LogWarning("Customer with Id {CustomerId} not found in repository", id);
                        return Result<CustomerDto>.Failure($"Customer with ID {id} not found.");
                    }

                    this._metrics.IncrementCounter("customers_retrieved_total", 1);
                    this._logger.LogDebug("Customer with Id {CustomerId} found. Converting to DTO", id);
                    var result = this._customerDtoConverter.Convert(customer);
                    
                    this._logger.LogInformation("Successfully fetched customer with Id {CustomerId}", id);
                    return Result<CustomerDto>.Success(result);
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("customers_errors_total", 1, "get_by_id");
                    this._logger.LogError(ex, "Error fetching customer with Id {CustomerId}", id);
                    return Result<CustomerDto>.Failure($"An error occurred while fetching customer with ID {id}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Method to add a new customer record asynchronously.
        /// </summary>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Result containing the created customer object if successful, or an error message if validation fails.</returns>
        public async Task<Result<CustomerDto>> AddCustomerAsync(CustomerDto custDto)
        {
            using (_metrics.TrackDuration("customers_operation_duration_seconds", "create"))
            {
                this._logger.LogInformation("Adding new customer. FirstName: {FirstName}, LastName: {LastName}", 
                    custDto.FirstName, custDto.LastName);
                
                var validationResult = this._customerDtoValidator.Validate(custDto);
                if (!validationResult.IsValid)
                {
                    this._metrics.IncrementCounter("customers_validation_errors_total", 1);
                    this._logger.LogWarning("Customer validation failed. Validator: {ValidatorName}, Reason: {Reason}",
                        validationResult.ValidatorName, validationResult.FailureReason);
                    return Result<CustomerDto>.Failure(validationResult.FailureReason ?? "Validation failed");
                }

                var custObj = this._customerConverter.Convert(custDto);
                this._logger.LogDebug("Customer DTO converted to entity");

                try
                {
                    await this._unitOfWork.BeginTransactionAsync();
                    var result = await this._unitOfWork.Customers.AddAsync(custObj);
                    await this._unitOfWork.CompleteAsync();
                    await this._unitOfWork.CommitTransactionAsync();

                    this._metrics.IncrementCounter("customers_created_total", 1);
                    this._logger.LogInformation("Customer added successfully. CustomerId: {CustomerId}", result.Id);
                    return Result<CustomerDto>.Success(this._customerDtoConverter.Convert(result));
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("customers_errors_total", 1, "create");
                    this._logger.LogError(ex, "Error adding customer. FirstName: {FirstName}, LastName: {LastName}", 
                        custDto.FirstName, custDto.LastName);
                    await this._unitOfWork.RollbackTransactionAsync();
                    return Result<CustomerDto>.Failure($"An error occurred while adding customer: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Method to update customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Result containing the updated customer object if successful, or an error message if validation fails or customer not found.</returns>
        public async Task<Result<CustomerDto>> UpdateCustomerAsync(long id, CustomerDto custDto)
        {
            using (_metrics.TrackDuration("customers_operation_duration_seconds", "update"))
            {
                this._logger.LogInformation("Updating customer with Id {CustomerId}. FirstName: {FirstName}, LastName: {LastName}", 
                    id, custDto.FirstName, custDto.LastName);
                
                var validationResult = this._customerDtoValidator.Validate(custDto);
                if (!validationResult.IsValid)
                {
                    this._metrics.IncrementCounter("customers_validation_errors_total", 1);
                    this._logger.LogWarning("Customer validation failed for update. CustomerId: {CustomerId}, Validator: {ValidatorName}, Reason: {Reason}",
                        id, validationResult.ValidatorName, validationResult.FailureReason);
                    return Result<CustomerDto>.Failure(validationResult.FailureReason ?? "Validation failed");
                }

                var existingCustomer = await this._unitOfWork.Customers.GetByIdAsync(id);
                if (existingCustomer == null)
                {
                    this._metrics.IncrementCounter("customers_not_found_total", 1);
                    this._logger.LogWarning("Customer with Id {CustomerId} not found for update", id);
                    return Result<CustomerDto>.Failure($"Customer with ID {id} not found.");
                }

                var updatedCustomer = this._customerConverter.Convert(custDto);
                existingCustomer.FirstName = updatedCustomer.FirstName;
                existingCustomer.LastName = updatedCustomer.LastName;
                this._logger.LogDebug("Customer entity updated with new values");

                try
                {
                    await this._unitOfWork.BeginTransactionAsync();
                    this._unitOfWork.Customers.Update(existingCustomer);
                    await this._unitOfWork.CompleteAsync();
                    await this._unitOfWork.CommitTransactionAsync();

                    this._metrics.IncrementCounter("customers_updated_total", 1);
                    this._logger.LogInformation("Customer updated successfully. CustomerId: {CustomerId}", id);
                    return Result<CustomerDto>.Success(this._customerDtoConverter.Convert(existingCustomer));
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("customers_errors_total", 1, "update");
                    this._logger.LogError(ex, "Error updating customer with Id {CustomerId}", id);
                    await this._unitOfWork.RollbackTransactionAsync();
                    return Result<CustomerDto>.Failure($"An error occurred while updating customer: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Method to delete customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>True if customer was deleted, false if not found.</returns>
        public async Task<bool> DeleteCustomerAsync(long id)
        {
            using (_metrics.TrackDuration("customers_operation_duration_seconds", "delete"))
            {
                _logger.LogInformation("Deleting customer with Id {CustomerId}", id);
                
                var record = await _unitOfWork.Customers.GetByIdAsync(id);

                if (record == null)
                {
                    this._metrics.IncrementCounter("customers_not_found_total", 1);
                    _logger.LogWarning("Customer with Id {CustomerId} not found for deletion", id);
                    return false;
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    _unitOfWork.Customers.Remove(record);
                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    
                    this._metrics.IncrementCounter("customers_deleted_total", 1);
                    _logger.LogInformation("Customer deleted successfully. CustomerId: {CustomerId}", id);
                    return true;
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("customers_errors_total", 1, "delete");
                    _logger.LogError(ex, "Error deleting customer with Id {CustomerId}", id);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Handles order created event by creating a notification.
        /// </summary>
        /// <param name="inventoryUpdatedEvent">Inventory updated event.</param>
        public async Task HandleOrderCreatedEvent(InventoryUpdatedEvent inventoryUpdatedEvent)
        {
            using (_metrics.TrackDuration("notifications_creation_duration_seconds"))
            {
                _metrics.IncrementCounter("notifications_created_attempts_total", 1);
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
                        
                        this._metrics.IncrementCounter("notifications_created_total", 1);
                        _logger.LogInformation("Notification created successfully. NotificationId: {NotificationId}, OrderId: {OrderId}, CustomerId: {CustomerId}", 
                            notification.NotificationId, notification.OrderId, notification.CustomerId);
                    }
                    catch (Exception ex)
                    {
                        this._metrics.IncrementCounter("notifications_errors_total", 1, "creation_error");
                        _logger.LogError(ex, "Error creating notification for OrderId: {OrderId}, CustomerId: {CustomerId}", 
                            inventoryUpdatedEvent.OrderId, inventoryUpdatedEvent.CustomerId);
                        await unitOfWork.RollbackTransactionAsync();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("notifications_errors_total", 1, "event_handling_error");
                    _logger.LogError(ex, "Error handling InventoryUpdatedEvent. OrderId: {OrderId}, CustomerId: {CustomerId}", 
                        inventoryUpdatedEvent.OrderId, inventoryUpdatedEvent.CustomerId);
                    throw;
                }
            }
        }
    }
}
