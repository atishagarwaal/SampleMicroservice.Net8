using AutoMapper;
using Retail.Api.Orders.DefaultInterface;
using Retail.Api.Orders.Dto;
using Retail.Api.Orders.Interface;
using Retail.Api.Orders.Model;

namespace Retail.Api.Orders.Service
{
    /// <summary>
    /// Order service class.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IEntityUnitOfWork _entityUnitOfWork;
        private readonly IDapperUnitOfWork _dapperUnitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderService"/> class.
        /// </summary>
        /// <param name="entityUnitOfWork">Intance of unit of work class.</param>
        /// <param name="dapperUnitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public OrderService(IEntityUnitOfWork entityUnitOfWork, IDapperUnitOfWork dapperUnitOfWork, IMapper mapper)
        {
            _entityUnitOfWork = entityUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Method to fetch all orders asynchronously.
        /// </summary>
        /// <returns>List of orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var returnList = new List<OrderDto>();

            // Get all orders
            var list = await _entityUnitOfWork.OrderEntityRepository.GetAllOrdersAsync();

            // Transform data
            foreach(var item in list)
            {
                var orderDto = _mapper.Map<OrderDto>(item);
                returnList.Add(orderDto);
            }

            return returnList;
        }

        /// <summary>
        /// Method to fetch order record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>Order object.</returns>
        public async Task<OrderDto> GetOrderByIdAsync(long id)
        {
            ////// Find record
            ////var record = await _entityUnitOfWork.OrderEntityRepository.GetOrderByIdAsync(id);

            ////// Transform data
            ////var orderDto = _mapper.Map<OrderDto>(record);

            ////return orderDto;

            // Find record
            var order = await _dapperUnitOfWork.OrderDapperRepository.GetByIdAsync(id);

            // Transform data
            var orderDto = _mapper.Map<OrderDto>(order);

            return orderDto;
        }

        /// <summary>
        /// Method to add a new order record asynchronously.
        /// </summary>
        /// <param name="orderDto">Order record.</param>
        /// <returns>Order object.</returns>
        public async Task<OrderDto> AddOrderAsync(OrderDto orderDto)
        {
            // Transform data
            var orderObj = _mapper.Map<Order>(orderDto);

            // Add order
            await _entityUnitOfWork.BeginTransactionAsync();
            var result = await _entityUnitOfWork.OrderEntityRepository.AddAsync(orderObj);
            await _entityUnitOfWork.CommitAsync();

            // Transform data
            orderDto = _mapper.Map<OrderDto>(result);

            return orderDto;

        }

        /// <summary>
        /// Method to update order record asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <param name="orderDto">Order record.</param>
        /// <returns>Order object.</returns>
        public async Task<OrderDto> UpdateOrderAsync(long id, OrderDto orderDto)
        {
            // Get order values
            var order = _mapper.Map<Order>(orderDto);

            // Update order in database
            await _entityUnitOfWork.BeginTransactionAsync();
            _entityUnitOfWork.OrderEntityRepository.Update(order);

            var lineitems = orderDto.LineItems;

            if (lineitems != null)
            {
                foreach (var lineitem in lineitems)
                {
                    if (lineitem != null)
                    {
                        // Get lineitem values
                        var lineRecord = _mapper.Map<LineItem>(lineitem);

                        // Update line item in database
                        _entityUnitOfWork.LineItemEntityRepository.Update(lineRecord);
                    }
                }
            }

            await _entityUnitOfWork.CommitAsync();

            // Find record
            var record = await _entityUnitOfWork.OrderEntityRepository.GetOrderByIdAsync(id);

            // Transform data
            orderDto = _mapper.Map<OrderDto>(record);

            return orderDto;
        }

        /// <summary>
        /// Method to delete order record asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>Order object.</returns>
        public async Task<bool> DeleteOrderAsync(long id)
        {
            // Find record
            var record = await _entityUnitOfWork.OrderEntityRepository.GetByIdAsync(id);

            if (record != null)
            {
                // Delete record
                await _entityUnitOfWork.BeginTransactionAsync();
                _entityUnitOfWork.OrderEntityRepository.Remove(record);
                await _entityUnitOfWork.CommitAsync();

                return true;
            }

            return false;
        }
    }
}
