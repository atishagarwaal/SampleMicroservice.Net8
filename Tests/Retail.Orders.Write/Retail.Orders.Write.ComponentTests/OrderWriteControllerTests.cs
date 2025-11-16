using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.Constants;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.API.Controllers;
using MediatR;
using CommonLibrary.Results;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for OrderWriteController class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class OrderWriteControllerTests
    {
        private Mock<IMediator> _mockMediator = null!;
        private Mock<IMessageValidator<OrderDto>> _mockOrderDtoValidator = null!;
        private Mock<ILogger<OrderWriteController>> _mockLogger = null!;
        private OrderWriteController _controller = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockMediator = new Mock<IMediator>();
            _mockOrderDtoValidator = new Mock<IMessageValidator<OrderDto>>();
            _mockLogger = new Mock<ILogger<OrderWriteController>>();

            // Setup default validation to pass
            _mockOrderDtoValidator
                .Setup(x => x.Validate(It.IsAny<OrderDto>()))
                .Returns(new ValidationData());

            _controller = new OrderWriteController(
                _mockMediator.Object,
                _mockOrderDtoValidator.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public void OrderWriteController_Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _controller.Should().NotBeNull();
            _controller.Should().BeOfType<OrderWriteController>();
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Post_WithValidOrderDto_ReturnsOkResult()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00
            };

            var expectedResult = new OrderDto
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00
            };

            _mockOrderDtoValidator
                .Setup(x => x.Validate(orderDto))
                .Returns(new ValidationData());

            _mockMediator
                .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), default))
                .ReturnsAsync(Result<OrderDto>.Success(expectedResult));

            // Act
            var result = await _controller.Post(orderDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Post_WithNullOrderDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Post(null!);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Post_WithInvalidOrderDto_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                CustomerId = 0, // Invalid
                OrderDate = DateTime.Now,
                TotalAmount = 150.00
            };

            var validationData = new ValidationData("OrderDtoValidator", "The given identifier does not have a valid value.", FailureSeverity.Error);

            _mockOrderDtoValidator
                .Setup(x => x.Validate(orderDto))
                .Returns(validationData);

            // Act
            var result = await _controller.Post(orderDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Post_WhenMediatorReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00
            };

            _mockOrderDtoValidator
                .Setup(x => x.Validate(orderDto))
                .Returns(new ValidationData());

            _mockMediator
                .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), default))
                .ReturnsAsync(Result<OrderDto>.Failure("Failed to create order"));

            // Act
            var result = await _controller.Post(orderDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Put_WithValidOrderDto_ReturnsOkResult()
        {
            // Arrange
            var orderId = 1L;
            var orderDto = new OrderDto
            {
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 200.00
            };

            var expectedResult = new OrderDto
            {
                Id = orderId,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 200.00
            };

            _mockOrderDtoValidator
                .Setup(x => x.Validate(orderDto))
                .Returns(new ValidationData());

            _mockMediator
                .Setup(x => x.Send(It.IsAny<UpdateOrderCommand>(), default))
                .ReturnsAsync(Result<OrderDto>.Success(expectedResult));

            // Act
            var result = await _controller.Put(orderId, orderDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Put_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 200.00
            };

            // Act
            var result = await _controller.Put(0, orderDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Put_WithNullOrderDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Put(1, null!);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Put_WithInvalidOrderDto_ReturnsBadRequest()
        {
            // Arrange
            var orderId = 1L;
            var orderDto = new OrderDto
            {
                CustomerId = 0, // Invalid
                OrderDate = DateTime.Now,
                TotalAmount = 200.00
            };

            var validationData = new ValidationData("OrderDtoValidator", "The given identifier does not have a valid value.", FailureSeverity.Error);

            _mockOrderDtoValidator
                .Setup(x => x.Validate(orderDto))
                .Returns(validationData);

            // Act
            var result = await _controller.Put(orderId, orderDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Delete_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var orderId = 1L;

            _mockMediator
                .Setup(x => x.Send(It.IsAny<DeleteOrderCommand>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(true);
        }

        [TestMethod]
        [TestCategory("OrderWriteController")]
        public async Task Delete_WithInvalidId_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
        }
    }
}

