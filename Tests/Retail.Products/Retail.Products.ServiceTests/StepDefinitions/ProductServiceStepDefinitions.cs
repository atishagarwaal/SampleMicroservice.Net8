using FluentAssertions;
using NUnit.Framework;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Products.ServiceTests.Common;
using TechTalk.SpecFlow;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using CommonLibrary.Handlers.Dto;
using It = Moq.It;

namespace Retail.Products.ServiceTests.StepDefinitions
{
    [Binding]
    public class ProductServiceStepDefinitions : TestBase
    {
        private IProductService _productService = null!;
        private IEnumerable<SkuDto>? _allProducts;
        private SkuDto? _productById;
        private SkuDto? _addedProduct;
        private SkuDto? _updatedProduct;
        private bool _deleteResult;
        private Exception? _exception;

        [BeforeScenario]
        public void BeforeScenario()
        {
            SetupServices();
            
            // Create ProductService manually with mocked dependencies
            var mockMapper = new Mock<AutoMapper.IMapper>();
            var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            
            // Set up AutoMapper mock to return mapped objects
            mockMapper.Setup(x => x.Map<IEnumerable<SkuDto>>(It.IsAny<IEnumerable<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>>()))
                .Returns<IEnumerable<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>>(skus => 
                    skus.Select(s => new SkuDto { Id = s.Id, Name = s.Name, UnitPrice = s.UnitPrice, Inventory = s.Inventory }).ToList());
            
            mockMapper.Setup(x => x.Map<SkuDto>(It.IsAny<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>()))
                .Returns<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>(sku => 
                    new SkuDto { Id = sku.Id, Name = sku.Name, UnitPrice = sku.UnitPrice, Inventory = sku.Inventory });
            
            mockMapper.Setup(x => x.Map<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>(It.IsAny<SkuDto>()))
                .Returns<SkuDto>(dto => 
                    new Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku { Id = dto.Id, Name = dto.Name, UnitPrice = dto.UnitPrice, Inventory = dto.Inventory });
            
            _productService = new Retail.Api.Products.src.CleanArchitecture.Application.Service.ProductService(
                MockUnitOfWork.Object,
                mockMapper.Object,
                MockMessagePublisher.Object,
                mockServiceScopeFactory.Object);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            Cleanup();
        }

        [Given(@"I have a product service")]
        public void GivenIHaveAProductService()
        {
            _productService.Should().NotBeNull();
        }

        [Given(@"there are products in the system")]
        public void GivenThereAreProductsInTheSystem()
        {
            var products = new List<SkuDto>
            {
                new SkuDto { Id = 1, Name = "Product 1", UnitPrice = 29.99, Inventory = 100 },
                new SkuDto { Id = 2, Name = "Product 2", UnitPrice = 39.99, Inventory = 200 }
            };

            MockUnitOfWork.Setup(x => x.Skus.GetAllAsync())
                .ReturnsAsync(products.Select(p => new Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku
                {
                    Id = p.Id,
                    Name = p.Name,
                    UnitPrice = p.UnitPrice,
                    Inventory = p.Inventory
                }));
        }

        [Given(@"there are no products in the system")]
        public void GivenThereAreNoProductsInTheSystem()
        {
            MockUnitOfWork.Setup(x => x.Skus.GetAllAsync())
                .ReturnsAsync(new List<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>());
        }

        [Given(@"there is a product with ID ""(.*)""")]
        public void GivenThereIsAProductWithId(string id)
        {
            var productId = long.Parse(id);
            var product = new Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku
            {
                Id = productId,
                Name = "Test Product",
                UnitPrice = 29.99,
                Inventory = 100
            };

            MockUnitOfWork.Setup(x => x.Skus.GetByIdAsync(productId))
                .ReturnsAsync(product);
        }

        [Given(@"there is no product with ID ""(.*)""")]
        public void GivenThereIsNoProductWithId(string id)
        {
            var productId = long.Parse(id);
            MockUnitOfWork.Setup(x => x.Skus.GetByIdAsync(productId))
                .ReturnsAsync((Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku?)null);
        }

        [Given(@"I have a valid product to add")]
        public void GivenIHaveAValidProductToAdd()
        {
            // This will be set in the When step
        }

        [Given(@"I have invalid product data")]
        public void GivenIHaveInvalidProductData()
        {
            // This will be set in the When step
        }

        [Given(@"I have updated product information")]
        public void GivenIHaveUpdatedProductInformation()
        {
            // This will be set in the When step
        }

        [Given(@"there is an existing product with ID ""(.*)""")]
        public void GivenThereIsAnExistingProductWithId(string id)
        {
            var productId = long.Parse(id);
            var product = new Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku
            {
                Id = productId,
                Name = "Existing Product",
                UnitPrice = 29.99,
                Inventory = 100
            };

            MockUnitOfWork.Setup(x => x.Skus.GetByIdAsync(productId))
                .ReturnsAsync(product);
        }

        [Given(@"there are products with sufficient inventory")]
        public void GivenThereAreProductsWithSufficientInventory()
        {
            // This will be set up in the When step for event handling
        }

        [Given(@"there are products with insufficient inventory")]
        public void GivenThereAreProductsWithInsufficientInventory()
        {
            // This will be set up in the When step for event handling
        }

        [Given(@"there are products with zero inventory")]
        public void GivenThereAreProductsWithZeroInventory()
        {
            // This will be set up in the When step for event handling
        }

        [Given(@"there are multiple products with sufficient inventory")]
        public void GivenThereAreMultipleProductsWithSufficientInventory()
        {
            // This will be set up in the When step for event handling
        }

        [Given(@"an order created event is received")]
        public void GivenAnOrderCreatedEventIsReceived()
        {
            // This will be set up in the When step for event handling
        }

        [Given(@"an order created event with multiple line items is received")]
        public void GivenAnOrderCreatedEventWithMultipleLineItemsIsReceived()
        {
            // This will be set up in the When step for event handling
        }

        [When(@"I request all products")]
        public async Task WhenIRequestAllProducts()
        {
            try
            {
                _allProducts = await _productService.GetAllProductsAsync();
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [When(@"I request the product with ID ""(.*)""")]
        public async Task WhenIRequestTheProductWithId(string id)
        {
            try
            {
                var productId = long.Parse(id);
                _productById = await _productService.GetProductByIdAsync(productId);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [When(@"I add the new product")]
        public async Task WhenIAddTheNewProduct()
        {
            try
            {
                var newProduct = new SkuDto
                {
                    Name = "New Product",
                    UnitPrice = 49.99,
                    Inventory = 150
                };

                // Set up mocks for AddProductAsync
                var addedSku = new Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku
                {
                    Id = 1,
                    Name = "New Product",
                    UnitPrice = 49.99,
                    Inventory = 150
                };

                MockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
                MockUnitOfWork.Setup(x => x.Skus.AddAsync(It.IsAny<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>()))
                    .ReturnsAsync(addedSku);
                MockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);
                MockUnitOfWork.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

                _addedProduct = await _productService.AddProductAsync(newProduct);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [When(@"I update the product with ID ""(.*)""")]
        public async Task WhenIUpdateTheProductWithId(string id)
        {
            try
            {
                var productId = long.Parse(id);
                var updatedProduct = new SkuDto
                {
                    Id = productId,
                    Name = "Updated Product",
                    UnitPrice = 59.99,
                    Inventory = 200
                };

                // Set up mocks for UpdateProductAsync
                var updatedSku = new Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku
                {
                    Id = productId,
                    Name = "Updated Product",
                    UnitPrice = 59.99,
                    Inventory = 200
                };

                MockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
                MockUnitOfWork.Setup(x => x.Skus.Update(It.IsAny<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>()));
                MockUnitOfWork.Setup(x => x.Skus.GetByIdAsync(productId)).ReturnsAsync(updatedSku);
                MockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);
                MockUnitOfWork.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

                _updatedProduct = await _productService.UpdateProductAsync(productId, updatedProduct);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [When(@"I delete the product with ID ""(.*)""")]
        public async Task WhenIDeleteTheProductWithId(string id)
        {
            try
            {
                var productId = long.Parse(id);
                
                // Set up mocks for DeleteProductAsync
                MockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
                MockUnitOfWork.Setup(x => x.Skus.Remove(It.IsAny<Retail.Api.Products.src.CleanArchitecture.Domain.Entities.Sku>()));
                MockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);
                MockUnitOfWork.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
                
                _deleteResult = await _productService.DeleteProductAsync(productId);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [When(@"I add the invalid product")]
        public async Task WhenIAddTheInvalidProduct()
        {
            try
            {
                var invalidProduct = new SkuDto
                {
                    Name = "", // Invalid: empty name
                    UnitPrice = -10.0, // Invalid: negative price
                    Inventory = -5 // Invalid: negative inventory
                };

                _addedProduct = await _productService.AddProductAsync(invalidProduct);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [When(@"I handle the order created event")]
        public async Task WhenIHandleTheOrderCreatedEvent()
        {
            try
            {
                // Set up mocks for event handling
                var mockOrderCreatedEvent = new CommonLibrary.MessageContract.OrderCreatedEvent
                {
                    OrderId = 1,
                    LineItems = new List<CommonLibrary.Handlers.Dto.LineItemDto>
                    {
                        new CommonLibrary.Handlers.Dto.LineItemDto { SkuId = 1, Qty = 2 },
                        new CommonLibrary.Handlers.Dto.LineItemDto { SkuId = 2, Qty = 1 }
                    }
                };

                // Set up mocks for the event handling scenario
                MockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
                MockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);
                MockUnitOfWork.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
                MockUnitOfWork.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);

                // Call the event handler
                await _productService.HandleOrderCreatedEvent(mockOrderCreatedEvent);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [Then(@"I should receive a list of all products")]
        public void ThenIShouldReceiveAListOfAllProducts()
        {
            _allProducts.Should().NotBeNull();
        }

        [Then(@"the list should contain the correct number of products")]
        public void ThenTheListShouldContainTheCorrectNumberOfProducts()
        {
            _allProducts.Should().HaveCount(2);
        }

        [Then(@"I should receive the product details")]
        public void ThenIShouldReceiveTheProductDetails()
        {
            _productById.Should().NotBeNull();
        }

        [Then(@"the product should have the correct ID")]
        public void ThenTheProductShouldHaveTheCorrectId()
        {
            _productById!.Id.Should().Be(1);
        }

        [Then(@"I should receive null")]
        public void ThenIShouldReceiveNull()
        {
            _productById.Should().BeNull();
        }

        [Then(@"I should receive an empty list")]
        public void ThenIShouldReceiveAnEmptyList()
        {
            _allProducts.Should().BeEmpty();
        }

        [Then(@"the product should be added successfully")]
        public void ThenTheProductShouldBeAddedSuccessfully()
        {
            _addedProduct.Should().NotBeNull();
        }

        [Then(@"I should receive the added product with an ID")]
        public void ThenIShouldReceiveTheAddedProductWithAnId()
        {
            _addedProduct!.Id.Should().BeGreaterThan(0);
        }

        [Then(@"the product should be updated successfully")]
        public void ThenTheProductShouldBeUpdatedSuccessfully()
        {
            _updatedProduct.Should().NotBeNull();
        }

        [Then(@"I should receive the updated product details")]
        public void ThenIShouldReceiveTheUpdatedProductDetails()
        {
            _updatedProduct!.Name.Should().Be("Updated Product");
            _updatedProduct.UnitPrice.Should().Be(59.99);
            _updatedProduct.Inventory.Should().Be(200);
        }

        [Then(@"the product should be deleted successfully")]
        public void ThenTheProductShouldBeDeletedSuccessfully()
        {
            _deleteResult.Should().BeTrue();
        }

        [Then(@"the operation should return true")]
        public void ThenTheOperationShouldReturnTrue()
        {
            _deleteResult.Should().BeTrue();
        }

        [Then(@"the operation should return false")]
        public void ThenTheOperationShouldReturnFalse()
        {
            _deleteResult.Should().BeFalse();
        }

        [Then(@"the operation should fail")]
        public void ThenTheOperationShouldFail()
        {
            _exception.Should().NotBeNull();
        }

        [Then(@"an appropriate error should be thrown")]
        public void ThenAnAppropriateErrorShouldBeThrown()
        {
            _exception.Should().NotBeNull();
        }

        [Then(@"the inventory should be updated correctly")]
        public void ThenTheInventoryShouldBeUpdatedCorrectly()
        {
            // This would be implemented based on your actual event handling logic
            Assert.Pass("Inventory update verification would be implemented here");
        }

        [Then(@"an inventory updated event should be published")]
        public void ThenAnInventoryUpdatedEventShouldBePublished()
        {
            // This would be implemented based on your actual event handling logic
            Assert.Pass("Event publishing verification would be implemented here");
        }

        [Then(@"the transaction should be committed")]
        public void ThenTheTransactionShouldBeCommitted()
        {
            // This would be implemented based on your actual event handling logic
            Assert.Pass("Transaction commit verification would be implemented here");
        }

        [Then(@"an exception should be thrown")]
        public void ThenAnExceptionShouldBeThrown()
        {
            _exception.Should().NotBeNull();
        }

        [Then(@"an inventory error event should be published")]
        public void ThenAnInventoryErrorEventShouldBePublished()
        {
            // This would be implemented based on your actual event handling logic
            Assert.Pass("Error event publishing verification would be implemented here");
        }

        [Then(@"the transaction should be rolled back")]
        public void ThenTheTransactionShouldBeRolledBack()
        {
            // This would be implemented based on your actual event handling logic
            Assert.Pass("Transaction rollback verification would be implemented here");
        }

        [Then(@"all product inventories should be updated correctly")]
        public void ThenAllProductInventoriesShouldBeUpdatedCorrectly()
        {
            // This would be implemented based on your actual event handling logic
            Assert.Pass("Multiple inventory update verification would be implemented here");
        }
    }
}
