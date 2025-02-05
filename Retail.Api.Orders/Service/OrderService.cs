using AutoMapper;
using MessagingLibrary.Interface;
using Retail.Api.Orders.DefaultInterface;
using Retail.Api.Orders.Dto;
using Retail.Api.Orders.Interface;
using Retail.Api.Orders.MessageContract;
using Retail.Api.Orders.Model;

namespace Retail.Api.Orders.Service
{
    /// <summary>
    /// Order service class.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IMessagePublisher messagePublisher)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
        }

        /// <summary>
        /// Method to fetch all orders asynchronously.
        /// </summary>
        /// <returns>List of orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var returnList = new List<OrderDto>();

            // Get all orders
            var list = await _unitOfWork.OrderRepository.GetAllOrdersAsync();

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
            var order = await _unitOfWork.OrderRepository.GetOrderByIdAsync(id);

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
            // Get order values
            var order = _mapper.Map<Order>(orderDto);

            // Update order in database
            _unitOfWork.BeginTransaction();
            var orderRecord = await _unitOfWork.OrderRepository.AddAsync(order);
            _unitOfWork.Commit();

            _unitOfWork.BeginTransaction();
            var lineitems = orderDto?.LineItems;

            if (lineitems != null)
            {
                foreach (var lineitem in lineitems)
                {
                    if (lineitem != null)
                    {
                        // Get lineitem values
                        var lineRecord = _mapper.Map<LineItem>(lineitem);

                        // Add order Id
                        lineRecord.OrderId = orderRecord.Id;

                        // Update line item in database
                        await _unitOfWork.LineItemRepository.AddAsync(lineRecord);
                    }
                }
            }

            _unitOfWork.Commit();

            // Find record
            var record = await _unitOfWork.OrderRepository.GetOrderByIdAsync(orderRecord.Id);

            // Transform data
            orderDto = _mapper.Map<OrderDto>(record);

            var newOrderMessage = new OrderCreatedEvent();
            newOrderMessage = _mapper.Map<OrderCreatedEvent>(orderDto);

            // Publish order creation event
            await _messagePublisher.PublishAsync<OrderCreatedEvent>(newOrderMessage, "OrderCreated").ConfigureAwait(false);

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
            _unitOfWork.BeginTransaction();
            _unitOfWork.OrderRepository.Update(order);

            var lineitems = orderDto?.LineItems;

            if (lineitems != null)
            {
                foreach (var lineitem in lineitems)
                {
                    if (lineitem != null)
                    {
                        // Get lineitem values
                        var lineRecord = _mapper.Map<LineItem>(lineitem);

                        // Update line item in database
                        _unitOfWork.LineItemRepository.Update(lineRecord);
                    }
                }
            }

            _unitOfWork.Commit();

            // Find record
            var record = await _unitOfWork.OrderRepository.GetOrderByIdAsync(id);

            // Transform data
            orderDto = _mapper.Map<OrderDto>(record);

            return orderDto;
        }

        /// <summary>
        /// Method to delete order record asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>Order object.</returns>
        public async Task<bool> RemoveOrderAsync(long id)
        {
            // Find record
            var orderDto = await _unitOfWork.OrderRepository.GetOrderByIdAsync(id);

            if (orderDto?.Id == 0)
            {
                return true;
            }

            // Get order values
            var order = _mapper.Map<Order>(orderDto);

            // Update order in database
            _unitOfWork.BeginTransaction();
            _unitOfWork.OrderRepository.Remove(order);

            var lineitems = orderDto?.LineItems;

            if (lineitems != null)
            {
                foreach (var lineitem in lineitems)
                {
                    if (lineitem != null)
                    {
                        // Get lineitem values
                        var lineRecord = _mapper.Map<LineItem>(lineitem);

                        // Update line item in database
                        _unitOfWork.LineItemRepository.Remove(lineRecord);
                    }
                }
            }

            _unitOfWork.Commit();

            return true;
        }
    }
}
