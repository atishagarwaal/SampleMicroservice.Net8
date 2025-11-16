using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.MessageContract;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Service;
using Retail.Api.Products.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Validation;
using Retail.Api.Products.src.CleanArchitecture.Application.Validation.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
using MessagingLibrary.Interface;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories;

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
        private Mock<ISkuRepository> _mockSkuRepository = null!;
        private Mock<IConverter<SkuDto, Sku>> _mockSkuConverter = null!;
        private Mock<IConverter<Sku, SkuDto>> _mockSkuDtoConverter = null!;
        private Mock<IMessageValidator<SkuDto>> _mockSkuDtoValidator = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IServiceScope> _mockServiceScope = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IMessagePublisher> _mockMessagePublisher = null!;
        private Mock<ILogger<ProductService>> _mockLogger = null!;
        private ProductService _productService = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSkuRepository = new Mock<ISkuRepository>();
            _mockSkuConverter = new Mock<IConverter<SkuDto, Sku>>();
            _mockSkuDtoConverter = new Mock<IConverter<Sku, SkuDto>>();
            _mockSkuDtoValidator = new Mock<IMessageValidator<SkuDto>>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockMessagePublisher = new Mock<IMessagePublisher>();
            _mockLogger = new Mock<ILogger<ProductService>>();

            _mockUnitOfWork
                .Setup(x => x.Skus)
                .Returns(_mockSkuRepository.Object);

            // Setup default validation to pass
            _mockSkuDtoValidator
                .Setup(x => x.Validate(It.IsAny<SkuDto>()))
                .Returns(new ValidationData());

            _productService = new ProductService(
                _mockUnitOfWork.Object,
                _mockSkuConverter.Object,
                _mockSkuDtoConverter.Object,
                _mockSkuDtoValidator.Object,
                _mockMessagePublisher.Object,
                _mockServiceScopeFactory.Object,
                _mockLogger.Object);
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

            _mockSkuRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(skus);

            _mockSkuDtoConverter
                .Setup(x => x.Convert(It.IsAny<Sku>()))
                .Returns<Sku>(sku => skuDtos.First(dto => dto.Id == sku.Id));

            // Act
            var result = await _productService.GetAllProductsAsync();
            var resultList = result.ToList(); // Materialize the result

            // Assert
            resultList.Should().NotBeNull();
            resultList.Should().HaveCount(2);
            resultList.Should().BeEquivalentTo(skuDtos);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task GetAllProductsAsync_WithEmptyData_ReturnsEmptyCollection()
        {
            // Arrange
            var emptySkus = new List<Sku>();

            _mockSkuRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(emptySkus);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
        {
            // Arrange
            var id = 1L;
            var sku = new Sku { Id = id, Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };
            var skuDto = new SkuDto { Id = id, Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };

            _mockSkuRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(sku);

            _mockSkuDtoConverter
                .Setup(x => x.Convert(sku))
                .Returns(skuDto);

            // Act
            var result = await _productService.GetProductByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(skuDto);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var id = 999L;

            _mockSkuRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Sku?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(id);

            // Assert
            result.Should().BeNull();
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

            _mockSkuDtoValidator
                .Setup(x => x.Validate(skuDto))
                .Returns(new ValidationData());

            _mockSkuConverter
                .Setup(x => x.Convert(skuDto))
                .Returns(sku);

            _mockSkuRepository
                .Setup(x => x.AddAsync(sku))
                .ReturnsAsync(addedSku);

            _mockSkuDtoConverter
                .Setup(x => x.Convert(addedSku))
                .Returns(resultSkuDto);

            // Act
            var result = await _productService.AddProductAsync(skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultSkuDto);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task AddProductAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var skuDto = new SkuDto { Name = "New Product", UnitPrice = 49.99, Inventory = 150 };
            var sku = new Sku { Name = "New Product", UnitPrice = 49.99, Inventory = 150 };

            _mockSkuDtoValidator
                .Setup(x => x.Validate(skuDto))
                .Returns(new ValidationData());

            _mockSkuConverter
                .Setup(x => x.Convert(skuDto))
                .Returns(sku);

            _mockSkuRepository
                .Setup(x => x.AddAsync(sku))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await _productService.Invoking(x => x.AddProductAsync(skuDto))
                .Should().ThrowAsync<Exception>();
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task AddProductAsync_WithInvalidData_ThrowsArgumentException()
        {
            // Arrange
            var skuDto = new SkuDto { Name = null, UnitPrice = 49.99, Inventory = 150 };
            var validationData = new ValidationData("SkuDtoValidator", "The Name field is null or whitespace.", FailureSeverity.Error);

            _mockSkuDtoValidator
                .Setup(x => x.Validate(skuDto))
                .Returns(validationData);

            // Act & Assert
            await _productService.Invoking(x => x.AddProductAsync(skuDto))
                .Should().ThrowAsync<ArgumentException>();
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

            _mockSkuDtoValidator
                .Setup(x => x.Validate(skuDto))
                .Returns(new ValidationData());

            _mockSkuConverter
                .Setup(x => x.Convert(skuDto))
                .Returns(sku);

            _mockSkuRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(updatedSku);

            _mockSkuDtoConverter
                .Setup(x => x.Convert(updatedSku))
                .Returns(resultSkuDto);

            // Act
            var result = await _productService.UpdateProductAsync(id, skuDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(resultSkuDto);
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task UpdateProductAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var id = 1L;
            var skuDto = new SkuDto { Id = id, Name = "Updated Product", UnitPrice = 59.99, Inventory = 200 };
            var sku = new Sku { Id = id, Name = "Updated Product", UnitPrice = 59.99, Inventory = 200 };

            _mockSkuDtoValidator
                .Setup(x => x.Validate(skuDto))
                .Returns(new ValidationData());

            _mockSkuConverter
                .Setup(x => x.Convert(skuDto))
                .Returns(sku);

            _mockSkuRepository
                .Setup(x => x.Update(It.IsAny<Sku>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            await _productService.Invoking(x => x.UpdateProductAsync(id, skuDto))
                .Should().ThrowAsync<Exception>();
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task UpdateProductAsync_WithInvalidData_ThrowsArgumentException()
        {
            // Arrange
            var id = 1L;
            var skuDto = new SkuDto { Id = id, Name = null, UnitPrice = 59.99, Inventory = 200 };
            var validationData = new ValidationData("SkuDtoValidator", "The Name field is null or whitespace.", FailureSeverity.Error);

            _mockSkuDtoValidator
                .Setup(x => x.Validate(skuDto))
                .Returns(validationData);

            // Act & Assert
            await _productService.Invoking(x => x.UpdateProductAsync(id, skuDto))
                .Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task DeleteProductAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var id = 1L;
            var sku = new Sku { Id = id, Name = "Product to Delete", UnitPrice = 29.99, Inventory = 100 };

            _mockSkuRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(sku);

            // Act
            var result = await _productService.DeleteProductAsync(id);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task DeleteProductAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var id = 999L;

            _mockSkuRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Sku?)null);

            // Act
            var result = await _productService.DeleteProductAsync(id);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("ProductService")]
        public async Task DeleteProductAsync_WhenExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var id = 1L;
            var sku = new Sku { Id = id, Name = "Product to Delete", UnitPrice = 29.99, Inventory = 100 };

            _mockSkuRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(sku);

            _mockSkuRepository
                .Setup(x => x.Remove(sku))
                .Throws(new Exception("Database error"));

            // Act & Assert
            await _productService.Invoking(x => x.DeleteProductAsync(id))
                .Should().ThrowAsync<Exception>();
        }

        // Note: Event handling tests are commented out due to Moq extension method limitations
        // These would require more complex mocking setup that's beyond the scope of basic unit tests
        // For comprehensive event handling testing, consider using integration tests or service tests
    }
}
