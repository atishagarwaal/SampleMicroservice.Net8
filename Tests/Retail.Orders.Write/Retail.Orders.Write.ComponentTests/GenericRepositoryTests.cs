using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Repositories;

namespace Retail.Orders.Write.ComponentTests
{
    /// <summary>
    /// Unit tests for GenericRepository class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("GenericRepository")]
    public sealed class GenericRepositoryTests
    {
        private ApplicationDbContext _context = null!;
        private GenericRepository<Order> _repository = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new GenericRepository<Order>(_context);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task AddAsync_WithValidEntity_ReturnsEntity()
        {
            // Arrange
            var order = new Order
            {
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00
            };

            // Act
            var result = await _repository.AddAsync(order);

            // Assert
            result.Should().NotBeNull();
            result.CustomerId.Should().Be(order.CustomerId);
            result.TotalAmount.Should().Be(order.TotalAmount);
        }

        [TestMethod]
        public async Task AddAsync_WithNullEntity_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => 
                _repository.AddAsync(null!));
        }

        [TestMethod]
        public async Task GetAllAsync_WithNoData_ReturnsEmptyCollection()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetAllAsync_WithData_ReturnsAllEntities()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 50.00 },
                new Order { CustomerId = 101, OrderDate = DateTime.Now, TotalAmount = 75.00 }
            };

            foreach (var order in orders)
            {
                await _repository.AddAsync(order);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();
            var resultList = result.ToList();

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().HaveCount(2);
            resultList.Should().Contain(o => o.CustomerId == 100);
            resultList.Should().Contain(o => o.CustomerId == 101);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithValidId_ReturnsEntity()
        {
            // Arrange
            var order = new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };
            var addedOrder = await _repository.AddAsync(order);
            await _context.SaveChangesAsync();
            var id = addedOrder.Id;

            // Act
            var result = await _repository.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.CustomerId.Should().Be(order.CustomerId);
            result.TotalAmount.Should().Be(order.TotalAmount);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999L);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetByIdAsync_WithZeroId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(0L);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void Update_WithValidEntity_ReturnsUpdatedEntity()
        {
            // Arrange
            var order = new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 50.00 };
            _context.Set<Order>().Add(order);
            _context.SaveChanges();

            order.TotalAmount = 100.00;

            // Act
            var result = _repository.Update(order);

            // Assert
            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(100.00);
        }

        [TestMethod]
        public void Update_WithNullEntity_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => 
                _repository.Update(null!));
        }

        [TestMethod]
        public void Remove_WithValidEntity_RemovesEntity()
        {
            // Arrange
            var order = new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 50.00 };
            _context.Set<Order>().Add(order);
            _context.SaveChanges();

            // Act
            _repository.Remove(order);
            _context.SaveChanges();

            // Assert
            var result = _context.Set<Order>().Find(order.Id);
            result.Should().BeNull();
        }

        [TestMethod]
        public void Remove_WithNullEntity_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => 
                _repository.Remove(null!));
        }

        [TestMethod]
        public async Task ExecuteQueryAsync_WithValidPredicate_ReturnsMatchingEntities()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 50.00 },
                new Order { CustomerId = 101, OrderDate = DateTime.Now, TotalAmount = 75.00 },
                new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 100.00 }
            };

            foreach (var order in orders)
            {
                await _repository.AddAsync(order);
            }
            await _context.SaveChangesAsync();

            Expression<Func<Order, bool>> predicate = o => o.CustomerId == 100;

            // Act
            var result = await _repository.ExecuteQueryAsync(predicate);
            var resultList = result.ToList();

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().HaveCount(2);
            resultList.Should().OnlyContain(o => o.CustomerId == 100);
        }

        [TestMethod]
        public async Task ExecuteQueryAsync_WithNoMatches_ReturnsEmptyCollection()
        {
            // Arrange
            var order = new Order { CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 50.00 };
            await _repository.AddAsync(order);
            await _context.SaveChangesAsync();

            Expression<Func<Order, bool>> predicate = o => o.CustomerId == 999;

            // Act
            var result = await _repository.ExecuteQueryAsync(predicate);
            var resultList = result.ToList();

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().BeEmpty();
        }

        [TestMethod]
        public async Task ExecuteQueryAsync_WithNullPredicate_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => 
                _repository.ExecuteQueryAsync(null!));
        }
    }
}

