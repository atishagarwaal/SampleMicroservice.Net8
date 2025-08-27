using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;

namespace Retail.Orders.Write.ServiceTests.Common
{
    /// <summary>
    /// Test data factory for creating sample entities and DTOs.
    /// </summary>
    public static class TestData
    {
        private static long _orderIdCounter = 1;
        private static long _lineItemIdCounter = 1;

        /// <summary>
        /// Creates a sample Order entity.
        /// </summary>
        /// <returns>A sample Order entity.</returns>
        public static Order CreateSampleOrder()
        {
            var order = new Order
            {
                Id = _orderIdCounter++,
                CustomerId = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99,
                LineItems = new List<LineItem>
                {
                    CreateSampleLineItem()
                }
            };

            // Update line item order ID to match the order
            foreach (var lineItem in order.LineItems)
            {
                lineItem.OrderId = order.Id;
            }

            return order;
        }

        /// <summary>
        /// Creates a list of sample Order entities.
        /// </summary>
        /// <param name="count">Number of orders to create.</param>
        /// <returns>A list of sample Order entities.</returns>
        public static List<Order> CreateSampleOrders(int count)
        {
            var orders = new List<Order>();
            for (int i = 0; i < count; i++)
            {
                orders.Add(CreateSampleOrder());
            }
            return orders;
        }

        /// <summary>
        /// Creates a sample LineItem entity.
        /// </summary>
        /// <returns>A sample LineItem entity.</returns>
        public static LineItem CreateSampleLineItem()
        {
            return new LineItem
            {
                Id = _lineItemIdCounter++,
                OrderId = 1,
                SkuId = 100,
                Qty = 2
            };
        }

        /// <summary>
        /// Creates a list of sample LineItem entities.
        /// </summary>
        /// <param name="count">Number of line items to create.</param>
        /// <returns>A list of sample LineItem entities.</returns>
        public static List<LineItem> CreateSampleLineItems(int count)
        {
            var lineItems = new List<LineItem>();
            for (int i = 0; i < count; i++)
            {
                lineItems.Add(CreateSampleLineItem());
            }
            return lineItems;
        }

        /// <summary>
        /// Creates a sample OrderDto.
        /// </summary>
        /// <returns>A sample OrderDto.</returns>
        public static OrderDto CreateSampleOrderDto()
        {
            return new OrderDto
            {
                Id = _orderIdCounter++,
                CustomerId = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99,
                LineItems = new List<LineItemDto>
                {
                    CreateSampleLineItemDto()
                }
            };
        }

        /// <summary>
        /// Creates a list of sample OrderDto objects.
        /// </summary>
        /// <param name="count">Number of order DTOs to create.</param>
        /// <returns>A list of sample OrderDto objects.</returns>
        public static List<OrderDto> CreateSampleOrderDtos(int count)
        {
            var orderDtos = new List<OrderDto>();
            for (int i = 0; i < count; i++)
            {
                orderDtos.Add(CreateSampleOrderDto());
            }
            return orderDtos;
        }

        /// <summary>
        /// Creates a sample LineItemDto.
        /// </summary>
        /// <returns>A sample LineItemDto.</returns>
        public static LineItemDto CreateSampleLineItemDto()
        {
            return new LineItemDto
            {
                Id = _lineItemIdCounter++,
                OrderId = 1,
                SkuId = 100,
                Qty = 2
            };
        }

        /// <summary>
        /// Creates a list of sample LineItemDto objects.
        /// </summary>
        /// <param name="count">Number of line item DTOs to create.</param>
        /// <returns>A list of sample LineItemDto objects.</returns>
        public static List<LineItemDto> CreateSampleLineItemDtos(int count)
        {
            var lineItemDtos = new List<LineItemDto>();
            for (int i = 0; i < count; i++)
            {
                lineItemDtos.Add(CreateSampleLineItemDto());
            }
            return lineItemDtos;
        }

        /// <summary>
        /// Creates a sample CreateOrderCommand.
        /// </summary>
        /// <returns>A sample CreateOrderCommand.</returns>
        public static CreateOrderCommand CreateSampleCreateOrderCommand()
        {
            return new CreateOrderCommand
            {
                Order = CreateSampleOrderDto()
            };
        }

        /// <summary>
        /// Creates a sample UpdateOrderCommand.
        /// </summary>
        /// <returns>A sample UpdateOrderCommand.</returns>
        public static UpdateOrderCommand CreateSampleUpdateOrderCommand()
        {
            return new UpdateOrderCommand
            {
                Order = CreateSampleOrderDto()
            };
        }

        /// <summary>
        /// Creates a sample DeleteOrderCommand.
        /// </summary>
        /// <returns>A sample DeleteOrderCommand.</returns>
        public static DeleteOrderCommand CreateSampleDeleteOrderCommand()
        {
            return new DeleteOrderCommand
            {
                Id = 1
            };
        }

        /// <summary>
        /// Resets the ID counters for consistent test data.
        /// </summary>
        public static void ResetCounters()
        {
            _orderIdCounter = 1;
            _lineItemIdCounter = 1;
        }
    }
}
