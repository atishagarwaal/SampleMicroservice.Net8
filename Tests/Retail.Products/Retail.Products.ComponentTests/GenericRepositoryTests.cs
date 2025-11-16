using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories;

namespace Retail.Products.ComponentTests
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
        private GenericRepository<Sku> _repository = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new GenericRepository<Sku>(_context);
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
            var sku = new Sku
            {
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = 100
            };

            // Act
            var result = await _repository.AddAsync(sku);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(sku.Name);
            result.UnitPrice.Should().Be(sku.UnitPrice);
            result.Inventory.Should().Be(sku.Inventory);
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
            var skus = new List<Sku>
            {
                new Sku { Name = "Product 1", UnitPrice = 10.00, Inventory = 50 },
                new Sku { Name = "Product 2", UnitPrice = 20.00, Inventory = 100 }
            };

            foreach (var sku in skus)
            {
                await _repository.AddAsync(sku);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();
            var resultList = result.ToList();

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().HaveCount(2);
            resultList.Should().Contain(s => s.Name == "Product 1");
            resultList.Should().Contain(s => s.Name == "Product 2");
        }

        [TestMethod]
        public async Task GetByIdAsync_WithValidId_ReturnsEntity()
        {
            // Arrange
            var sku = new Sku { Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };
            var addedSku = await _repository.AddAsync(sku);
            await _context.SaveChangesAsync();
            var id = addedSku.Id;

            // Act
            var result = await _repository.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(sku.Name);
            result.UnitPrice.Should().Be(sku.UnitPrice);
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
            var sku = new Sku { Name = "Original", UnitPrice = 10.00, Inventory = 50 };
            _context.Set<Sku>().Add(sku);
            _context.SaveChanges();

            sku.Name = "Updated";
            sku.UnitPrice = 20.00;

            // Act
            var result = _repository.Update(sku);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated");
            result.UnitPrice.Should().Be(20.00);
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
            var sku = new Sku { Name = "To Remove", UnitPrice = 10.00, Inventory = 50 };
            _context.Set<Sku>().Add(sku);
            _context.SaveChanges();

            // Act
            _repository.Remove(sku);
            _context.SaveChanges();

            // Assert
            var result = _context.Set<Sku>().Find(sku.Id);
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
            var skus = new List<Sku>
            {
                new Sku { Name = "Product 1", UnitPrice = 10.00, Inventory = 50 },
                new Sku { Name = "Product 2", UnitPrice = 20.00, Inventory = 100 },
                new Sku { Name = "Product 3", UnitPrice = 10.00, Inventory = 75 }
            };

            foreach (var sku in skus)
            {
                await _repository.AddAsync(sku);
            }
            await _context.SaveChangesAsync();

            Expression<Func<Sku, bool>> predicate = s => s.UnitPrice == 10.00;

            // Act
            var result = await _repository.ExecuteQueryAsync(predicate);
            var resultList = result.ToList();

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().HaveCount(2);
            resultList.Should().OnlyContain(s => s.UnitPrice == 10.00);
        }

        [TestMethod]
        public async Task ExecuteQueryAsync_WithNoMatches_ReturnsEmptyCollection()
        {
            // Arrange
            var sku = new Sku { Name = "Product 1", UnitPrice = 10.00, Inventory = 50 };
            await _repository.AddAsync(sku);
            await _context.SaveChangesAsync();

            Expression<Func<Sku, bool>> predicate = s => s.UnitPrice == 999.99;

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

