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
            var returnList = new List<SkuDto>();

            // Get all customers
            var list = await _unitOfWork.Skus.GetAllAsync();

            // Transform data
            foreach (var item in list)
            {
                var custDto = _mapper.Map<SkuDto>(item);
                returnList.Add(custDto);
            }

            return returnList;
        }

        /// <summary>
        /// Method to fetch product record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> GetProductByIdAsync(long id)
        {
            // Find record
            var record = await _unitOfWork.Skus.GetByIdAsync(id);

            // Transform data
            var custDto = _mapper.Map<SkuDto>(record);

            return custDto;
        }

        /// <summary>
        /// Method to add a new product record asynchronously.
        /// </summary>
        /// <param name="custDto">Product record.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> AddProductAsync(SkuDto custDto)
        {
            // Transform data
            var custObj = _mapper.Map<Sku>(custDto);

            // Add customer
            await _unitOfWork.BeginTransactionAsync();
            var result = await _unitOfWork.Skus.AddAsync(custObj);
            await _unitOfWork.BeginTransactionAsync();

            // Transform data
            custDto = _mapper.Map<SkuDto>(result);

            return custDto;

        }

        /// <summary>
        /// Method to update product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <param name="custDto">Product record.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> UpdateProductAsync(long id, SkuDto custDto)
        {
            var record = _mapper.Map<Sku>(custDto);

            // Update record
            await _unitOfWork.BeginTransactionAsync();
            _unitOfWork.Skus.Update(record);
            await _unitOfWork.BeginTransactionAsync();

            record = await _unitOfWork.Skus.GetByIdAsync(id);

            // Transform data
            custDto = _mapper.Map<SkuDto>(record);

            return custDto;
        }

        /// <summary>
        /// Method to delete product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Product object.</returns>
        public async Task<bool> DeleteProductAsync(long id)
        {
            // Find record
            var record = await _unitOfWork.Skus.GetByIdAsync(id);

            if (record != null)
            {
                // Delete record
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Skus.Remove(record);
                await _unitOfWork.BeginTransactionAsync();

                return true;
            }

            return false;
        }

        public async Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent)
        {
            try
            {
                //throw new Exception();

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();


                    var skuIds = orderCreatedEvent.LineItems.Select(i => i.SkuId).ToList();

                    // Fetch all product SKUs at once to avoid multiple DB calls
                    var skuList = await unitOfWork.Skus.ExecuteQueryAsync(i => skuIds.Contains(i.Id));

                    if (skuList.Any(i => i.Inventory == 0 || i.Inventory - orderCreatedEvent.LineItems.FirstOrDefault(j => j.SkuId == i.Id)?.Qty < 0))
                    {
                        throw new Exception("Inventory is not sufficient");
                    }

                    foreach (var sku in skuList)
                    {
                        sku.Inventory = (int)(sku.Inventory - orderCreatedEvent.LineItems.FirstOrDefault(j => j.SkuId == sku.Id)?.Qty);
                        unitOfWork.Skus.Update(sku);
                    }

                    var inventoryUpdatedMessage = new InventoryUpdatedEvent
                    {
                        CustomerId = orderCreatedEvent.CustomerId,
                        OrderDate = orderCreatedEvent.OrderDate,
                        OrderId = orderCreatedEvent.OrderId,
                        TotalAmount = orderCreatedEvent.TotalAmount,
                    };

                    await _messagePublisher.PublishAsync<InventoryUpdatedEvent>(inventoryUpdatedMessage, RabbitmqConstants.InventoryUpdated).ConfigureAwait(false);
                }
            }
            catch (Exception ex) 
            {
                var inventoryErrorMessage = new InventoryErrorEvent
                {
                    CustomerId = orderCreatedEvent.CustomerId,
                    OrderDate = orderCreatedEvent.OrderDate,
                    OrderId = orderCreatedEvent.OrderId,
                    TotalAmount = orderCreatedEvent.TotalAmount,
                };

                await _messagePublisher.PublishAsync<InventoryErrorEvent>(inventoryErrorMessage, RabbitmqConstants.InventoryError).ConfigureAwait(false);
            }
        }
    }
}
