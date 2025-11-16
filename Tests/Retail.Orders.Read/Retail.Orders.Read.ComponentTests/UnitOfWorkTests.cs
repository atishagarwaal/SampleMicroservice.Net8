using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MongoDB.Driver;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.UnitOfWork;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for UnitOfWork class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("UnitOfWork")]
    public sealed class UnitOfWorkTests
    {
        private Mock<ApplicationDbContext> _mockContext = null!;
        private UnitOfWork _unitOfWork = null!;

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
            _mockContext.Setup(x => x.GetCollection<Order>(It.IsAny<string>())).Returns(Mock.Of<MongoDB.Driver.IMongoCollection<Order>>());
            _unitOfWork = new UnitOfWork(_mockContext.Object);
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
        public void UnitOfWork_Orders_ReturnsSameInstance()
        {
            // Act
            var repository1 = _unitOfWork.Orders;
            var repository2 = _unitOfWork.Orders;

            // Assert
            repository1.Should().BeSameAs(repository2);
        }

        [TestMethod]
        public void UnitOfWork_Orders_IsIOrderRepository()
        {
            // Act
            var repository = _unitOfWork.Orders;

            // Assert
            repository.Should().BeAssignableTo<IOrderRepository>();
        }
    }
}

