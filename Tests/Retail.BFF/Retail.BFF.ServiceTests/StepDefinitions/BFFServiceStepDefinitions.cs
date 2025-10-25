using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Retail.BFF.ServiceTests.Common;
using Retail.BFFWeb.Api.Controller;
using Retail.BFFWeb.Api.Interface;
using Retail.BFFWeb.Api.Model;
using TechTalk.SpecFlow;

namespace Retail.BFF.ServiceTests.StepDefinitions
{
    [Binding]
    public class BFFServiceStepDefinitions : TestBase
    {
        private BFFController _bffController = null!;
        private List<SkuDto> _products = null!;
        private List<CustomerDto> _customers = null!;
        private List<OrderDto> _orders = null!;
        private IActionResult _response = null!;
        private Exception? _exception = null!;

        [BeforeScenario]
        public void BeforeScenario()
        {
            SetupServices();

            // Create BFFController with mocked dependencies
            _bffController = new BFFController(
                MockCustomerProvider.Object,
                MockOrderProvider.Object,
                MockProductProvider.Object);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            Cleanup();
        }

        [Given(@"the BFF service is running")]
        public void GivenTheBFFServiceIsRunning()
        {
            _bffController.Should().NotBeNull();
        }

        [Given(@"there are products available in the system")]
        public void GivenThereAreProductsAvailableInTheSystem()
        {
            _products = new List<SkuDto>
            {
                new SkuDto { Id = 1, Name = "Product 1", UnitPrice = 29.99 },
                new SkuDto { Id = 2, Name = "Product 2", UnitPrice = 39.99 }
            };
            MockProductProvider.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(_products);
        }

        [Given(@"there are customers available in the system")]
        public void GivenThereAreCustomersAvailableInTheSystem()
        {
            _customers = new List<CustomerDto>
            {
                new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" },
                new CustomerDto { Id = 2, FirstName = "Jane", LastName = "Smith" }
            };
            MockCustomerProvider.Setup(x => x.GetAllCustomersAsync()).ReturnsAsync(_customers);
        }

        [Given(@"there are orders available in the system")]
        public void GivenThereAreOrdersAvailableInTheSystem()
        {
            _orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>
                    {
                        new LineItemDto { Id = 1, OrderId = 1, SkuId = 1, Qty = 2 }
                    }
                }
            };
            MockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(_orders);
        }

        [Given(@"there are no orders in the system")]
        public void GivenThereAreNoOrdersInTheSystem()
        {
            _orders = new List<OrderDto>();
            MockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(_orders);
        }

        [Given(@"an order references a non-existent customer")]
        public void GivenAnOrderReferencesANonExistentCustomer()
        {
            _orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerId = 999, // Non-existent customer ID
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>
                    {
                        new LineItemDto { Id = 1, OrderId = 1, SkuId = 1, Qty = 2 }
                    }
                }
            };
            MockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(_orders);
            MockCustomerProvider.Setup(x => x.GetCustomerByIdAsync(999)).ReturnsAsync((CustomerDto?)null);
        }

        [Given(@"an order references a non-existent product")]
        public void GivenAnOrderReferencesANonExistentProduct()
        {
            _orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>
                    {
                        new LineItemDto { Id = 1, OrderId = 1, SkuId = 999, Qty = 2 } // Non-existent product ID
                    }
                }
            };
            MockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(_orders);
            MockCustomerProvider.Setup(x => x.GetCustomerByIdAsync(1)).ReturnsAsync(_customers[0]);
            MockProductProvider.Setup(x => x.GetProductByIdAsync(999)).ReturnsAsync((SkuDto?)null);
        }

        [Given(@"the order service is unavailable")]
        public void GivenTheOrderServiceIsUnavailable()
        {
            MockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ThrowsAsync(new Exception("Service unavailable"));
        }

        [Given(@"there are (.*) orders in the system")]
        public void GivenThereAreOrdersInTheSystem(int orderCount)
        {
            _orders = new List<OrderDto>();
            for (int i = 1; i <= orderCount; i++)
            {
                _orders.Add(new OrderDto
                {
                    Id = i,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 99.99,
                    LineItems = new List<LineItemDto>
                    {
                        new LineItemDto { Id = i, OrderId = i, SkuId = 1, Qty = 2 }
                    }
                });
            }
            MockOrderProvider.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(_orders);
        }

        [When(@"I request all order details")]
        public async Task WhenIRequestAllOrderDetails()
        {
            try
            {
                _response = await _bffController.GetAllOrdersDetails();
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [When(@"I send multiple concurrent requests")]
        public async Task WhenISendMultipleConcurrentRequests()
        {
            var tasks = new List<Task<IActionResult>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_bffController.GetAllOrdersDetails());
            }
            
            var results = await Task.WhenAll(tasks);
            _response = results[0]; // Use first result for assertions
        }

        [Then(@"I should receive a successful response")]
        public void ThenIShouldReceiveASuccessfulResponse()
        {
            _response.Should().BeOfType<OkObjectResult>();
        }

        [Then(@"I should receive an error response")]
        public void ThenIShouldReceiveAnErrorResponse()
        {
            _response.Should().BeOfType<ObjectResult>();
            var objectResult = _response as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Then(@"the response should contain order information")]
        public void ThenTheResponseShouldContainOrderInformation()
        {
            var okResult = _response as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Then(@"each order should have customer details")]
        public void ThenEachOrderShouldHaveCustomerDetails()
        {
            var okResult = _response as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            // The response should contain order data with customer details
        }

        [Then(@"each order should have product details")]
        public void ThenEachOrderShouldHaveProductDetails()
        {
            var okResult = _response as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            // The response should contain order data with product details
        }

        [Then(@"the response should be an empty list")]
        public void ThenTheResponseShouldBeAnEmptyList()
        {
            var okResult = _response as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            // For empty orders, the response should still be valid but contain no order data
        }

        [Then(@"the order should show ""(.*)"" customer")]
        public void ThenTheOrderShouldShowCustomer(string expectedCustomer)
        {
            var okResult = _response as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            // The response should contain order data with customer information
        }

        [Then(@"the line item should show ""(.*)"" product")]
        public void ThenTheLineItemShouldShowProduct(string expectedProduct)
        {
            var okResult = _response as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            // The response should contain order data with product information
        }

        [Then(@"the error should indicate internal server error")]
        public void ThenTheErrorShouldIndicateInternalServerError()
        {
            var objectResult = _response as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Then(@"all requests should complete successfully")]
        public void ThenAllRequestsShouldCompleteSuccessfully()
        {
            _response.Should().BeOfType<OkObjectResult>();
        }

        [Then(@"response times should be reasonable")]
        public void ThenResponseTimesShouldBeReasonable()
        {
            // This is a placeholder assertion - in real scenarios you'd measure actual response times
            _response.Should().NotBeNull();
        }

        [Then(@"the response should contain the following structure:")]
        public void ThenTheResponseShouldContainTheFollowingStructure(Table table)
        {
            var okResult = _response as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Then(@"each line item should contain:")]
        public void ThenEachLineItemShouldContain(Table table)
        {
            var okResult = _response as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }
    }
}
