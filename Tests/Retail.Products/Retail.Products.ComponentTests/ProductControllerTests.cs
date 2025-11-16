using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Retail.Api.Products.src.CleanArchitecture.API.Controllers;
using Retail.Api.Products.src.CleanArchitecture.Application.Constants;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Validation;
using Retail.Api.Products.src.CleanArchitecture.Application.Validation.Interfaces;

namespace Retail.Products.ComponentTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class ProductControllerTests
    {
        private Mock<IProductService> _mockProductService = null!;
        private Mock<IMessageValidator<SkuDto>> _mockSkuDtoValidator = null!;
        private Mock<ILogger<ProductController>> _mockLogger = null!;
        private ProductController _controller = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProductService = new Mock<IProductService>();
            _mockSkuDtoValidator = new Mock<IMessageValidator<SkuDto>>();
            _mockLogger = new Mock<ILogger<ProductController>>();
            _controller = new ProductController(_mockProductService.Object, _mockSkuDtoValidator.Object, _mockLogger.Object);

            // Default setup for validator to pass
            _mockSkuDtoValidator.Setup(x => x.Validate(It.IsAny<SkuDto>()))
                .Returns(new ValidationData());
        }

        [TestMethod]
        public void ProductController_Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            _controller.Should().NotBeNull();
            _controller.Should().BeOfType<ProductController>();
        }

        [TestMethod]
        public async Task Post_WithValidSkuDto_ReturnsOkResult()
        {
            // Arrange
            var skuDto = new SkuDto { Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };
            _mockProductService.Setup(x => x.AddProductAsync(skuDto))
                .ReturnsAsync(skuDto);

            // Act
            var result = await _controller.Post(skuDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.Value.Should().BeEquivalentTo(skuDto);
            _mockSkuDtoValidator.Verify(x => x.Validate(skuDto), Times.Once);
            _mockProductService.Verify(x => x.AddProductAsync(skuDto), Times.Once);
        }

        [TestMethod]
        public async Task Post_WithNullSkuDto_ReturnsBadRequest()
        {
            // Arrange
            SkuDto nullSkuDto = null!;

            // Act
            var result = await _controller.Post(nullSkuDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.Value.Should().Be(MessageConstants.InvalidParameter);
            _mockSkuDtoValidator.Verify(x => x.Validate(It.IsAny<SkuDto>()), Times.Never);
            _mockProductService.Verify(x => x.AddProductAsync(It.IsAny<SkuDto>()), Times.Never);
        }

        [TestMethod]
        public async Task Post_WithInvalidSkuDto_ReturnsBadRequest()
        {
            // Arrange
            var invalidSkuDto = new SkuDto { Name = null, UnitPrice = 29.99, Inventory = 100 };
            var validationData = new ValidationData("SkuDtoValidator", "The Name field is null or whitespace.", FailureSeverity.Error);
            _mockSkuDtoValidator.Setup(x => x.Validate(invalidSkuDto)).Returns(validationData);

            // Act
            var result = await _controller.Post(invalidSkuDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestValue = (result as BadRequestObjectResult)!.Value as dynamic;
            ((string)badRequestValue.error).Should().Be("The Name field is null or whitespace.");
            ((string)badRequestValue.validator).Should().Be("SkuDtoValidator");
            _mockSkuDtoValidator.Verify(x => x.Validate(invalidSkuDto), Times.Once);
            _mockProductService.Verify(x => x.AddProductAsync(It.IsAny<SkuDto>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_WithValidSkuDto_ReturnsOkResult()
        {
            // Arrange
            long id = 1;
            var skuDto = new SkuDto { Id = id, Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };
            _mockProductService.Setup(x => x.UpdateProductAsync(id, skuDto))
                .ReturnsAsync(skuDto);

            // Act
            var result = await _controller.Put(id, skuDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.Value.Should().BeEquivalentTo(skuDto);
            _mockSkuDtoValidator.Verify(x => x.Validate(skuDto), Times.Once);
            _mockProductService.Verify(x => x.UpdateProductAsync(id, skuDto), Times.Once);
        }

        [TestMethod]
        public async Task Put_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            long invalidId = 0;
            var skuDto = new SkuDto { Name = "Test Product", UnitPrice = 29.99, Inventory = 100 };

            // Act
            var result = await _controller.Put(invalidId, skuDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.Value.Should().Be(MessageConstants.InvalidParameter);
            _mockSkuDtoValidator.Verify(x => x.Validate(It.IsAny<SkuDto>()), Times.Never);
            _mockProductService.Verify(x => x.UpdateProductAsync(It.IsAny<long>(), It.IsAny<SkuDto>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_WithNullSkuDto_ReturnsBadRequest()
        {
            // Arrange
            long id = 1;
            SkuDto nullSkuDto = null!;

            // Act
            var result = await _controller.Put(id, nullSkuDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.Value.Should().Be(MessageConstants.InvalidParameter);
            _mockSkuDtoValidator.Verify(x => x.Validate(It.IsAny<SkuDto>()), Times.Never);
            _mockProductService.Verify(x => x.UpdateProductAsync(It.IsAny<long>(), It.IsAny<SkuDto>()), Times.Never);
        }

        [TestMethod]
        public async Task Put_WithInvalidSkuDto_ReturnsBadRequest()
        {
            // Arrange
            long id = 1;
            var invalidSkuDto = new SkuDto { Id = id, Name = null, UnitPrice = 29.99, Inventory = 100 };
            var validationData = new ValidationData("SkuDtoValidator", "The Name field is null or whitespace.", FailureSeverity.Error);
            _mockSkuDtoValidator.Setup(x => x.Validate(invalidSkuDto)).Returns(validationData);

            // Act
            var result = await _controller.Put(id, invalidSkuDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestValue = (result as BadRequestObjectResult)!.Value as dynamic;
            ((string)badRequestValue.error).Should().Be("The Name field is null or whitespace.");
            ((string)badRequestValue.validator).Should().Be("SkuDtoValidator");
            _mockSkuDtoValidator.Verify(x => x.Validate(invalidSkuDto), Times.Once);
            _mockProductService.Verify(x => x.UpdateProductAsync(It.IsAny<long>(), It.IsAny<SkuDto>()), Times.Never);
        }
    }
}

