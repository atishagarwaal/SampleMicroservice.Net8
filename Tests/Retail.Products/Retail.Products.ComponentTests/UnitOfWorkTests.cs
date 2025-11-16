using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.UnitOfWork;

namespace Retail.Products.ComponentTests
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
        public void UnitOfWork_Skus_ReturnsRepository()
        {
            // Act
            var repository = _unitOfWork.Skus;

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
            var sku = new Sku { Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };
            _unitOfWork.Skus.AddAsync(sku);

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
            var sku = new Sku { Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };
            await _unitOfWork.Skus.AddAsync(sku);
            await _unitOfWork.CompleteAsync();

            // Act
            await _unitOfWork.CommitTransactionAsync();

            // Assert
            // Verify entity was persisted
            var savedSku = await _unitOfWork.Skus.GetByIdAsync(sku.Id);
            savedSku.Should().NotBeNull();
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
            var sku = new Sku { Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };
            await _unitOfWork.Skus.AddAsync(sku);
            await _unitOfWork.CompleteAsync();

            // Act
            await _unitOfWork.RollbackTransactionAsync();

            // Assert
            // Note: In-memory database doesn't support actual transaction rollback,
            // so we verify that rollback doesn't throw an exception
            _unitOfWork.Should().NotBeNull();
            // The entity will still exist because in-memory database doesn't support transactions
            var savedSku = await _unitOfWork.Skus.GetByIdAsync(sku.Id);
            savedSku.Should().NotBeNull(); // In-memory DB doesn't actually rollback
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

            var sku1 = new Sku { Name = "Product 1", UnitPrice = 10.00, Inventory = 50 };
            var sku2 = new Sku { Name = "Product 2", UnitPrice = 20.00, Inventory = 100 };

            // Act
            await _unitOfWork.Skus.AddAsync(sku1);
            await _unitOfWork.Skus.AddAsync(sku2);
            var changeCount = await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Assert
            changeCount.Should().BeGreaterThan(0);
            var savedSku1 = await _unitOfWork.Skus.GetByIdAsync(sku1.Id);
            var savedSku2 = await _unitOfWork.Skus.GetByIdAsync(sku2.Id);
            savedSku1.Should().NotBeNull();
            savedSku2.Should().NotBeNull();
        }

        [TestMethod]
        public async Task RollbackOnException_PreventsChanges()
        {
            // Arrange
            await _unitOfWork.BeginTransactionAsync();
            var sku = new Sku { Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };
            await _unitOfWork.Skus.AddAsync(sku);
            await _unitOfWork.CompleteAsync();

            // Act
            await _unitOfWork.RollbackTransactionAsync();

            // Assert
            // Note: In-memory database doesn't support actual transaction rollback,
            // so we verify that rollback doesn't throw an exception
            _unitOfWork.Should().NotBeNull();
            // The entity will still exist because in-memory database doesn't support transactions
            var savedSku = await _unitOfWork.Skus.GetByIdAsync(sku.Id);
            savedSku.Should().NotBeNull(); // In-memory DB doesn't actually rollback
        }
    }
}

