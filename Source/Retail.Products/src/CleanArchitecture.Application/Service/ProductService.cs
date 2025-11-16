using CommonLibrary.MessageContract;
using InventoryErrorEventNameSpace;
using InventoryUpdatedEventNameSpace;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="skuConverter">Instance of SKU converter.</param>
        /// <param name="skuDtoConverter">Instance of SKU DTO converter.</param>
        /// <param name="skuDtoValidator">Instance of SKU DTO validator.</param>
        /// <param name="messagePublisher">Instance of message publisher.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        public ProductService(
            IUnitOfWork unitOfWork,
            IConverter<SkuDto, Sku> skuConverter,
            IConverter<Sku, SkuDto> skuDtoConverter,
            IMessageValidator<SkuDto> skuDtoValidator,
            IMessagePublisher messagePublisher,
            IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _skuConverter = skuConverter;
            _skuDtoConverter = skuDtoConverter;
            _skuDtoValidator = skuDtoValidator;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Method to fetch all products asynchronously.
        /// </summary>
        /// <returns>List of products.</returns>
        public async Task<IEnumerable<SkuDto>> GetAllProductsAsync()
        {
            var list = await _unitOfWork.Skus.GetAllAsync();
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
            var record = await _unitOfWork.Skus.GetByIdAsync(id);
            if (record == null)
            {
                return null!;
            }

            return _skuDtoConverter.Convert(record);
        }

        /// <summary>
        /// Method to add a new product record asynchronously.
        /// </summary>
        /// <param name="custDto">Product record.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> AddProductAsync(SkuDto skuDto)
        {
            // Validate using validator
            var validationResult = _skuDtoValidator.Validate(skuDto);
            if (!validationResult.IsValid)
            {
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

                return _skuDtoConverter.Convert(result);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Method to update product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <param name="custDto">Product record.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> UpdateProductAsync(long id, SkuDto skuDto)
        {
            // Validate using validator
            var validationResult = _skuDtoValidator.Validate(skuDto);
            if (!validationResult.IsValid)
            {
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
                    throw new InvalidOperationException($"Product with ID {id} was not found after update");
                }

                return _skuDtoConverter.Convert(updatedRecord);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Method to delete product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Product object.</returns>
        public async Task<bool> DeleteProductAsync(long id)
        {
            var record = await _unitOfWork.Skus.GetByIdAsync(id);
            if (record == null)
            {
                return false;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Skus.Remove(record);
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

        public async Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent)
        {
            Console.WriteLine($"Product Service: Received OrderCreatedEvent - OrderId: {orderCreatedEvent?.OrderId}, CustomerId: {orderCreatedEvent?.CustomerId}, LineItemsCount: {orderCreatedEvent?.LineItems?.Length ?? 0}");
            
            if (orderCreatedEvent == null)
            {
                Console.WriteLine("Product Service: OrderCreatedEvent is null");
                throw new ArgumentNullException(nameof(orderCreatedEvent));
            }

            if (orderCreatedEvent.LineItems == null || orderCreatedEvent.LineItems.Length == 0)
            {
                Console.WriteLine($"Product Service: OrderCreatedEvent has no LineItems for OrderId: {orderCreatedEvent.OrderId}");
                throw new InvalidOperationException($"OrderCreatedEvent for OrderId {orderCreatedEvent.OrderId} has no LineItems");
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await unitOfWork.BeginTransactionAsync();
            try
            {
                var skuIds = orderCreatedEvent.LineItems.Select(i => i.SkuId).ToList();
                var skuList = await unitOfWork.Skus.ExecuteQueryAsync(i => skuIds.Contains(i.Id));

                if (skuList.Any(i => i.Inventory == 0 || i.Inventory - orderCreatedEvent.LineItems.FirstOrDefault(j => j.SkuId == i.Id)?.Qty < 0))
                {
                    throw new Exception("Inventory is not sufficient");
                }

                foreach (var sku in skuList)
                {
                    var qty = orderCreatedEvent.LineItems.FirstOrDefault(j => j.SkuId == sku.Id)?.Qty ?? 0;
                    sku.Inventory -= (int)qty;
                    unitOfWork.Skus.Update(sku);
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

                Console.WriteLine($"Product Service: Sending InventoryUpdatedEvent - OrderId: {inventoryUpdatedMessage.OrderId}, CustomerId: {inventoryUpdatedMessage.CustomerId}, LineItemsCount: {inventoryUpdatedMessage.LineItems?.Length ?? 0}");
                await _messagePublisher.PublishAsync<InventoryUpdatedEvent>(inventoryUpdatedMessage, RabbitmqConstants.InventoryUpdated).ConfigureAwait(false);
                Console.WriteLine($"Product Service: InventoryUpdatedEvent sent successfully");

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Product Service: Error processing OrderCreatedEvent - {ex.Message}");
                Console.WriteLine($"Product Service: Stack trace - {ex.StackTrace}");
                await unitOfWork.RollbackTransactionAsync();

                var inventoryErrorMessage = new InventoryErrorEvent
                {
                    CustomerId = orderCreatedEvent.CustomerId,
                    OrderDate = orderCreatedEvent.OrderDate.DateTime,
                    OrderId = orderCreatedEvent.OrderId,
                    TotalAmount = orderCreatedEvent.TotalAmount,
                };

                await _messagePublisher.PublishAsync<InventoryErrorEvent>(inventoryErrorMessage, RabbitmqConstants.InventoryError).ConfigureAwait(false);
                throw;
            }
        }
    }
}