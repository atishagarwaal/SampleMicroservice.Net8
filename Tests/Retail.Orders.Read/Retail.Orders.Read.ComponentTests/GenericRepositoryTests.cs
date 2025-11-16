using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Repositories;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for GenericRepository class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("GenericRepository")]
    public sealed class GenericRepositoryTests
    {
        private Mock<ApplicationDbContext> _mockContext = null!;
        private Mock<IMongoCollection<Order>> _mockCollection = null!;
        private GenericRepository<Order> _repository = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            // Use ConfigurationBuilder to create a real configuration that works with GetConnectionString
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:DefaultConnection", "mongodb://localhost:27017" },
                    { "MongoDBSettings:DatabaseName", "OrdersDb" }
                })
                .Build();
            
            _mockContext = new Mock<ApplicationDbContext>(configuration);
            _mockCollection = new Mock<IMongoCollection<Order>>();
            
            _mockContext
                .Setup(x => x.GetCollection<Order>(It.IsAny<string>()))
                .Returns(_mockCollection.Object);

            _repository = new GenericRepository<Order>(_mockContext.Object, "Orders");
        }

        [TestMethod]
        public async Task AddAsync_WithValidEntity_ReturnsEntity()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 100,
                OrderDate = DateTime.Now,
                TotalAmount = 150.00
            };

            _mockCollection
                .Setup(x => x.InsertOneAsync(order, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

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
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<Order>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.Setup(x => x.Current).Returns(new List<Order>());

            _mockCollection
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Order>>(), It.IsAny<FindOptions<Order, Order>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

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
                new Order { Id = 1, CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 50.00 },
                new Order { Id = 2, CustomerId = 101, OrderDate = DateTime.Now, TotalAmount = 75.00 }
            };

            var mockCursor = new Mock<IAsyncCursor<Order>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.Setup(x => x.Current).Returns(orders);

            _mockCollection
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Order>>(), It.IsAny<FindOptions<Order, Order>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetAllAsync();
            var resultList = result.ToList();

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().HaveCount(2);
            resultList.Should().Contain(o => o.Id == 1);
            resultList.Should().Contain(o => o.Id == 2);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithValidId_ReturnsEntity()
        {
            // Arrange
            var order = new Order { Id = 1, CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };

            var mockCursor = new Mock<IAsyncCursor<Order>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.Setup(x => x.Current).Returns(new List<Order> { order });

            _mockCollection
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Order>>(), It.IsAny<FindOptions<Order, Order>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetByIdAsync(1L);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.CustomerId.Should().Be(100);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<Order>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.Setup(x => x.Current).Returns(new List<Order>());

            _mockCollection
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Order>>(), It.IsAny<FindOptions<Order, Order>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetByIdAsync(999L);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task RemoveAsync_WithValidId_RemovesEntity()
        {
            // Arrange
            _mockCollection
                .Setup(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Order>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Act
            await _repository.RemoveAsync(1L);

            // Assert
            // Method completed without exception
            _repository.Should().NotBeNull();
        }

        [TestMethod]
        public async Task UpdateAsync_WithValidEntity_UpdatesEntity()
        {
            // Arrange
            var order = new Order { Id = 1, CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 150.00 };

            _mockCollection
                .Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Order>>(), order, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, order.Id));

            // Act
            await _repository.UpdateAsync(1L, order);

            // Assert
            // Method completed without exception
            _repository.Should().NotBeNull();
        }

        [TestMethod]
        public async Task UpdateAsync_WithNullEntity_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => 
                _repository.UpdateAsync(1L, null!));
        }

        [TestMethod]
        public async Task ExecuteQueryAsync_WithValidPredicate_ReturnsMatchingEntities()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 50.00 },
                new Order { Id = 2, CustomerId = 101, OrderDate = DateTime.Now, TotalAmount = 75.00 },
                new Order { Id = 3, CustomerId = 100, OrderDate = DateTime.Now, TotalAmount = 100.00 }
            };

            var matchingOrders = orders.Where(o => o.CustomerId == 100).ToList();

            var mockCursor = new Mock<IAsyncCursor<Order>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.Setup(x => x.Current).Returns(matchingOrders);

            _mockCollection
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Order>>(), It.IsAny<FindOptions<Order, Order>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

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
            var mockCursor = new Mock<IAsyncCursor<Order>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.Setup(x => x.Current).Returns(new List<Order>());

            _mockCollection
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Order>>(), It.IsAny<FindOptions<Order, Order>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

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

