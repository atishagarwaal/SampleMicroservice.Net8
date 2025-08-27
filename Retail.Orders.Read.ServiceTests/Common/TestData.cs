using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Read.ServiceTests.Common
{
    /// <summary>
    /// Test data for order service tests.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// Creates a sample order entity for testing.
        /// </summary>
        /// <returns>Sample order entity.</returns>
        public static Order CreateSampleOrder()
        {
            return new Order
            {
                Id = 1L,
                CustomerId = 123L,
                OrderDate = new DateTime(2024, 1, 15),
                TotalAmount = 99.99,
                LineItems = new List<LineItem>
                {
                    new LineItem
                    {
                        Id = 1L,
                        OrderId = 1L,
                        SkuId = 100L,
                        Qty = 2
                    },
                    new LineItem
                    {
                        Id = 2L,
                        OrderId = 1L,
                        SkuId = 200L,
                        Qty = 1
                    }
                }
            };
        }

        /// <summary>
        /// Creates a sample order DTO for testing.
        /// </summary>
        /// <returns>Sample order DTO.</returns>
        public static OrderDto CreateSampleOrderDto()
        {
            return new OrderDto
            {
                Id = 1L,
                CustomerId = 123L,
                OrderDate = new DateTime(2024, 1, 15),
                TotalAmount = 99.99,
                LineItems = new List<LineItemDto>
                {
                    new LineItemDto
                    {
                        Id = 1L,
                        OrderId = 1L,
                        SkuId = 100L,
                        Qty = 2
                    },
                    new LineItemDto
                    {
                        Id = 2L,
                        OrderId = 1L,
                        SkuId = 200L,
                        Qty = 1
                    }
                }
            };
        }

        /// <summary>
        /// Creates a list of sample orders for testing.
        /// </summary>
        /// <param name="count">Number of orders to create.</param>
        /// <returns>List of sample orders.</returns>
        public static List<Order> CreateSampleOrders(int count = 3)
        {
            var orders = new List<Order>();
            for (int i = 1; i <= count; i++)
            {
                orders.Add(new Order
                {
                    Id = i,
                    CustomerId = 100L + i,
                    OrderDate = new DateTime(2024, 1, 15).AddDays(i),
                    TotalAmount = 50.0 + (i * 10.0),
                    LineItems = new List<LineItem>
                    {
                        new LineItem
                        {
                            Id = i * 10L,
                            OrderId = i,
                            SkuId = 1000L + i,
                            Qty = i
                        }
                    }
                });
            }
            return orders;
        }

        /// <summary>
        /// Creates a list of sample order DTOs for testing.
        /// </summary>
        /// <param name="count">Number of order DTOs to create.</param>
        /// <returns>List of sample order DTOs.</returns>
        public static List<OrderDto> CreateSampleOrderDtos(int count = 3)
        {
            var orderDtos = new List<OrderDto>();
            for (int i = 1; i <= count; i++)
            {
                orderDtos.Add(new OrderDto
                {
                    Id = i,
                    CustomerId = 100L + i,
                    OrderDate = new DateTime(2024, 1, 15).AddDays(i),
                    TotalAmount = 50.0 + (i * 10.0),
                    LineItems = new List<LineItemDto>
                    {
                        new LineItemDto
                        {
                            Id = i * 10L,
                            OrderId = i,
                            SkuId = 1000L + i,
                            Qty = i
                        }
                    }
                });
            }
            return orderDtos;
        }

        /// <summary>
        /// Creates a sample line item entity for testing.
        /// </summary>
        /// <returns>Sample line item entity.</returns>
        public static LineItem CreateSampleLineItem()
        {
            return new LineItem
            {
                Id = 1L,
                OrderId = 1L,
                SkuId = 100L,
                Qty = 2
            };
        }

        /// <summary>
        /// Creates a sample line item DTO for testing.
        /// </summary>
        /// <returns>Sample line item DTO.</returns>
        public static LineItemDto CreateSampleLineItemDto()
        {
            return new LineItemDto
            {
                Id = 1L,
                OrderId = 1L,
                SkuId = 100L,
                Qty = 2
            };
        }
    }
}
