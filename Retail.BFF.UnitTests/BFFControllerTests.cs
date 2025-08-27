using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.BFFWeb.Api.Controller;
using Retail.BFFWeb.Api.Interface;
using Retail.BFFWeb.Api.Model;

namespace Retail.BFF.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class BFFControllerTests
    {
        private Mock<ICustomerProvider> _mockCustomerProvider = null!;
        private Mock<IOrderProvider> _mockOrderProvider = null!;
        private Mock<IProductProvider> _mockProductProvider = null!;
        private BFFController _bffController = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mockOrderProvider = new Mock<IOrderProvider>();
            _mockProductProvider = new Mock<IProductProvider>();

            _bffController = new BFFController(
                _mockCustomerProvider.Object,
                _mockOrderProvider.Object,
                _mockProductProvider.Object);
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public void BFFController_Constructor_CreatesInstance()
        {
            // Act & Assert
            _bffController.Should().NotBeNull();
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public void BFFController_Constructor_WithNullCustomerProvider_CreatesInstance()
        {
            // Act & Assert
            Action act = () => new BFFController(null!, _mockOrderProvider.Object, _mockProductProvider.Object);
            act.Should().NotThrow();
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public void BFFController_Constructor_WithNullOrderProvider_CreatesInstance()
        {
            // Act & Assert
            Action act = () => new BFFController(_mockCustomerProvider.Object, null!, _mockProductProvider.Object);
            act.Should().NotThrow();
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public void BFFController_Constructor_WithNullProductProvider_CreatesInstance()
        {
            // Act & Assert
            Action act = () => new BFFController(_mockCustomerProvider.Object, _mockOrderProvider.Object, null!);
            act.Should().NotThrow();
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public async Task GetAllOrdersDetails_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>
                    {
                        new LineItemDto { Id = 1, OrderId = 1, SkuId = 1, Qty = 2 }
                    }
                }
            };

            var customers = new List<CustomerDto>
            {
                new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" }
            };

            var products = new List<SkuDto>
            {
                new SkuDto { Id = 1, Name = "Product 1", UnitPrice = 29.99 }
            };

            _mockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(orders);
            _mockCustomerProvider.Setup(x => x.GetCustomerByIdAsync(1)).ReturnsAsync(customers[0]);
            _mockProductProvider.Setup(x => x.GetProductByIdAsync(1)).ReturnsAsync(products[0]);

            // Act
            var result = await _bffController.GetAllOrdersDetails();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public async Task GetAllOrdersDetails_WithEmptyOrders_ReturnsOkResultWithEmptyList()
        {
            // Arrange
            var orders = new List<OrderDto>();
            _mockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(orders);

            // Act
            var result = await _bffController.GetAllOrdersDetails();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public async Task GetAllOrdersDetails_WithNullCustomer_HandlesGracefully()
        {
            // Arrange
            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>
                    {
                        new LineItemDto { Id = 1, OrderId = 1, SkuId = 1, Qty = 2 }
                    }
                }
            };

            _mockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(orders);
            _mockCustomerProvider.Setup(x => x.GetCustomerByIdAsync(1)).ReturnsAsync((CustomerDto?)null);
            _mockProductProvider.Setup(x => x.GetProductByIdAsync(1)).ReturnsAsync((SkuDto?)null);

            // Act
            var result = await _bffController.GetAllOrdersDetails();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public async Task GetAllOrdersDetails_WithNullProduct_HandlesGracefully()
        {
            // Arrange
            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>
                    {
                        new LineItemDto { Id = 1, OrderId = 1, SkuId = 1, Qty = 2 }
                    }
                }
            };

            var customers = new List<CustomerDto>
            {
                new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" }
            };

            _mockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(orders);
            _mockCustomerProvider.Setup(x => x.GetCustomerByIdAsync(1)).ReturnsAsync(customers[0]);
            _mockProductProvider.Setup(x => x.GetProductByIdAsync(1)).ReturnsAsync((SkuDto?)null);

            // Act
            var result = await _bffController.GetAllOrdersDetails();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public async Task GetAllOrdersDetails_WithOrderProviderException_ReturnsInternalServerError()
        {
            // Arrange
            _mockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _bffController.GetAllOrdersDetails();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public async Task GetAllOrdersDetails_WithCustomerProviderException_ReturnsInternalServerError()
        {
            // Arrange
            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>()
                }
            };

            _mockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(orders);
            _mockCustomerProvider.Setup(x => x.GetCustomerByIdAsync(1)).ThrowsAsync(new Exception("Customer service error"));

            // Act
            var result = await _bffController.GetAllOrdersDetails();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [TestMethod]
        [TestCategory("BFFController")]
        public async Task GetAllOrdersDetails_WithProductProviderException_ReturnsInternalServerError()
        {
            // Arrange
            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>
                    {
                        new LineItemDto { Id = 1, OrderId = 1, SkuId = 1, Qty = 2 }
                    }
                }
            };

            var customers = new List<CustomerDto>
            {
                new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" }
            };

            _mockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(orders);
            _mockCustomerProvider.Setup(x => x.GetCustomerByIdAsync(1)).ReturnsAsync(customers[0]);
            _mockProductProvider.Setup(x => x.GetProductByIdAsync(1)).ThrowsAsync(new Exception("Product service error"));

            // Act
            var result = await _bffController.GetAllOrdersDetails();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }
    }
}
