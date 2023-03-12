using AutoMapper;
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
        private readonly IEntityUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public OrderService(IEntityUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
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
            var list = await _unitOfWork.OrderEntityRepository.GetAllOrdersAsync();

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
            // Find record
            var record = await _unitOfWork.OrderEntityRepository.GetByIdAsync(id);

            // Transform data
            var orderDto = _mapper.Map<OrderDto>(record);

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
            await _unitOfWork.BeginTransactionAsync();
            var result = await _unitOfWork.OrderEntityRepository.AddAsync(orderObj);
            await _unitOfWork.CommitAsync();

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
            // Find record
            var record = await _unitOfWork.OrderEntityRepository.GetByIdAsync(id);

            record = _mapper.Map<Order>(orderDto);

            // Update record
            await _unitOfWork.BeginTransactionAsync();
            var result = _unitOfWork.OrderEntityRepository.Update(record);
            await _unitOfWork.CommitAsync();

            // Transform data
            orderDto = _mapper.Map<OrderDto>(result);

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
            var record = await _unitOfWork.OrderEntityRepository.GetByIdAsync(id);

            if (record != null)
            {
                // Delete record
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.OrderEntityRepository.Remove(record);
                await _unitOfWork.CommitAsync();

                return true;
            }

            return false;
        }
    }
}
