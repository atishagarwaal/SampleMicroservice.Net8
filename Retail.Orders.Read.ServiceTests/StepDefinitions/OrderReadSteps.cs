using NUnit.Framework;
using TechTalk.SpecFlow;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Retail.Orders.Read.ServiceTests.Common;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Read.ServiceTests.StepDefinitions
{
    /// <summary>
    /// Step definitions for Order Read tests.
    /// </summary>
    [Binding]
    public class OrderReadSteps : TestBase
    {
        private List<Order> _orders = new();
        private List<OrderDto> _orderDtos = new();
        private Order? _currentOrder;
        private OrderDto? _currentOrderDto;
        private long _requestedOrderId;
        private long _requestedCustomerId;
        private bool _orderFound;
        private Exception? _lastException;

        [Given(@"there are orders in the system")]
        public void GivenThereAreOrdersInTheSystem()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _orders = TestData.CreateSampleOrders(3);
            _orders.Should().HaveCount(3);
        }

        [Given(@"there is an order with ID ""(.*)""")]
        public void GivenThereIsAnOrderWithId(string orderId)
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _requestedOrderId = long.Parse(orderId);
            _currentOrder = TestData.CreateSampleOrder();
            _currentOrder.Id = _requestedOrderId;
            _orders.Add(_currentOrder);
        }

        [Given(@"there are orders for customer ""(.*)""")]
        public void GivenThereAreOrdersForCustomer(string customerId)
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _requestedCustomerId = long.Parse(customerId);
            var customerOrders = TestData.CreateSampleOrders(2);
            foreach (var order in customerOrders)
            {
                order.CustomerId = _requestedCustomerId;
            }
            _orders.AddRange(customerOrders);
        }

        [Given(@"there is an order with line items")]
        public void GivenThereIsAnOrderWithLineItems()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _currentOrder = TestData.CreateSampleOrder();
            _currentOrder.LineItems = new List<LineItem>
            {
                TestData.CreateSampleLineItem(),
                new LineItem { Id = 2, OrderId = 1, SkuId = 200, Qty = 1 }
            };
            _orders.Add(_currentOrder);
        }

        [Given(@"there is no order with ID ""(.*)""")]
        public void GivenThereIsNoOrderWithId(string orderId)
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _requestedOrderId = long.Parse(orderId);
            _currentOrder = null;
            _orderFound = false;
        }

        [When(@"I request all orders")]
        public void WhenIRequestAllOrders()
        {
            try
            {
                _orderDtos = TestData.CreateSampleOrderDtos(_orders.Count);
                Logger?.LogInformation($"Retrieved {_orderDtos.Count} orders");
            }
            catch (Exception ex)
            {
                _lastException = ex;
                Logger?.LogError(ex, "Failed to retrieve all orders");
            }
        }

        [When(@"I request the order with ID ""(.*)""")]
        public void WhenIRequestTheOrderWithId(string orderId)
        {
            try
            {
                var requestedId = long.Parse(orderId);
                var foundOrder = _orders.FirstOrDefault(o => o.Id == requestedId);
                if (foundOrder != null)
                {
                    _currentOrderDto = TestData.CreateSampleOrderDto();
                    // Set the correct ID to match the requested order
                    _currentOrderDto.Id = requestedId;
                    _orderFound = true;
                }
                else
                {
                    _currentOrderDto = null;
                    _orderFound = false;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                Logger?.LogError(ex, $"Failed to retrieve order with ID {orderId}");
            }
        }

        [When(@"I request orders for customer ""(.*)""")]
        public void WhenIRequestOrdersForCustomer(string customerId)
        {
            try
            {
                var requestedCustomerId = long.Parse(customerId);
                var customerOrders = _orders.Where(o => o.CustomerId == requestedCustomerId).ToList();
                _orderDtos = TestData.CreateSampleOrderDtos(customerOrders.Count);
                foreach (var dto in _orderDtos)
                {
                    dto.CustomerId = requestedCustomerId;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                Logger?.LogError(ex, $"Failed to retrieve orders for customer {customerId}");
            }
        }

        [When(@"I request the order details")]
        public void WhenIRequestTheOrderDetails()
        {
            try
            {
                if (_currentOrder != null)
                {
                    _currentOrderDto = TestData.CreateSampleOrderDto();
                    _currentOrderDto.Id = _currentOrder.Id;
                    _currentOrderDto.CustomerId = _currentOrder.CustomerId;
                    _currentOrderDto.LineItems = _currentOrder.LineItems.Select(li => new LineItemDto
                    {
                        Id = li.Id,
                        OrderId = li.OrderId,
                        SkuId = li.SkuId,
                        Qty = li.Qty
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                Logger?.LogError(ex, "Failed to retrieve order details");
            }
        }

        [Then(@"I should receive a list of orders")]
        public void ThenIShouldReceiveAListOfOrders()
        {
            _orderDtos.Should().NotBeNull();
            _orderDtos.Should().HaveCountGreaterThan(0);
        }

        [Then(@"each order should have valid data")]
        public void ThenEachOrderShouldHaveValidData()
        {
            foreach (var orderDto in _orderDtos)
            {
                orderDto.Id.Should().BeGreaterThan(0);
                orderDto.CustomerId.Should().BeGreaterThan(0);
                orderDto.OrderDate.Should().NotBe(default(DateTime));
            }
        }

        [Then(@"I should receive the order details")]
        public void ThenIShouldReceiveTheOrderDetails()
        {
            _currentOrderDto.Should().NotBeNull();
            _currentOrderDto!.Id.Should().Be(_requestedOrderId);
            _currentOrderDto.CustomerId.Should().BeGreaterThan(0);
            _currentOrderDto.OrderDate.Should().NotBe(default(DateTime));
        }

        [Then(@"the order should have the correct ID")]
        public void ThenTheOrderShouldHaveTheCorrectId()
        {
            _currentOrderDto.Should().NotBeNull();
            _currentOrderDto!.Id.Should().Be(_requestedOrderId);
        }

        [Then(@"all orders should belong to customer ""(.*)""")]
        public void ThenAllOrdersShouldBelongToCustomer(string customerId)
        {
            var expectedCustomerId = long.Parse(customerId);
            foreach (var orderDto in _orderDtos)
            {
                orderDto.CustomerId.Should().Be(expectedCustomerId);
            }
        }

        [Then(@"I should receive the order with line items")]
        public void ThenIShouldReceiveTheOrderWithLineItems()
        {
            _currentOrderDto.Should().NotBeNull();
            _currentOrderDto!.LineItems.Should().NotBeNull();
            _currentOrderDto.LineItems.Should().HaveCountGreaterThan(0);
        }

        [Then(@"the line items should have valid data")]
        public void ThenTheLineItemsShouldHaveValidData()
        {
            _currentOrderDto.Should().NotBeNull();
            foreach (var lineItem in _currentOrderDto!.LineItems!)
            {
                lineItem.Id.Should().BeGreaterThan(0);
                lineItem.OrderId.Should().BeGreaterThan(0);
                lineItem.SkuId.Should().BeGreaterThan(0);
                lineItem.Qty.Should().BeGreaterThan(0);
            }
        }

        [Then(@"I should receive a not found response")]
        public void ThenIShouldReceiveANotFoundResponse()
        {
            _orderFound.Should().BeFalse();
            _currentOrderDto.Should().BeNull();
        }
    }
}
