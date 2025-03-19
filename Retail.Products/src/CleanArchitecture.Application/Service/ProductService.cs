using AutoMapper;
using CommonLibrary.MessageContract;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
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
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IMessagePublisher messagePublisher, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            return _mapper.Map<IEnumerable<SkuDto>>(list);
        }

        /// <summary>
        /// Method to fetch product record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> GetProductByIdAsync(long id)
        {
            var record = await _unitOfWork.Skus.GetByIdAsync(id);
            return _mapper.Map<SkuDto>(record);
        }

        /// <summary>
        /// Method to add a new product record asynchronously.
        /// </summary>
        /// <param name="custDto">Product record.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> AddProductAsync(SkuDto skuDto)
        {
            var sku = _mapper.Map<Sku>(skuDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _unitOfWork.Skus.AddAsync(sku);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<SkuDto>(result);
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
            var record = _mapper.Map<Sku>(skuDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Skus.Update(record);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                var updatedRecord = await _unitOfWork.Skus.GetByIdAsync(id);
                return _mapper.Map<SkuDto>(updatedRecord);
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
                    sku.Inventory -= orderCreatedEvent.LineItems.FirstOrDefault(j => j.SkuId == sku.Id)?.Qty ?? 0;
                    unitOfWork.Skus.Update(sku);
                }

                await unitOfWork.CompleteAsync();

                var inventoryUpdatedMessage = new InventoryUpdatedEvent
                {
                    CustomerId = orderCreatedEvent.CustomerId,
                    OrderDate = orderCreatedEvent.OrderDate,
                    OrderId = orderCreatedEvent.OrderId,
                    TotalAmount = orderCreatedEvent.TotalAmount,
                };

                await _messagePublisher.PublishAsync<InventoryUpdatedEvent>(inventoryUpdatedMessage, RabbitmqConstants.InventoryUpdated).ConfigureAwait(false);

                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();

                var inventoryErrorMessage = new InventoryErrorEvent
                {
                    CustomerId = orderCreatedEvent.CustomerId,
                    OrderDate = orderCreatedEvent.OrderDate,
                    OrderId = orderCreatedEvent.OrderId,
                    TotalAmount = orderCreatedEvent.TotalAmount,
                };

                await _messagePublisher.PublishAsync<InventoryErrorEvent>(inventoryErrorMessage, RabbitmqConstants.InventoryError).ConfigureAwait(false);
                throw;
            }
        }
    }
}