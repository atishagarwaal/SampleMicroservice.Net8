using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Repositories;

namespace Retail.Customers.ComponentTests
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
        private GenericRepository<Customer> _repository = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new GenericRepository<Customer>(_context);
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
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = await _repository.AddAsync(customer);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be(customer.FirstName);
            result.LastName.Should().Be(customer.LastName);
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
            var customers = new List<Customer>
            {
                new Customer { FirstName = "John", LastName = "Doe" },
                new Customer { FirstName = "Jane", LastName = "Smith" }
            };

            foreach (var customer in customers)
            {
                await _repository.AddAsync(customer);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();
            var resultList = result.ToList();

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().HaveCount(2);
            resultList.Should().Contain(c => c.FirstName == "John");
            resultList.Should().Contain(c => c.FirstName == "Jane");
        }

        [TestMethod]
        public async Task GetByIdAsync_WithValidId_ReturnsEntity()
        {
            // Arrange
            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            var addedCustomer = await _repository.AddAsync(customer);
            await _context.SaveChangesAsync();
            var id = addedCustomer.Id;

            // Act
            var result = await _repository.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be(customer.FirstName);
            result.LastName.Should().Be(customer.LastName);
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
            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            _context.Set<Customer>().Add(customer);
            _context.SaveChanges();

            customer.FirstName = "Jane";

            // Act
            var result = _repository.Update(customer);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("Jane");
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
            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            _context.Set<Customer>().Add(customer);
            _context.SaveChanges();

            // Act
            _repository.Remove(customer);
            _context.SaveChanges();

            // Assert
            var result = _context.Set<Customer>().Find(customer.Id);
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
            var customers = new List<Customer>
            {
                new Customer { FirstName = "John", LastName = "Doe" },
                new Customer { FirstName = "Jane", LastName = "Smith" },
                new Customer { FirstName = "John", LastName = "Smith" }
            };

            foreach (var customer in customers)
            {
                await _repository.AddAsync(customer);
            }
            await _context.SaveChangesAsync();

            Expression<Func<Customer, bool>> predicate = c => c.FirstName == "John";

            // Act
            var result = await _repository.ExecuteQueryAsync(predicate);
            var resultList = result.ToList();

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().HaveCount(2);
            resultList.Should().OnlyContain(c => c.FirstName == "John");
        }

        [TestMethod]
        public async Task ExecuteQueryAsync_WithNoMatches_ReturnsEmptyCollection()
        {
            // Arrange
            var customer = new Customer { FirstName = "John", LastName = "Doe" };
            await _repository.AddAsync(customer);
            await _context.SaveChangesAsync();

            Expression<Func<Customer, bool>> predicate = c => c.FirstName == "NonExistent";

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

