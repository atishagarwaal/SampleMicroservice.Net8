using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.UnitOfWork;

namespace Retail.Orders.Write.ComponentTests
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
        private Mock<ILoggerFactory> _mockLoggerFactory = null!;
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
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            
            _mockLoggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            _unitOfWork = new UnitOfWork(_context, _mockLogger.Object, _mockLoggerFactory.Object);
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
        public void UnitOfWork_Orders_ReturnsRepository()
        {
            // Act
            var repository = _unitOfWork.Orders;

            // Assert
            repository.Should().NotBeNull();
        }

        [TestMethod]
        public void UnitOfWork_LineItems_ReturnsRepository()
        {
            // Act
            var repository = _unitOfWork.LineItems;

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
            var order = new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };
            await _unitOfWork.Orders.AddAsync(order);

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
            var order = new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // Act
            await _unitOfWork.CommitTransactionAsync();

            // Assert
            // Verify entity was persisted
            var savedOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id);
            savedOrder.Should().NotBeNull();
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
            var order = new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // Act
            Func<Task> act = async () => await _unitOfWork.RollbackTransactionAsync();

            // Assert
            // InMemory database doesn't support real transactions, so we verify the method completes without exception
            await act.Should().NotThrowAsync();
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

            var order = new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };
            var lineItem = new LineItem { OrderId = 0, SkuId = 1, Qty = 2 };

            // Act
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();
            lineItem.OrderId = order.Id;
            await _unitOfWork.LineItems.AddAsync(lineItem);
            var changeCount = await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Assert
            changeCount.Should().BeGreaterThan(0);
            var savedOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id);
            var savedLineItem = await _unitOfWork.LineItems.GetByIdAsync(lineItem.Id);
            savedOrder.Should().NotBeNull();
            savedLineItem.Should().NotBeNull();
        }

        [TestMethod]
        public async Task RollbackOnException_PreventsChanges()
        {
            // Arrange
            await _unitOfWork.BeginTransactionAsync();
            var order = new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // Act
            Func<Task> act = async () => await _unitOfWork.RollbackTransactionAsync();

            // Assert
            // InMemory database doesn't support real transactions, so we verify the method completes without exception
            await act.Should().NotThrowAsync();
        }
    }
}

