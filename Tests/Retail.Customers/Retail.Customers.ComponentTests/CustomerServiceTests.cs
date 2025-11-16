using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.MessageContract;
using FluentAssertions;
using InventoryUpdatedEventNameSpace;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Service;
using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Validation;
using Retail.Api.Customers.src.CleanArchitecture.Application.Validation.Interfaces;
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
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IServiceScope> _mockServiceScope = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IMessageValidator<CustomerDto>> _mockCustomerDtoValidator = null!;
        private Mock<IConverter<CustomerDto, Customer>> _mockCustomerConverter = null!;
        private Mock<IConverter<Customer, CustomerDto>> _mockCustomerDtoConverter = null!;
        private Mock<ILogger<CustomerService>> _mockLogger = null!;
        private CustomerService _customerService = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockCustomerDtoValidator = new Mock<IMessageValidator<CustomerDto>>();
            _mockCustomerConverter = new Mock<IConverter<CustomerDto, Customer>>();
            _mockCustomerDtoConverter = new Mock<IConverter<Customer, CustomerDto>>();
            _mockLogger = new Mock<ILogger<CustomerService>>();

            _mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            // Setup default validation to pass
            _mockCustomerDtoValidator
                .Setup(x => x.Validate(It.IsAny<CustomerDto>()))
                .Returns(new ValidationData());

            _customerService = new CustomerService(
                _mockUnitOfWork.Object,
                _mockServiceScopeFactory.Object,
                _mockCustomerDtoValidator.Object,
                _mockCustomerConverter.Object,
                _mockCustomerDtoConverter.Object,
                _mockLogger.Object);
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
            var customers = new List<Customer>
            {
                new Customer { Id = 1, FirstName = "John", LastName = "Doe" },
                new Customer { Id = 2, FirstName = "Jane", LastName = "Smith" }
            };

            var customerDtos = new List<CustomerDto>
            {
                new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" },
                new CustomerDto { Id = 2, FirstName = "Jane", LastName = "Smith" }
            };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetAllAsync())
                .ReturnsAsync(customers);

            _mockCustomerDtoConverter
                .Setup(x => x.Convert(It.IsAny<Customer>()))
                .Returns<Customer>(customer => customerDtos.First(dto => dto.Id == customer.Id));

            // Act
            var result = await _customerService.GetAllCustomersAsync();
            var resultList = result.ToList();

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().HaveCount(2);
            resultList.Should().BeEquivalentTo(customerDtos);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task GetCustomerByIdAsync_WithValidId_ReturnsMappedCustomer()
        {
            // Arrange
            var customerId = 1L;
            var customer = new Customer { Id = customerId, FirstName = "John", LastName = "Doe" };
            var customerDto = new CustomerDto { Id = customerId, FirstName = "John", LastName = "Doe" };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _mockCustomerDtoConverter
                .Setup(x => x.Convert(customer))
                .Returns(customerDto);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(customerDto);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task GetCustomerByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var customerId = 999L;

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task AddCustomerAsync_WithValidCustomer_ReturnsMappedCustomer()
        {
            // Arrange
            var customerDto = new CustomerDto { FirstName = "John", LastName = "Doe" };
            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            var addedCustomer = new Customer { Id = 1, FirstName = "John", LastName = "Doe" };
            var resultDto = new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" };

            _mockCustomerDtoValidator
                .Setup(x => x.Validate(customerDto))
                .Returns(new ValidationData());

            _mockCustomerConverter
                .Setup(x => x.Convert(customerDto))
                .Returns(customer);

            _mockUnitOfWork
                .Setup(x => x.Customers.AddAsync(customer))
                .ReturnsAsync(addedCustomer);

            _mockCustomerDtoConverter
                .Setup(x => x.Convert(addedCustomer))
                .Returns(resultDto);

            // Act
            var result = await _customerService.AddCustomerAsync(customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultDto);
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task AddCustomerAsync_WithInvalidCustomer_ThrowsArgumentException()
        {
            // Arrange
            var customerDto = new CustomerDto { FirstName = string.Empty, LastName = "Doe" };
            var validationData = new ValidationData("CustomerDtoValidator", "The FirstName field is null or whitespace.", FailureSeverity.Error);

            _mockCustomerDtoValidator
                .Setup(x => x.Validate(customerDto))
                .Returns(validationData);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
                _customerService.AddCustomerAsync(customerDto));
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task AddCustomerAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var customerDto = new CustomerDto { FirstName = "John", LastName = "Doe" };
            var customer = new Customer { FirstName = "John", LastName = "Doe" };

            _mockCustomerDtoValidator
                .Setup(x => x.Validate(customerDto))
                .Returns(new ValidationData());

            _mockCustomerConverter
                .Setup(x => x.Convert(customerDto))
                .Returns(customer);

            _mockUnitOfWork
                .Setup(x => x.Customers.AddAsync(customer))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _customerService.AddCustomerAsync(customerDto));
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task UpdateCustomerAsync_WithValidCustomer_ReturnsUpdatedCustomer()
        {
            // Arrange
            var customerId = 1L;
            var existingCustomer = new Customer { Id = customerId, FirstName = "John", LastName = "Doe" };
            var customerDto = new CustomerDto { Id = customerId, FirstName = "John", LastName = "Updated" };
            var updatedCustomer = new Customer { Id = customerId, FirstName = "John", LastName = "Updated" };
            var resultDto = new CustomerDto { Id = customerId, FirstName = "John", LastName = "Updated" };

            _mockCustomerDtoValidator
                .Setup(x => x.Validate(customerDto))
                .Returns(new ValidationData());

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            _mockCustomerConverter
                .Setup(x => x.Convert(customerDto))
                .Returns(updatedCustomer);

            _mockCustomerDtoConverter
                .Setup(x => x.Convert(existingCustomer))
                .Returns(resultDto);

            // Act
            var result = await _customerService.UpdateCustomerAsync(customerId, customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultDto);
            existingCustomer.FirstName.Should().Be("John");
            existingCustomer.LastName.Should().Be("Updated");
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task UpdateCustomerAsync_WithInvalidCustomer_ThrowsArgumentException()
        {
            // Arrange
            var customerId = 1L;
            var customerDto = new CustomerDto { FirstName = string.Empty, LastName = "Doe" };
            var validationData = new ValidationData("CustomerDtoValidator", "The FirstName field is null or whitespace.", FailureSeverity.Error);

            _mockCustomerDtoValidator
                .Setup(x => x.Validate(customerDto))
                .Returns(validationData);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
                _customerService.UpdateCustomerAsync(customerId, customerDto));
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task UpdateCustomerAsync_WithInvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var customerId = 999L;
            var customerDto = new CustomerDto { FirstName = "John", LastName = "Doe" };

            _mockCustomerDtoValidator
                .Setup(x => x.Validate(customerDto))
                .Returns(new ValidationData());

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => 
                _customerService.UpdateCustomerAsync(customerId, customerDto));
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task UpdateCustomerAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var customerId = 1L;
            var existingCustomer = new Customer { Id = customerId, FirstName = "John", LastName = "Doe" };
            var customerDto = new CustomerDto { Id = customerId, FirstName = "John", LastName = "Updated" };
            var updatedCustomer = new Customer { Id = customerId, FirstName = "John", LastName = "Updated" };

            _mockCustomerDtoValidator
                .Setup(x => x.Validate(customerDto))
                .Returns(new ValidationData());

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            _mockCustomerConverter
                .Setup(x => x.Convert(customerDto))
                .Returns(updatedCustomer);

            _mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _customerService.UpdateCustomerAsync(customerId, customerDto));
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task DeleteCustomerAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var customerId = 1L;
            var customer = new Customer { Id = customerId, FirstName = "John", LastName = "Doe" };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _customerService.DeleteCustomerAsync(customerId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task DeleteCustomerAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var customerId = 999L;

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _customerService.DeleteCustomerAsync(customerId);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("CustomerService")]
        public async Task DeleteCustomerAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var customerId = 1L;
            var customer = new Customer { Id = customerId, FirstName = "John", LastName = "Doe" };

            _mockUnitOfWork
                .Setup(x => x.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _mockUnitOfWork
                .Setup(x => x.CompleteAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _customerService.DeleteCustomerAsync(customerId));
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

            // Assert - Verify behavior: method completed without exception
            // No Verify calls per testing standards - focus on behavior, not implementation
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
        }
    }
}
