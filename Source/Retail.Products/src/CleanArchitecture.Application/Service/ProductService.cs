using CommonLibrary.MessageContract;
using InventoryErrorEventNameSpace;
using InventoryUpdatedEventNameSpace;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
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
        public ProductService(
            IUnitOfWork unitOfWork,
            IConverter<SkuDto, Sku> skuConverter,
            IConverter<Sku, SkuDto> skuDtoConverter,
            IMessageValidator<SkuDto> skuDtoValidator,
            IMessagePublisher messagePublisher,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _skuConverter = skuConverter;
            _skuDtoConverter = skuDtoConverter;
            _skuDtoValidator = skuDtoValidator;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Method to fetch all products asynchronously.
        /// </summary>
        /// <returns>List of products.</returns>
        public async Task<IEnumerable<SkuDto>> GetAllProductsAsync()
        {
            _logger.LogInformation("Fetching all products");
            var list = await _unitOfWork.Skus.GetAllAsync();
            var count = list?.Count() ?? 0;
            _logger.LogInformation("Retrieved {ProductCount} products", count);
            return list
                .Where(sku => sku != null)
                .Select(sku => _skuDtoConverter.Convert(sku));
        }

        /// <summary>
        /// Method to fetch product record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> GetProductByIdAsync(long id)
        {
            _logger.LogInformation("Fetching product by ID. ProductId: {ProductId}", id);
            var record = await _unitOfWork.Skus.GetByIdAsync(id);
            if (record == null)
            {
                _logger.LogWarning("Product not found. ProductId: {ProductId}", id);
                return null!;
            }

            _logger.LogInformation("Product retrieved successfully. ProductId: {ProductId}, Name: {ProductName}", 
                id, record.Name);
            return _skuDtoConverter.Convert(record);
        }

        /// <summary>
        /// Method to add a new product record asynchronously.
        /// </summary>
        /// <param name="skuDto">Product record.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> AddProductAsync(SkuDto skuDto)
        {
            _logger.LogInformation("Adding new product. Name: {ProductName}, UnitPrice: {UnitPrice}, Inventory: {Inventory}",
                skuDto.Name, skuDto.UnitPrice, skuDto.Inventory);

            // Validate using validator
            var validationResult = _skuDtoValidator.Validate(skuDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Product validation failed. Validator: {ValidatorName}, Reason: {FailureReason}",
                    validationResult.ValidatorName, validationResult.FailureReason);
                throw new ArgumentException(validationResult.FailureReason ?? "Validation failed", nameof(skuDto));
            }

            // Convert DTO to entity
            var sku = _skuConverter.Convert(skuDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _unitOfWork.Skus.AddAsync(sku);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Product added successfully. ProductId: {ProductId}, Name: {ProductName}",
                    result.Id, result.Name);

                return _skuDtoConverter.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product. Name: {ProductName}", skuDto.Name);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Method to update product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <param name="skuDto">Product record.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> UpdateProductAsync(long id, SkuDto skuDto)
        {
            _logger.LogInformation("Updating product. ProductId: {ProductId}, Name: {ProductName}",
                id, skuDto.Name);

            // Validate using validator
            var validationResult = _skuDtoValidator.Validate(skuDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Product validation failed. ProductId: {ProductId}, Validator: {ValidatorName}, Reason: {FailureReason}",
                    id, validationResult.ValidatorName, validationResult.FailureReason);
                throw new ArgumentException(validationResult.FailureReason ?? "Validation failed", nameof(skuDto));
            }

            // Convert DTO to entity
            var record = _skuConverter.Convert(skuDto);
            record.Id = id; // Ensure the ID from the parameter is used

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Skus.Update(record);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                var updatedRecord = await _unitOfWork.Skus.GetByIdAsync(id);
                if (updatedRecord == null)
                {
                    _logger.LogError("Product not found after update. ProductId: {ProductId}", id);
                    throw new InvalidOperationException($"Product with ID {id} was not found after update");
                }

                _logger.LogInformation("Product updated successfully. ProductId: {ProductId}, Name: {ProductName}",
                    id, updatedRecord.Name);

                return _skuDtoConverter.Convert(updatedRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product. ProductId: {ProductId}, Name: {ProductName}",
                    id, skuDto.Name);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Method to delete product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        public async Task<bool> DeleteProductAsync(long id)
        {
            _logger.LogInformation("Deleting product. ProductId: {ProductId}", id);

            var record = await _unitOfWork.Skus.GetByIdAsync(id);
            if (record == null)
            {
                _logger.LogWarning("Product not found for deletion. ProductId: {ProductId}", id);
                return false;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Skus.Remove(record);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Product deleted successfully. ProductId: {ProductId}, Name: {ProductName}",
                    id, record.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product. ProductId: {ProductId}", id);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
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
                throw new ArgumentNullException(nameof(orderCreatedEvent));
            }

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["OrderId"] = orderCreatedEvent.OrderId,
                ["CustomerId"] = orderCreatedEvent.CustomerId
            }))
            {
                _logger.LogInformation("Processing OrderCreatedEvent. LineItemsCount: {LineItemsCount}",
                    orderCreatedEvent.LineItems?.Length ?? 0);

                if (orderCreatedEvent.LineItems == null || orderCreatedEvent.LineItems.Length == 0)
                {
                    _logger.LogError("OrderCreatedEvent has no LineItems. OrderId: {OrderId}", orderCreatedEvent.OrderId);
                    throw new InvalidOperationException($"OrderCreatedEvent for OrderId {orderCreatedEvent.OrderId} has no LineItems");
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
                        _logger.LogError("Insufficient inventory for order. OrderId: {OrderId}", orderCreatedEvent.OrderId);
                        throw new Exception("Inventory is not sufficient");
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
                    _logger.LogInformation("OrderCreatedEvent processed successfully. Inventory updated and event published");
                }
                catch (Exception ex)
                {
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