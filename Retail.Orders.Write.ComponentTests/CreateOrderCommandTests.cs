using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for CreateOrderCommand class.
    /// </summary>
    [TestClass]
    public class CreateOrderCommandTests
    {
        [TestMethod]
        public void CreateOrderCommand_WithValidOrder_ShouldBeValid()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99,
                LineItems = new List<LineItemDto>
                {
                    new LineItemDto { Id = 1, OrderId = 1, SkuId = 100, Qty = 2 }
                }
            };

            // Act
            var command = new CreateOrderCommand { Order = orderDto };

            // Assert
            command.Should().NotBeNull();
            command.Order.Should().Be(orderDto);
            command.Order.Id.Should().Be(1);
            command.Order.CustomerId.Should().Be(123);
        }

        [TestMethod]
        public void CreateOrderCommand_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var expectedOrder = new OrderDto
            {
                Id = 1,
                CustomerId = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99
            };

            // Act
            var command = new CreateOrderCommand { Order = expectedOrder };

            // Assert
            command.Order.Should().Be(expectedOrder);
            command.Order.Id.Should().Be(expectedOrder.Id);
            command.Order.CustomerId.Should().Be(expectedOrder.CustomerId);
            command.Order.OrderDate.Should().Be(expectedOrder.OrderDate);
            command.Order.TotalAmount.Should().Be(expectedOrder.TotalAmount);
        }

        [TestMethod]
        public void CreateOrderCommand_WithNullOrder_ShouldBeValid()
        {
            // Arrange & Act
            var command = new CreateOrderCommand { Order = null };

            // Assert
            command.Should().NotBeNull();
            command.Order.Should().BeNull();
        }

        [TestMethod]
        public void CreateOrderCommand_WithEmptyOrder_ShouldBeValid()
        {
            // Arrange
            var emptyOrder = new OrderDto();

            // Act
            var command = new CreateOrderCommand { Order = emptyOrder };

            // Assert
            command.Should().NotBeNull();
            command.Order.Should().Be(emptyOrder);
        }

        [TestMethod]
        public void CreateOrderCommand_OrderProperty_ShouldBeMutable()
        {
            // Arrange
            var command = new CreateOrderCommand();
            var order1 = new OrderDto { Id = 1, CustomerId = 123 };
            var order2 = new OrderDto { Id = 2, CustomerId = 456 };

            // Act
            command.Order = order1;
            command.Order.Should().Be(order1);

            command.Order = order2;
            command.Order.Should().Be(order2);
        }

        [TestMethod]
        public void CreateOrderCommand_ShouldImplementIRequest()
        {
            // Arrange & Act
            var command = new CreateOrderCommand();

            // Assert
            command.Should().BeAssignableTo<MediatR.IRequest<OrderDto>>();
        }
    }
}
