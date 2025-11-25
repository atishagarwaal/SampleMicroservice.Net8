using CommonLibrary.Exceptions;
using CommonLibrary.MessageContract;
using CommonLibrary.Results;
using CommonLibrary.Telemetry;
using InventoryErrorEventNameSpace;
using InventoryUpdatedEventNameSpace;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderCreatedEventNameSpace;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Validation.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.UnitOfWork;

namespace Retail.Api.Products.src.CleanArchitecture.Application.Service
{
    /// <summary>
    /// Product service class.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConverter<SkuDto, Sku> _skuConverter;
        private readonly IConverter<Sku, SkuDto> _skuDtoConverter;
        private readonly IMessageValidator<SkuDto> _skuDtoValidator;
        private readonly ILogger<ProductService> _logger;
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="skuConverter">Instance of SKU converter.</param>
        /// <param name="skuDtoConverter">Instance of SKU DTO converter.</param>
        /// <param name="skuDtoValidator">Instance of SKU DTO validator.</param>
        /// <param name="messagePublisher">Instance of message publisher.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="metrics">Instance of metrics service.</param>
        public ProductService(
            IUnitOfWork unitOfWork,
            IConverter<SkuDto, Sku> skuConverter,
            IConverter<Sku, SkuDto> skuDtoConverter,
            IMessageValidator<SkuDto> skuDtoValidator,
            IMessagePublisher messagePublisher,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ProductService> logger,
            IMetricsService metrics)
        {
            _unitOfWork = unitOfWork;
            _skuConverter = skuConverter;
            _skuDtoConverter = skuDtoConverter;
            _skuDtoValidator = skuDtoValidator;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _metrics = metrics;
        }

        /// <summary>
        /// Method to fetch all products asynchronously.
        /// </summary>
        /// <returns>List of products.</returns>
        public async Task<IEnumerable<SkuDto>> GetAllProductsAsync()
        {
            using (_metrics.TrackDuration("products_operation_duration_seconds", "get_all"))
            {
                _logger.LogInformation("Fetching all products");
                try
                {
                    var list = await _unitOfWork.Skus.GetAllAsync();
                    var count = list?.Count() ?? 0;
                    _metrics.IncrementCounter("products_retrieved_total", count);
                    _logger.LogInformation("Retrieved {ProductCount} products", count);
                    return list
                        .Where(sku => sku != null)
                        .Select(sku => _skuDtoConverter.Convert(sku));
                }
                catch (Exception ex)
                {
                    _metrics.IncrementCounter("products_errors_total", 1, "get_all");
                    _logger.LogError(ex, "Error fetching all products");
                    throw;
                }
            }
        }

        /// <summary>
        /// Method to fetch product record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Result containing the product object if found, or an error message if not found.</returns>
        public async Task<Result<SkuDto>> GetProductByIdAsync(long id)
        {
            using (_metrics.TrackDuration("products_operation_duration_seconds", "get_by_id"))
            {
                this._logger.LogInformation("Fetching product by ID. ProductId: {ProductId}", id);
                
                try
                {
                    var record = await this._unitOfWork.Skus.GetByIdAsync(id);
                    if (record == null)
                    {
                        this._metrics.IncrementCounter("products_not_found_total", 1);
                        this._logger.LogWarning("Product not found. ProductId: {ProductId}", id);
                        return Result<SkuDto>.Failure($"Product with ID {id} not found.");
                    }

                    this._metrics.IncrementCounter("products_retrieved_total", 1);
                    this._logger.LogInformation("Product retrieved successfully. ProductId: {ProductId}, Name: {ProductName}", 
                        id, record.Name);
                    return Result<SkuDto>.Success(this._skuDtoConverter.Convert(record));
                }
                catch (DbUpdateException ex)
                {
                    // Unexpected error: database failure
                    this._metrics.IncrementCounter("products_errors_total", 1, "get_by_id");
                    this._logger.LogError(ex, "Database error fetching product. ProductId: {ProductId}", id);
                    throw; // Let middleware handle
                }
                catch (Exception ex)
                {
                    // Unexpected error: system failure
                    this._metrics.IncrementCounter("products_errors_total", 1, "get_by_id");
                    this._logger.LogError(ex, "Unexpected error fetching product. ProductId: {ProductId}", id);
                    throw; // Let middleware handle
                }
            }
        }

        /// <summary>
        /// Method to add a new product record asynchronously.
        /// </summary>
        /// <param name="skuDto">Product record.</param>
        /// <returns>Result containing the created product object if successful, or an error message if validation fails.</returns>
        public async Task<Result<SkuDto>> AddProductAsync(SkuDto skuDto)
        {
            using (_metrics.TrackDuration("products_operation_duration_seconds", "create"))
            {
                this._logger.LogInformation("Adding new product. Name: {ProductName}, UnitPrice: {UnitPrice}, Inventory: {Inventory}",
                    skuDto.Name, skuDto.UnitPrice, skuDto.Inventory);

                // Validate using validator
                var validationResult = this._skuDtoValidator.Validate(skuDto);
                if (!validationResult.IsValid)
                {
                    this._metrics.IncrementCounter("products_validation_errors_total", 1);
                    this._logger.LogWarning("Product validation failed. Validator: {ValidatorName}, Reason: {FailureReason}",
                        validationResult.ValidatorName, validationResult.FailureReason);
                    return Result<SkuDto>.Failure(validationResult.FailureReason ?? "Validation failed");
                }

                // Convert DTO to entity
                var sku = this._skuConverter.Convert(skuDto);

                try
                {
                    await this._unitOfWork.BeginTransactionAsync();
                    var result = await this._unitOfWork.Skus.AddAsync(sku);
                    await this._unitOfWork.CompleteAsync();
                    await this._unitOfWork.CommitTransactionAsync();

                    this._metrics.IncrementCounter("products_created_total", 1);
                    this._logger.LogInformation("Product added successfully. ProductId: {ProductId}, Name: {ProductName}",
                        result.Id, result.Name);

                    return Result<SkuDto>.Success(this._skuDtoConverter.Convert(result));
                }
                catch (DbUpdateException ex)
                {
                    // Unexpected error: database failure
                    this._metrics.IncrementCounter("products_errors_total", 1, "create");
                    this._logger.LogError(ex, "Database error creating product. Name: {ProductName}", skuDto.Name);
                    await this._unitOfWork.RollbackTransactionAsync();
                    throw; // Let middleware handle
                }
                catch (Exception ex)
                {
                    // Unexpected error: system failure
                    this._metrics.IncrementCounter("products_errors_total", 1, "create");
                    this._logger.LogError(ex, "Unexpected error creating product. Name: {ProductName}", skuDto.Name);
                    await this._unitOfWork.RollbackTransactionAsync();
                    throw; // Let middleware handle
                }
            }
        }

        /// <summary>
        /// Method to update product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <param name="skuDto">Product record.</param>
        /// <returns>Result containing the updated product object if successful, or an error message if validation fails or product not found.</returns>
        public async Task<Result<SkuDto>> UpdateProductAsync(long id, SkuDto skuDto)
        {
            using (_metrics.TrackDuration("products_operation_duration_seconds", "update"))
            {
                this._logger.LogInformation("Updating product. ProductId: {ProductId}, Name: {ProductName}",
                    id, skuDto.Name);

                // Validate using validator
                var validationResult = this._skuDtoValidator.Validate(skuDto);
                if (!validationResult.IsValid)
                {
                    this._metrics.IncrementCounter("products_validation_errors_total", 1);
                    this._logger.LogWarning("Product validation failed. ProductId: {ProductId}, Validator: {ValidatorName}, Reason: {FailureReason}",
                        id, validationResult.ValidatorName, validationResult.FailureReason);
                    return Result<SkuDto>.Failure(validationResult.FailureReason ?? "Validation failed");
                }

                // Check if product exists
                var existingProduct = await this._unitOfWork.Skus.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    this._metrics.IncrementCounter("products_not_found_total", 1);
                    this._logger.LogWarning("Product with Id {ProductId} not found for update", id);
                    return Result<SkuDto>.Failure($"Product with ID {id} not found.");
                }

                // Convert DTO to entity
                var record = this._skuConverter.Convert(skuDto);
                record.Id = id; // Ensure the ID from the parameter is used

                try
                {
                    await this._unitOfWork.BeginTransactionAsync();
                    this._unitOfWork.Skus.Update(record);
                    await this._unitOfWork.CompleteAsync();
                    await this._unitOfWork.CommitTransactionAsync();

                    var updatedRecord = await this._unitOfWork.Skus.GetByIdAsync(id);
                    if (updatedRecord == null)
                    {
                        // This is unexpected - product should exist after update
                        this._logger.LogError("Product not found after update. ProductId: {ProductId}", id);
                        throw new NotFoundException($"Product with ID {id} was not found after update");
                    }

                    this._metrics.IncrementCounter("products_updated_total", 1);
                    this._logger.LogInformation("Product updated successfully. ProductId: {ProductId}, Name: {ProductName}",
                        id, updatedRecord.Name);

                    return Result<SkuDto>.Success(this._skuDtoConverter.Convert(updatedRecord));
                }
                catch (DbUpdateException ex)
                {
                    // Unexpected error: database failure
                    this._metrics.IncrementCounter("products_errors_total", 1, "update");
                    this._logger.LogError(ex, "Database error updating product. ProductId: {ProductId}", id);
                    await this._unitOfWork.RollbackTransactionAsync();
                    throw; // Let middleware handle
                }
                catch (Exception ex)
                {
                    // Unexpected error: system failure
                    this._metrics.IncrementCounter("products_errors_total", 1, "update");
                    this._logger.LogError(ex, "Unexpected error updating product. ProductId: {ProductId}", id);
                    await this._unitOfWork.RollbackTransactionAsync();
                    throw; // Let middleware handle
                }
            }
        }

        /// <summary>
        /// Method to delete product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        public async Task<bool> DeleteProductAsync(long id)
        {
            using (_metrics.TrackDuration("products_operation_duration_seconds", "delete"))
            {
                _logger.LogInformation("Deleting product. ProductId: {ProductId}", id);

                var record = await _unitOfWork.Skus.GetByIdAsync(id);
                if (record == null)
                {
                    this._metrics.IncrementCounter("products_not_found_total", 1);
                    _logger.LogWarning("Product not found for deletion. ProductId: {ProductId}", id);
                    return false;
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync();
                    _unitOfWork.Skus.Remove(record);
                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    this._metrics.IncrementCounter("products_deleted_total", 1);
                    _logger.LogInformation("Product deleted successfully. ProductId: {ProductId}, Name: {ProductName}",
                        id, record.Name);
                    return true;
                }
                catch (DbUpdateException ex)
                {
                    // Unexpected error: database failure
                    this._metrics.IncrementCounter("products_errors_total", 1, "delete");
                    _logger.LogError(ex, "Database error deleting product. ProductId: {ProductId}", id);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw; // Let middleware handle
                }
                catch (Exception ex)
                {
                    // Unexpected error: system failure
                    this._metrics.IncrementCounter("products_errors_total", 1, "delete");
                    _logger.LogError(ex, "Unexpected error deleting product. ProductId: {ProductId}", id);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw; // Let middleware handle
                }
            }
        }

        /// <summary>
        /// Handles the OrderCreatedEvent to update inventory.
        /// </summary>
        /// <param name="orderCreatedEvent">The order created event.</param>
        /// <returns>Task representing the async operation.</returns>
        public async Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent)
        {
            if (orderCreatedEvent == null)
            {
                _logger.LogError("OrderCreatedEvent is null");
                _metrics.IncrementCounter("inventory_update_errors_total", 1, "null_event");
                throw new ArgumentNullException(nameof(orderCreatedEvent));
            }

            using (_metrics.TrackDuration("inventory_update_duration_seconds"))
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["OrderId"] = orderCreatedEvent.OrderId,
                ["CustomerId"] = orderCreatedEvent.CustomerId
            }))
            {
                _metrics.IncrementCounter("inventory_updates_total", 1);
                _logger.LogInformation("Processing OrderCreatedEvent. LineItemsCount: {LineItemsCount}",
                    orderCreatedEvent.LineItems?.Length ?? 0);

                if (orderCreatedEvent.LineItems == null || orderCreatedEvent.LineItems.Length == 0)
                {
                    _logger.LogError("OrderCreatedEvent has no LineItems. OrderId: {OrderId}", orderCreatedEvent.OrderId);
                    throw new BusinessRuleException($"OrderCreatedEvent for OrderId {orderCreatedEvent.OrderId} has no LineItems");
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await unitOfWork.BeginTransactionAsync();
                try
                {
                    var skuIds = orderCreatedEvent.LineItems.Select(i => i.SkuId).ToList();
                    _logger.LogDebug("Querying SKUs for inventory update. SkuIds: {SkuIds}", string.Join(", ", skuIds));
                    
                    var skuList = await unitOfWork.Skus.ExecuteQueryAsync(i => skuIds.Contains(i.Id));

                    if (skuList.Any(i => i.Inventory == 0 || i.Inventory - orderCreatedEvent.LineItems.FirstOrDefault(j => j.SkuId == i.Id)?.Qty < 0))
                    {
                        this._metrics.IncrementCounter("inventory_update_errors_total", 1, "insufficient_inventory");
                        _logger.LogError("Insufficient inventory for order. OrderId: {OrderId}", orderCreatedEvent.OrderId);
                        throw new BusinessRuleException($"Insufficient inventory for order {orderCreatedEvent.OrderId}");
                    }

                    foreach (var sku in skuList)
                    {
                        var qty = orderCreatedEvent.LineItems.FirstOrDefault(j => j.SkuId == sku.Id)?.Qty ?? 0;
                        var oldInventory = sku.Inventory;
                        sku.Inventory -= (int)qty;
                        unitOfWork.Skus.Update(sku);
                        
                        _logger.LogDebug("Updating inventory for SKU. SkuId: {SkuId}, OldInventory: {OldInventory}, Quantity: {Quantity}, NewInventory: {NewInventory}",
                            sku.Id, oldInventory, qty, sku.Inventory);
                    }

                    await unitOfWork.CompleteAsync();

                    var inventoryUpdatedMessage = new InventoryUpdatedEvent
                    {
                        CustomerId = orderCreatedEvent.CustomerId,
                        OrderDate = orderCreatedEvent.OrderDate,
                        OrderId = orderCreatedEvent.OrderId,
                        TotalAmount = orderCreatedEvent.TotalAmount,
                        LineItems = orderCreatedEvent.LineItems.Select(li => new InventoryUpdatedEventNameSpace.LineItem
                        {
                            Id = li.Id,
                            OrderId = li.OrderId,
                            SkuId = li.SkuId,
                            Qty = li.Qty
                        }).ToArray(),
                    };

                    _logger.LogInformation("Publishing InventoryUpdatedEvent. LineItemsCount: {LineItemsCount}",
                        inventoryUpdatedMessage.LineItems?.Length ?? 0);
                    await _messagePublisher.PublishAsync<InventoryUpdatedEvent>(inventoryUpdatedMessage, RabbitmqConstants.InventoryUpdated).ConfigureAwait(false);
                    _logger.LogInformation("InventoryUpdatedEvent published successfully");

                    await unitOfWork.CommitTransactionAsync();
                    this._metrics.IncrementCounter("inventory_updates_success_total", 1);
                    _logger.LogInformation("OrderCreatedEvent processed successfully. Inventory updated and event published");
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("inventory_update_errors_total", 1, "processing_error");
                    _logger.LogError(ex, "Error processing OrderCreatedEvent");

                    await unitOfWork.RollbackTransactionAsync();

                    var inventoryErrorMessage = new InventoryErrorEvent
                    {
                        CustomerId = orderCreatedEvent.CustomerId,
                        OrderDate = orderCreatedEvent.OrderDate.DateTime,
                        OrderId = orderCreatedEvent.OrderId,
                        TotalAmount = orderCreatedEvent.TotalAmount,
                    };

                    _logger.LogInformation("Publishing InventoryErrorEvent due to processing failure");
                    await _messagePublisher.PublishAsync<InventoryErrorEvent>(inventoryErrorMessage, RabbitmqConstants.InventoryError).ConfigureAwait(false);
                    throw;
                }
            }
        }
    }
}