using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
using MessagingLibrary.Interface;
using OrderCreatedEventNameSpace;
using Moq;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for CreateOrderCommandHandler class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class CreateOrderCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<IOrderRepository> _mockOrderRepository = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IServiceScope> _mockServiceScope = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IConverter<OrderDto, Order>> _mockOrderConverter = null!;
        private Mock<IConverter<Order, OrderDto>> _mockOrderDtoConverter = null!;
        private Mock<IMessageValidator<OrderDto>> _mockOrderDtoValidator = null!;
        private Mock<IMessagePublisher> _mockMessagePublisher = null!;
        private Mock<ILogger<CreateOrderCommandHandler>> _mockLogger = null!;
        private CreateOrderCommandHandler _handler = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockOrderConverter = new Mock<IConverter<OrderDto, Order>>();
            _mockOrderDtoConverter = new Mock<IConverter<Order, OrderDto>>();
            _mockOrderDtoValidator = new Mock<IMessageValidator<OrderDto>>();
            _mockMessagePublisher = new Mock<IMessagePublisher>();
            _mockLogger = new Mock<ILogger<CreateOrderCommandHandler>>();

            _mockUnitOfWork
                .Setup(x => x.Orders)
                .Returns(_mockOrderRepository.Object);

            _mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IUnitOfWork)))
                .Returns(_mockUnitOfWork.Object);

            // Setup default validation to pass
            _mockOrderDtoValidator
                .Setup(x => x.Validate(It.IsAny<OrderDto>()))
                .Returns(new ValidationData());

            _handler = new CreateOrderCommandHandler(
                _mockUnitOfWork.Object,
                _mockOrderConverter.Object,
                _mockOrderDtoConverter.Object,
                _mockOrderDtoValidator.Object,
                _mockMessagePublisher.Object,
                _mockServiceScopeFactory.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        [TestCategory("CreateOrderCommandHandler")]
        public void CreateOrderCommandHandler_Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _handler.Should().NotBeNull();
            _handler.Should().BeOfType<CreateOrderCommandHandler>();
        }

        [TestMethod]
        [TestCategory("CreateOrderCommandHandler")]
        public async Task Handle_WithValidOrder_ReturnsCreatedOrderDto()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
                LineItems = new List<LineItemDto>
                {
                    new LineItemDto { OrderId = 0, SkuId = 100, Qty = 2 }
                }
            };

            var order = new Order
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
            };

            var savedOrder = new Order
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
            };

            var resultDto = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
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

            _mockOrderRepository
                .Setup(x => x.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(savedOrder);

            _mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ReturnsAsync(1);

            _mockOrderRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(savedOrder);

            _mockMessagePublisher
                .Setup(x => x.PublishAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(x => x.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockOrderDtoConverter
                .Setup(x => x.Convert(savedOrder))
                .Returns(resultDto);

            var command = new CreateOrderCommand { Order = orderDto };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultDto);

            _mockOrderDtoValidator.Verify(x => x.Validate(orderDto), Times.Once);
            _mockOrderConverter.Verify(x => x.Convert(orderDto), Times.Once);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockOrderRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            _mockOrderDtoConverter.Verify(x => x.Convert(savedOrder), Times.Once);
        }

        [TestMethod]
        [TestCategory("CreateOrderCommandHandler")]
        public async Task Handle_WithInvalidOrder_ThrowsArgumentException()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                CustomerId = 0, // Invalid
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
            };

            var validationData = new ValidationData("OrderDtoValidator", "The given identifier does not have a valid value.", FailureSeverity.Error);

            _mockOrderDtoValidator
                .Setup(x => x.Validate(orderDto))
                .Returns(validationData);

            var command = new CreateOrderCommand { Order = orderDto };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
                _handler.Handle(command, CancellationToken.None));

            _mockOrderDtoValidator.Verify(x => x.Validate(orderDto), Times.Once);
            _mockOrderConverter.Verify(x => x.Convert(It.IsAny<OrderDto>()), Times.Never);
            // Note: BeginTransactionAsync is called before validation in the handler
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("CreateOrderCommandHandler")]
        public async Task Handle_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
            };

            var order = new Order
            {
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00,
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

            _mockOrderRepository
                .Setup(x => x.AddAsync(It.IsAny<Order>()))
                .ThrowsAsync(new Exception("Database error"));

            _mockUnitOfWork
                .Setup(x => x.RollbackTransactionAsync())
                .Returns(Task.CompletedTask);

            var command = new CreateOrderCommand { Order = orderDto };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _handler.Handle(command, CancellationToken.None));

            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}

