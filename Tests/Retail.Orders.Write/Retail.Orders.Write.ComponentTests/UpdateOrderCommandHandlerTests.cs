using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Application.Handlers;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Moq;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for UpdateOrderCommandHandler class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class UpdateOrderCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<IOrderRepository> _mockOrderRepository = null!;
        private Mock<ILineItemRepository> _mockLineItemRepository = null!;
        private Mock<IConverter<OrderDto, Order>> _mockOrderConverter = null!;
        private Mock<IConverter<Order, OrderDto>> _mockOrderDtoConverter = null!;
        private Mock<IMessageValidator<OrderDto>> _mockOrderDtoValidator = null!;
        private UpdateOrderCommandHandler _handler = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockLineItemRepository = new Mock<ILineItemRepository>();
            _mockOrderConverter = new Mock<IConverter<OrderDto, Order>>();
            _mockOrderDtoConverter = new Mock<IConverter<Order, OrderDto>>();
            _mockOrderDtoValidator = new Mock<IMessageValidator<OrderDto>>();

            _mockUnitOfWork
                .Setup(x => x.Orders)
                .Returns(_mockOrderRepository.Object);

            _mockUnitOfWork
                .Setup(x => x.LineItems)
                .Returns(_mockLineItemRepository.Object);

            // Setup default validation to pass
            _mockOrderDtoValidator
                .Setup(x => x.Validate(It.IsAny<OrderDto>()))
                .Returns(new ValidationData());

            _handler = new UpdateOrderCommandHandler(
                _mockUnitOfWork.Object,
                _mockOrderConverter.Object,
                _mockOrderDtoConverter.Object,
                _mockOrderDtoValidator.Object);
        }

        [TestMethod]
        [TestCategory("UpdateOrderCommandHandler")]
        public void UpdateOrderCommandHandler_Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _handler.Should().NotBeNull();
            _handler.Should().BeOfType<UpdateOrderCommandHandler>();
        }

        [TestMethod]
        [TestCategory("UpdateOrderCommandHandler")]
        public async Task Handle_WithValidOrder_ReturnsUpdatedOrderDto()
        {
            // Arrange
            var orderId = 1L;
            var orderDto = new OrderDto
            {
                Id = orderId,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 200.00,
                LineItems = new List<LineItemDto>
                {
                    new LineItemDto { Id = 1, OrderId = orderId, SkuId = 100, Qty = 3 }
                }
            };

            var order = new Order
            {
                Id = orderId,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 200.00,
            };

            var updatedOrder = new Order
            {
                Id = orderId,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 200.00,
            };

            var resultDto = new OrderDto
            {
                Id = orderId,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 200.00,
            };

            _mockOrderDtoValidator
                .Setup(x => x.Validate(orderDto))
                .Returns(new ValidationData());

            _mockOrderConverter
                .Setup(x => x.Convert(orderDto))
                .Returns(order);

            _mockUnitOfWork
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ReturnsAsync(1);

            _mockUnitOfWork
                .Setup(x => x.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(x => x.Orders.GetByIdAsync(orderId))
                .ReturnsAsync(updatedOrder);

            _mockOrderDtoConverter
                .Setup(x => x.Convert(updatedOrder))
                .Returns(resultDto);

            var command = new UpdateOrderCommand { Order = orderDto };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultDto);

            _mockOrderDtoValidator.Verify(x => x.Validate(orderDto), Times.Once);
            _mockOrderConverter.Verify(x => x.Convert(orderDto), Times.Once);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockOrderRepository.Verify(x => x.Update(It.IsAny<Order>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            _mockOrderDtoConverter.Verify(x => x.Convert(updatedOrder), Times.Once);
        }

        [TestMethod]
        [TestCategory("UpdateOrderCommandHandler")]
        public async Task Handle_WithInvalidOrder_ThrowsArgumentException()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 0, // Invalid
                OrderDate = DateTime.Now,
                TotalAmount = 200.00,
            };

            var validationData = new ValidationData("OrderDtoValidator", "The given identifier does not have a valid value.", FailureSeverity.Error);

            _mockOrderDtoValidator
                .Setup(x => x.Validate(orderDto))
                .Returns(validationData);

            var command = new UpdateOrderCommand { Order = orderDto };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
                _handler.Handle(command, CancellationToken.None));

            _mockOrderDtoValidator.Verify(x => x.Validate(orderDto), Times.Once);
            _mockOrderConverter.Verify(x => x.Convert(It.IsAny<OrderDto>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
        }

        [TestMethod]
        [TestCategory("UpdateOrderCommandHandler")]
        public async Task Handle_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 200.00,
            };

            var order = new Order
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 200.00,
            };

            _mockOrderDtoValidator
                .Setup(x => x.Validate(orderDto))
                .Returns(new ValidationData());

            _mockOrderConverter
                .Setup(x => x.Convert(orderDto))
                .Returns(order);

            _mockUnitOfWork
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ThrowsAsync(new Exception("Database error"));

            _mockUnitOfWork
                .Setup(x => x.RollbackTransactionAsync())
                .Returns(Task.CompletedTask);

            var command = new UpdateOrderCommand { Order = orderDto };

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<Exception>(() => 
                _handler.Handle(command, CancellationToken.None));

            exception.Should().NotBeNull();
            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}

