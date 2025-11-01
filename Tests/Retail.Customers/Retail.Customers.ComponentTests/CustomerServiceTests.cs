using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommonLibrary.MessageContract;
using FluentAssertions;
using InventoryUpdatedEventNameSpace;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Service;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for CustomerService class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class CustomerServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<IMapper> _mockMapper = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IServiceScope> _mockServiceScope = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private CustomerService _customerService = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            _mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            _customerService = new CustomerService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockServiceScopeFactory.Object);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public void CustomerService_Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _customerService.Should().NotBeNull();
            _customerService.Should().BeOfType<CustomerService>();
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task GetAllCustomersAsync_WithValidData_ReturnsMappedCustomers()
        {
            // Arrange
            var customers = new List<Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer>
            {
                new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { Id = 1, FirstName = "John", LastName = "Doe" },
                new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { Id = 2, FirstName = "Jane", LastName = "Smith" }
            };

            var customerDtos = new List<CustomerDto>
            {
                new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" },
                new CustomerDto { Id = 2, FirstName = "Jane", LastName = "Smith" }
            };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetAllAsync())
                .ReturnsAsync(customers);

            _mockMapper
                .Setup(x => x.Map<IEnumerable<CustomerDto>>(customers))
                .Returns(customerDtos);

            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(customerDtos);

            _mockUnitOfWork.Verify(x => x.Customers.GetAllAsync(), Times.Once);
            _mockMapper.Verify(x => x.Map<IEnumerable<CustomerDto>>(customers), Times.Once);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task GetCustomerByIdAsync_WithValidId_ReturnsMappedCustomer()
        {
            // Arrange
            var customerId = 1L;
            var customer = new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { Id = customerId, FirstName = "John", LastName = "Doe" };
            var customerDto = new CustomerDto { Id = customerId, FirstName = "John", LastName = "Doe" };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _mockMapper
                .Setup(x => x.Map<CustomerDto>(customer))
                .Returns(customerDto);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(customerDto);

            _mockUnitOfWork.Verify(x => x.Customers.GetByIdAsync(customerId), Times.Once);
            _mockMapper.Verify(x => x.Map<CustomerDto>(customer), Times.Once);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task GetCustomerByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var customerId = 999L;

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync((Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer?)null);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().BeNull();

            _mockUnitOfWork.Verify(x => x.Customers.GetByIdAsync(customerId), Times.Once);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task AddCustomerAsync_WithValidCustomer_ReturnsMappedCustomer()
        {
            // Arrange
            var customerDto = new CustomerDto { FirstName = "John", LastName = "Doe" };
            var customer = new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { FirstName = "John", LastName = "Doe" };
            var addedCustomer = new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { Id = 1, FirstName = "John", LastName = "Doe" };
            var resultDto = new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" };

            _mockMapper
                .Setup(x => x.Map<Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer>(customerDto))
                .Returns(customer);

            _mockUnitOfWork
                .Setup(x => x.Customers.AddAsync(customer))
                .ReturnsAsync(addedCustomer);

            _mockMapper
                .Setup(x => x.Map<CustomerDto>(addedCustomer))
                .Returns(resultDto);

            // Act
            var result = await _customerService.AddCustomerAsync(customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultDto);

            _mockMapper.Verify(x => x.Map<Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer>(customerDto), Times.Once);
            _mockUnitOfWork.Verify(x => x.Customers.AddAsync(customer), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task AddCustomerAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var customerDto = new CustomerDto { FirstName = "John", LastName = "Doe" };
            var customer = new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { FirstName = "John", LastName = "Doe" };

            _mockMapper
                .Setup(x => x.Map<Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer>(customerDto))
                .Returns(customer);

            _mockUnitOfWork
                .Setup(x => x.Customers.AddAsync(customer))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _customerService.AddCustomerAsync(customerDto));

            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task UpdateCustomerAsync_WithValidCustomer_ReturnsUpdatedCustomer()
        {
            // Arrange
            var customerId = 1L;
            var existingCustomer = new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { Id = customerId, FirstName = "John", LastName = "Doe" };
            var customerDto = new CustomerDto { Id = customerId, FirstName = "John", LastName = "Updated" };
            var resultDto = new CustomerDto { Id = customerId, FirstName = "John", LastName = "Updated" };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            _mockMapper
                .Setup(x => x.Map(customerDto, existingCustomer))
                .Verifiable();

            _mockMapper
                .Setup(x => x.Map<CustomerDto>(existingCustomer))
                .Returns(resultDto);

            // Act
            var result = await _customerService.UpdateCustomerAsync(customerId, customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultDto);

            _mockUnitOfWork.Verify(x => x.Customers.GetByIdAsync(customerId), Times.Once);
            _mockUnitOfWork.Verify(x => x.Customers.Update(existingCustomer), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            _mockMapper.Verify();
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task UpdateCustomerAsync_WithInvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var customerId = 999L;
            var customerDto = new CustomerDto { FirstName = "John", LastName = "Doe" };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync((Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => 
                _customerService.UpdateCustomerAsync(customerId, customerDto));

            _mockUnitOfWork.Verify(x => x.Customers.GetByIdAsync(customerId), Times.Once);
            _mockUnitOfWork.Verify(x => x.Customers.Update(It.IsAny<Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task UpdateCustomerAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var customerId = 1L;
            var existingCustomer = new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { Id = customerId, FirstName = "John", LastName = "Doe" };
            var customerDto = new CustomerDto { Id = customerId, FirstName = "John", LastName = "Updated" };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            _mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _customerService.UpdateCustomerAsync(customerId, customerDto));

            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task DeleteCustomerAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var customerId = 1L;
            var customer = new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { Id = customerId, FirstName = "John", LastName = "Doe" };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _customerService.DeleteCustomerAsync(customerId);

            // Assert
            result.Should().BeTrue();

            _mockUnitOfWork.Verify(x => x.Customers.GetByIdAsync(customerId), Times.Once);
            _mockUnitOfWork.Verify(x => x.Customers.Remove(customer), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task DeleteCustomerAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var customerId = 999L;

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync((Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer?)null);

            // Act
            var result = await _customerService.DeleteCustomerAsync(customerId);

            // Assert
            result.Should().BeFalse();

            _mockUnitOfWork.Verify(x => x.Customers.GetByIdAsync(customerId), Times.Once);
            _mockUnitOfWork.Verify(x => x.Customers.Remove(It.IsAny<Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task DeleteCustomerAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var customerId = 1L;
            var customer = new Retail.Api.Customers.src.CleanArchitecture.Domain.Entities.Customer { Id = customerId, FirstName = "John", LastName = "Doe" };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _customerService.DeleteCustomerAsync(customerId));

            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task HandleOrderCreatedEvent_WithValidEvent_CreatesNotification()
        {
            // Arrange
            var inventoryEvent = new InventoryUpdatedEvent
            {
                OrderId = 123,
                CustomerId = 456
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            
            // Setup the mock unit of work methods
            mockUnitOfWork
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);
            
            mockUnitOfWork
                .Setup(x => x.Notifications.AddAsync(It.IsAny<Notification>()))
                .ReturnsAsync(new Notification());
            
            mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ReturnsAsync(1);
            
            mockUnitOfWork
                .Setup(x => x.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IUnitOfWork)))
                .Returns(mockUnitOfWork.Object);

            // Act
            await _customerService.HandleOrderCreatedEvent(inventoryEvent);

            // Assert
            mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            mockUnitOfWork.Verify(x => x.Notifications.AddAsync(It.Is<Notification>(n => 
                n.OrderId == inventoryEvent.OrderId && 
                n.CustomerId == inventoryEvent.CustomerId)), Times.Once);
            mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task HandleOrderCreatedEvent_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var inventoryEvent = new InventoryUpdatedEvent
            {
                OrderId = 123,
                CustomerId = 456
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            
            // Setup the mock unit of work methods
            mockUnitOfWork
                .Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);
            
            mockUnitOfWork
                .Setup(x => x.Notifications.AddAsync(It.IsAny<Notification>()))
                .ReturnsAsync(new Notification());
            
            mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ThrowsAsync(new Exception("Database error"));
            
            mockUnitOfWork
                .Setup(x => x.RollbackTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IUnitOfWork)))
                .Returns(mockUnitOfWork.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _customerService.HandleOrderCreatedEvent(inventoryEvent));

            mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}
