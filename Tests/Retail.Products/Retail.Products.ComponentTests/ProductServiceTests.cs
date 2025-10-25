using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommonLibrary.MessageContract;
using CommonLibrary.Handlers.Dto;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Service;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
using MessagingLibrary.Interface;

namespace Retail.Products.ComponentTests
{
    /// <summary>
    /// Unit tests for ProductService class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class ProductServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<IMapper> _mockMapper = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IServiceScope> _mockServiceScope = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IMessagePublisher> _mockMessagePublisher = null!;
        private ProductService _productService = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockMessagePublisher = new Mock<IMessagePublisher>();

            // Note: We're not setting up the service scope mocks to avoid Moq extension method issues
            // These would be set up in individual tests that need them

            _productService = new ProductService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockMessagePublisher.Object,
                _mockServiceScopeFactory.Object);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public void ProductService_Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _productService.Should().NotBeNull();
            _productService.Should().BeOfType<ProductService>();
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task GetAllProductsAsync_WithValidData_ReturnsMappedProducts()
        {
            // Arrange
            var skus = new List<Sku>
            {
                new Sku { Id = 1, Name = "Product 1", UnitPrice = 29.99, Inventory = 100 },
                new Sku { Id = 2, Name = "Product 2", UnitPrice = 39.99, Inventory = 200 }
            };

            var skuDtos = new List<SkuDto>
            {
                new SkuDto { Id = 1, Name = "Product 1", UnitPrice = 29.99, Inventory = 100 },
                new SkuDto { Id = 2, Name = "Product 2", UnitPrice = 39.99, Inventory = 200 }
            };

            _mockUnitOfWork
                .Setup(x => x.Skus.GetAllAsync())
                .ReturnsAsync(skus);

            _mockMapper
                .Setup(x => x.Map<IEnumerable<SkuDto>>(skus))
                .Returns(skuDtos);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(skuDtos);

            _mockUnitOfWork.Verify(x => x.Skus.GetAllAsync(), Times.Once);
            _mockMapper.Verify(x => x.Map<IEnumerable<SkuDto>>(skus), Times.Once);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task GetAllProductsAsync_WithEmptyData_ReturnsEmptyCollection()
        {
            // Arrange
            var emptySkus = new List<Sku>();
            var emptySkuDtos = new List<SkuDto>();

            _mockUnitOfWork
                .Setup(x => x.Skus.GetAllAsync())
                .ReturnsAsync(emptySkus);

            _mockMapper
                .Setup(x => x.Map<IEnumerable<SkuDto>>(emptySkus))
                .Returns(emptySkuDtos);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _mockUnitOfWork.Verify(x => x.Skus.GetAllAsync(), Times.Once);
            _mockMapper.Verify(x => x.Map<IEnumerable<SkuDto>>(emptySkus), Times.Once);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
        {
            // Arrange
            var id = 1L;
            var sku = new Sku { Id = id, Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };
            var skuDto = new SkuDto { Id = id, Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };

            _mockUnitOfWork
                .Setup(x => x.Skus.GetByIdAsync(id))
                .ReturnsAsync(sku);

            _mockMapper
                .Setup(x => x.Map<SkuDto>(sku))
                .Returns(skuDto);

            // Act
            var result = await _productService.GetProductByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(skuDto);

            _mockUnitOfWork.Verify(x => x.Skus.GetByIdAsync(id), Times.Once);
            _mockMapper.Verify(x => x.Map<SkuDto>(sku), Times.Once);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var id = 999L;

            _mockUnitOfWork
                .Setup(x => x.Skus.GetByIdAsync(id))
                .ReturnsAsync((Sku?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(id);

            // Assert
            result.Should().BeNull();

            _mockUnitOfWork.Verify(x => x.Skus.GetByIdAsync(id), Times.Once);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task AddProductAsync_WithValidData_ReturnsAddedProduct()
        {
            // Arrange
            var skuDto = new SkuDto { Name = "New Product", UnitPrice = 49.99, Inventory = 150 };
            var sku = new Sku { Name = "New Product", UnitPrice = 49.99, Inventory = 150 };
            var addedSku = new Sku { Id = 1, Name = "New Product", UnitPrice = 49.99, Inventory = 150 };
            var resultSkuDto = new SkuDto { Id = 1, Name = "New Product", UnitPrice = 49.99, Inventory = 150 };

            _mockMapper
                .Setup(x => x.Map<Sku>(skuDto))
                .Returns(sku);

            _mockUnitOfWork
                .Setup(x => x.Skus.AddAsync(sku))
                .ReturnsAsync(addedSku);

            _mockMapper
                .Setup(x => x.Map<SkuDto>(addedSku))
                .Returns(resultSkuDto);

            // Act
            var result = await _productService.AddProductAsync(skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultSkuDto);

            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.Skus.AddAsync(sku), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            _mockMapper.Verify(x => x.Map<Sku>(skuDto), Times.Once);
            _mockMapper.Verify(x => x.Map<SkuDto>(addedSku), Times.Once);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task AddProductAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var skuDto = new SkuDto { Name = "New Product", UnitPrice = 49.99, Inventory = 150 };
            var sku = new Sku { Name = "New Product", UnitPrice = 49.99, Inventory = 150 };

            _mockMapper
                .Setup(x => x.Map<Sku>(skuDto))
                .Returns(sku);

            _mockUnitOfWork
                .Setup(x => x.Skus.AddAsync(sku))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await _productService.Invoking(x => x.AddProductAsync(skuDto))
                .Should().ThrowAsync<Exception>();

            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task UpdateProductAsync_WithValidData_ReturnsUpdatedProduct()
        {
            // Arrange
            var id = 1L;
            var skuDto = new SkuDto { Id = id, Name = "Updated Product", UnitPrice = 59.99, Inventory = 200 };
            var sku = new Sku { Id = id, Name = "Updated Product", UnitPrice = 59.99, Inventory = 200 };
            var updatedSku = new Sku { Id = id, Name = "Updated Product", UnitPrice = 59.99, Inventory = 200 };
            var resultSkuDto = new SkuDto { Id = id, Name = "Updated Product", UnitPrice = 59.99, Inventory = 200 };

            _mockMapper
                .Setup(x => x.Map<Sku>(skuDto))
                .Returns(sku);

            _mockUnitOfWork
                .Setup(x => x.Skus.GetByIdAsync(id))
                .ReturnsAsync(updatedSku);

            _mockMapper
                .Setup(x => x.Map<SkuDto>(updatedSku))
                .Returns(resultSkuDto);

            // Act
            var result = await _productService.UpdateProductAsync(id, skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultSkuDto);

            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.Skus.Update(sku), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.Skus.GetByIdAsync(id), Times.Once);
            _mockMapper.Verify(x => x.Map<Sku>(skuDto), Times.Once);
            _mockMapper.Verify(x => x.Map<SkuDto>(updatedSku), Times.Once);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task UpdateProductAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var id = 1L;
            var skuDto = new SkuDto { Id = id, Name = "Updated Product", UnitPrice = 59.99, Inventory = 200 };
            var sku = new Sku { Id = id, Name = "Updated Product", UnitPrice = 59.99, Inventory = 200 };

            _mockMapper
                .Setup(x => x.Map<Sku>(skuDto))
                .Returns(sku);

            _mockUnitOfWork
                .Setup(x => x.Skus.Update(sku))
                .Throws(new Exception("Database error"));

            // Act & Assert
            await _productService.Invoking(x => x.UpdateProductAsync(id, skuDto))
                .Should().ThrowAsync<Exception>();

            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task DeleteProductAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var id = 1L;
            var sku = new Sku { Id = id, Name = "Product to Delete", UnitPrice = 29.99, Inventory = 100 };

            _mockUnitOfWork
                .Setup(x => x.Skus.GetByIdAsync(id))
                .ReturnsAsync(sku);

            // Act
            var result = await _productService.DeleteProductAsync(id);

            // Assert
            result.Should().BeTrue();

            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.Skus.Remove(sku), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task DeleteProductAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var id = 999L;

            _mockUnitOfWork
                .Setup(x => x.Skus.GetByIdAsync(id))
                .ReturnsAsync((Sku?)null);

            // Act
            var result = await _productService.DeleteProductAsync(id);

            // Assert
            result.Should().BeFalse();

            _mockUnitOfWork.Verify(x => x.Skus.GetByIdAsync(id), Times.Once);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task DeleteProductAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var id = 1L;
            var sku = new Sku { Id = id, Name = "Product to Delete", UnitPrice = 29.99, Inventory = 100 };

            _mockUnitOfWork
                .Setup(x => x.Skus.GetByIdAsync(id))
                .ReturnsAsync(sku);

            _mockUnitOfWork
                .Setup(x => x.Skus.Remove(sku))
                .Throws(new Exception("Database error"));

            // Act & Assert
            await _productService.Invoking(x => x.DeleteProductAsync(id))
                .Should().ThrowAsync<Exception>();

            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        // Note: Event handling tests are commented out due to Moq extension method limitations
        // These would require more complex mocking setup that's beyond the scope of basic unit tests
        // For comprehensive event handling testing, consider using integration tests or service tests
    }
}
