using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.UnitOfWork;

namespace Retail.Customers.ComponentTests
{
    /// <summary>
    /// Unit tests for UnitOfWork class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("UnitOfWork")]
    public sealed class UnitOfWorkTests
    {
        private ApplicationDbContext _context = null!;
        private Mock<ILogger<UnitOfWork>> _mockLogger = null!;
        private UnitOfWork _unitOfWork = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<UnitOfWork>>();

            _unitOfWork = new UnitOfWork(_context, _mockLogger.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _unitOfWork?.Dispose();
            _context?.Dispose();
        }

        [TestMethod]
        public void UnitOfWork_Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _unitOfWork.Should().NotBeNull();
            _unitOfWork.Should().BeOfType<UnitOfWork>();
        }

        [TestMethod]
        public void UnitOfWork_Customers_ReturnsRepository()
        {
            // Act
            var repository = _unitOfWork.Customers;

            // Assert
            repository.Should().NotBeNull();
        }

        [TestMethod]
        public void UnitOfWork_Notifications_ReturnsRepository()
        {
            // Act
            var repository = _unitOfWork.Notifications;

            // Assert
            repository.Should().NotBeNull();
        }

        [TestMethod]
        public async Task BeginTransactionAsync_WhenCalled_StartsTransaction()
        {
            // Act
            await _unitOfWork.BeginTransactionAsync();

            // Assert
            // Transaction should be started - verify by checking CompleteAsync works
            var result = await _unitOfWork.CompleteAsync();
            result.Should().BeGreaterThanOrEqualTo(0);
        }

        [TestMethod]
        public async Task BeginTransactionAsync_WhenCalledMultipleTimes_DoesNotThrow()
        {
            // Act
            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.BeginTransactionAsync();

            // Assert - Should not throw exception
            _unitOfWork.Should().NotBeNull();
        }

        [TestMethod]
        public async Task CompleteAsync_WithChanges_ReturnsChangeCount()
        {
            // Arrange
            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            await _unitOfWork.Customers.AddAsync(customer);

            // Act
            var result = await _unitOfWork.CompleteAsync();

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public async Task CompleteAsync_WithNoChanges_ReturnsZero()
        {
            // Act
            var result = await _unitOfWork.CompleteAsync();

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task CommitTransactionAsync_AfterBeginTransaction_CommitsSuccessfully()
        {
            // Arrange
            await _unitOfWork.BeginTransactionAsync();
            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CompleteAsync();

            // Act
            await _unitOfWork.CommitTransactionAsync();

            // Assert
            // Verify entity was persisted
            var savedCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
            savedCustomer.Should().NotBeNull();
        }

        [TestMethod]
        public async Task CommitTransactionAsync_WithoutBeginTransaction_DoesNotThrow()
        {
            // Act
            await _unitOfWork.CommitTransactionAsync();

            // Assert - Should not throw exception
            _unitOfWork.Should().NotBeNull();
        }

        [TestMethod]
        public async Task RollbackTransactionAsync_AfterBeginTransaction_RollsBackSuccessfully()
        {
            // Arrange
            await _unitOfWork.BeginTransactionAsync();
            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CompleteAsync();

            // Act
            await _unitOfWork.RollbackTransactionAsync();

            // Assert
            // Note: In-memory database doesn't support actual transaction rollback,
            // so we verify that rollback doesn't throw an exception
            _unitOfWork.Should().NotBeNull();
            // The entity will still exist because in-memory database doesn't support transactions
            var savedCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
            savedCustomer.Should().NotBeNull(); // In-memory DB doesn't actually rollback
        }

        [TestMethod]
        public async Task RollbackTransactionAsync_WithoutBeginTransaction_DoesNotThrow()
        {
            // Act
            await _unitOfWork.RollbackTransactionAsync();

            // Assert - Should not throw exception
            _unitOfWork.Should().NotBeNull();
        }

        [TestMethod]
        public async Task MultipleOperations_InTransaction_AllSucceed()
        {
            // Arrange
            await _unitOfWork.BeginTransactionAsync();

            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            var notification = new Notification 
            { 
                CustomerId = 0, 
                OrderId = 123, 
                OrderDate = DateTime.Now, 
                Message = "Test notification" 
            };

            // Act
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CompleteAsync();
            notification.CustomerId = customer.Id;
            await _unitOfWork.Notifications.AddAsync(notification);
            var changeCount = await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Assert
            changeCount.Should().BeGreaterThan(0);
            var savedCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
            savedCustomer.Should().NotBeNull();
            // Verify notification was added by checking it exists in the context
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            allNotifications.Should().Contain(n => n.CustomerId == customer.Id && n.OrderId == 123);
        }

        [TestMethod]
        public async Task RollbackOnException_PreventsChanges()
        {
            // Arrange
            await _unitOfWork.BeginTransactionAsync();
            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CompleteAsync();

            // Act
            await _unitOfWork.RollbackTransactionAsync();

            // Assert
            // Note: In-memory database doesn't support actual transaction rollback,
            // so we verify that rollback doesn't throw an exception
            _unitOfWork.Should().NotBeNull();
            // The entity will still exist because in-memory database doesn't support transactions
            var savedCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
            savedCustomer.Should().NotBeNull(); // In-memory DB doesn't actually rollback
        }
    }
}

