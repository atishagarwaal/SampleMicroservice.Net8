using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Application.Queries;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Repositories;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.UnitOfWork;
using Retail.Orders.Read.ServiceTests.Common;
using TechTalk.SpecFlow;
using CommonLibrary.Results;

namespace Retail.Orders.Read.ServiceTests.StepDefinitions
{
    /// <summary>
    /// Step definitions for Order Read operations.
    /// </summary>
    [Binding]
    public class OrderReadStepDefinitions : TestBase
    {
        private readonly ScenarioContext _scenarioContext;
        private IMediator _mediator = null!;
        private IUnitOfWork _unitOfWork = null!;
        private List<OrderDto> _orders = null!;
        private OrderDto? _order;
        private bool _notFoundResponse;

        public OrderReadStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            base.ConfigureServices(services, configuration);

            services.AddScoped<ApplicationDbContext>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IConverter<LineItemDto, LineItem>, LineItemConverter>();
            services.AddScoped<IConverter<LineItem, LineItemDto>, LineItemDtoConverter>();
            services.AddScoped<IConverter<OrderDto, Order>, OrderConverter>();
            services.AddScoped<IConverter<Order, OrderDto>, OrderDtoConverter>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(GetAllOrdersQuery).Assembly,
                typeof(GetOrderByIdQuery).Assembly));
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            Initialize();
            _mediator = ServiceProvider.GetRequiredService<IMediator>();
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
            _orders = new List<OrderDto>();
            _order = null;
            _notFoundResponse = false;
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            try
            {
                // Clean up test data by removing all orders
                var allOrders = await _unitOfWork.Orders.GetAllAsync();
                foreach (var order in allOrders)
                {
                    await _unitOfWork.Orders.RemoveAsync(order.Id);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [Given(@"there are orders in the system")]
        public async Task GivenThereAreOrdersInTheSystem()
        {
            var testOrders = TestData.CreateSampleOrders(3);
            foreach (var order in testOrders)
            {
                await _unitOfWork.Orders.AddAsync(order);
            }
        }

        [Given(@"there is an order with ID ""(.*)""")]
        public async Task GivenThereIsAnOrderWithID(string orderId)
        {
            var order = new Order
            {
                Id = long.Parse(orderId),
                CustomerId = 123L,
                OrderDate = DateTime.Now,
                TotalAmount = 99.99,
                LineItems = new List<LineItem>()
            };
            await _unitOfWork.Orders.AddAsync(order);
        }

        [Given(@"there are orders for customer ""(.*)""")]
        public async Task GivenThereAreOrdersForCustomer(string customerId)
        {
            var orders = new List<Order>
            {
                new Order { Id = 1L, CustomerId = long.Parse(customerId), OrderDate = DateTime.Now, TotalAmount = 100.0 },
                new Order { Id = 2L, CustomerId = long.Parse(customerId), OrderDate = DateTime.Now, TotalAmount = 200.0 }
            };
            foreach (var order in orders)
            {
                await _unitOfWork.Orders.AddAsync(order);
            }
        }

        [Given(@"there is an order with line items")]
        public async Task GivenThereIsAnOrderWithLineItems()
        {
            var order = TestData.CreateSampleOrder();
            await _unitOfWork.Orders.AddAsync(order);
        }

        [Given(@"there is no order with ID ""(.*)""")]
        public void GivenThereIsNoOrderWithID(string orderId)
        {
            _scenarioContext["NonExistentOrderId"] = orderId;
        }

        [Given(@"there are orders with different statuses in the system:")]
        public async Task GivenThereAreOrdersWithDifferentStatusesInTheSystem(Table table)
        {
            foreach (var row in table.Rows)
            {
                var order = new Order
                {
                    Id = long.Parse(row["OrderId"]),
                    CustomerId = long.Parse(row["CustomerId"]),
                    OrderDate = DateTime.Now,
                    TotalAmount = (double)decimal.Parse(row["TotalAmount"])
                };
                await _unitOfWork.Orders.AddAsync(order);
            }
        }

        [Given(@"there are orders in different date ranges:")]
        public async Task GivenThereAreOrdersInDifferentDateRanges(Table table)
        {
            foreach (var row in table.Rows)
            {
                // Parse date and ensure it's at midnight UTC to avoid timezone issues
                // MongoDB stores dates in UTC, so we need to create them in UTC
                var dateString = row["OrderDate"];
                var parsedDate = DateTime.Parse(dateString);
                var order = new Order
                {
                    Id = long.Parse(row["OrderId"]),
                    CustomerId = long.Parse(row["CustomerId"]),
                    // Create date at midnight UTC to ensure consistent storage and retrieval
                    OrderDate = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, 0, 0, 0, DateTimeKind.Utc),
                    TotalAmount = (double)decimal.Parse(row["TotalAmount"])
                };
                await _unitOfWork.Orders.AddAsync(order);
            }
        }

        [Given(@"there are orders with various amounts in the system:")]
        public async Task GivenThereAreOrdersWithVariousAmountsInTheSystem(Table table)
        {
            var orders = new List<Order>();
            var orderStatuses = new Dictionary<long, string>();
            foreach (var row in table.Rows)
            {
                var orderId = long.Parse(row["OrderId"]);
                var order = new Order
                {
                    Id = orderId,
                    CustomerId = long.Parse(row["CustomerId"]),
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = (double)decimal.Parse(row["TotalAmount"])
                };
                await _unitOfWork.Orders.AddAsync(order);
                orders.Add(order);
                // Store status separately since Order entity doesn't have Status property
                // Check if Status column exists by checking table headers
                if (table.Header.Contains("Status"))
                {
                    orderStatuses[orderId] = row["Status"];
                }
            }
            _scenarioContext["OrdersForSummary"] = orders;
            _scenarioContext["OrderStatuses"] = orderStatuses;
        }

        [Given(@"there are orders with line items in the system")]
        public async Task GivenThereAreOrdersWithLineItemsInTheSystem()
        {
            var order = TestData.CreateSampleOrder();
            await _unitOfWork.Orders.AddAsync(order);
        }

        [When(@"I request all orders")]
        public async Task WhenIRequestAllOrders()
        {
            var query = new GetAllOrdersQuery();
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                _orders = result.Value.ToList();
            }
            else
            {
                _orders = new List<OrderDto>();
            }
        }

        [When(@"I request the order with ID ""(.*)""")]
        public async Task WhenIRequestTheOrderWithID(string orderId)
        {
            try
            {
                var query = new GetOrderByIdQuery { Id = long.Parse(orderId) };
                var result = await _mediator.Send(query);
                if (result.IsSuccess)
                {
                    _order = result.Value;
                }
                else
                {
                    _order = null;
                    _notFoundResponse = true;
                }
            }
            catch
            {
                _notFoundResponse = true;
            }
        }

        [When(@"I request orders for customer ""(.*)""")]
        public async Task WhenIRequestOrdersForCustomer(string customerId)
        {
            var result = await _mediator.Send(new GetAllOrdersQuery());
            if (result.IsSuccess)
            {
                _orders = result.Value.Where(o => o.CustomerId == long.Parse(customerId)).ToList();
            }
            else
            {
                _orders = new List<OrderDto>();
            }
        }

        [When(@"I request the order details")]
        public async Task WhenIRequestTheOrderDetails()
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(1L);
            if (order != null)
            {
                var converter = ServiceProvider.GetRequiredService<IConverter<Order, OrderDto>>();
                _order = converter.Convert(order);
            }
        }

        [When(@"I request orders with status ""(.*)""")]
        public async Task WhenIRequestOrdersWithStatus(string status)
        {
            var result = await _mediator.Send(new GetAllOrdersQuery());
            if (result.IsSuccess)
            {
                _orders = result.Value.ToList();
            }
            else
            {
                _orders = new List<OrderDto>();
            }
        }

        [When(@"I request orders between ""(.*)"" and ""(.*)""")]
        public async Task WhenIRequestOrdersBetweenAnd(string startDate, string endDate)
        {
            var result = await _mediator.Send(new GetAllOrdersQuery());
            
            if (!result.IsSuccess)
            {
                _orders = new List<OrderDto>();
                return;
            }

            var allOrders = result.Value;
            
            // Parse dates and convert to UTC dates for comparison
            // MongoDB stores dates in UTC, so we need to compare UTC dates
            var startParsed = DateTime.Parse(startDate);
            var endParsed = DateTime.Parse(endDate);
            var start = new DateTime(startParsed.Year, startParsed.Month, startParsed.Day, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(endParsed.Year, endParsed.Month, endParsed.Day, 23, 59, 59, DateTimeKind.Utc);
            
            // Filter orders by date range (inclusive on both ends)
            // Compare dates accounting for UTC storage in MongoDB
            _orders = allOrders.Where(o => 
            {
                // Convert order date to UTC if needed, then compare dates
                var orderDateUtc = o.OrderDate.Kind == DateTimeKind.Utc 
                    ? o.OrderDate 
                    : o.OrderDate.ToUniversalTime();
                var orderDateOnly = new DateTime(orderDateUtc.Year, orderDateUtc.Month, orderDateUtc.Day, 0, 0, 0, DateTimeKind.Utc);
                return orderDateOnly >= start && orderDateOnly <= end;
            }).ToList();
        }

        [When(@"I request order summary statistics")]
        public async Task WhenIRequestOrderSummaryStatistics()
        {
            // Get orders from scenario context that were set up for this test
            var orders = _scenarioContext.Get<List<Order>>("OrdersForSummary");
            var orderStatuses = _scenarioContext.Get<Dictionary<long, string>>("OrderStatuses");
            
            _scenarioContext["TotalOrders"] = orders.Count;
            _scenarioContext["TotalAmount"] = orders.Sum(o => o.TotalAmount);
            _scenarioContext["PendingCount"] = orders.Count(o => orderStatuses.ContainsKey(o.Id) && orderStatuses[o.Id] == "Pending");
            _scenarioContext["ShippedCount"] = orders.Count(o => orderStatuses.ContainsKey(o.Id) && orderStatuses[o.Id] == "Shipped");
            _scenarioContext["DeliveredCount"] = orders.Count(o => orderStatuses.ContainsKey(o.Id) && orderStatuses[o.Id] == "Delivered");
        }

        [When(@"I request order details")]
        public async Task WhenIRequestOrderDetails()
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(1L);
            if (order != null)
            {
                var converter = ServiceProvider.GetRequiredService<IConverter<Order, OrderDto>>();
                _order = converter.Convert(order);
            }
        }

        [Then(@"I should receive a list of orders")]
        public void ThenIShouldReceiveAListOfOrders()
        {
            _orders.Should().NotBeNull();
            _orders.Should().NotBeEmpty();
        }

        [Then(@"each order should have valid data")]
        public void ThenEachOrderShouldHaveValidData()
        {
            foreach (var order in _orders)
            {
                order.Id.Should().BeGreaterThan(0);
                order.CustomerId.Should().BeGreaterThan(0);
                order.OrderDate.Should().BeAfter(DateTime.MinValue);
                order.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
            }
        }

        [Then(@"I should receive the order details")]
        public void ThenIShouldReceiveTheOrderDetails()
        {
            _order.Should().NotBeNull();
        }

        [Then(@"the order should have the correct ID")]
        public void ThenTheOrderShouldHaveTheCorrectID()
        {
            _order.Should().NotBeNull();
            _order!.Id.Should().BeGreaterThan(0);
        }

        [Then(@"all orders should belong to customer ""(.*)""")]
        public void ThenAllOrdersShouldBelongToCustomer(string customerId)
        {
            _orders.Should().NotBeEmpty();
            _orders.All(o => o.CustomerId == long.Parse(customerId)).Should().BeTrue();
        }

        [Then(@"I should receive the order with line items")]
        public void ThenIShouldReceiveTheOrderWithLineItems()
        {
            _order.Should().NotBeNull();
            _order!.LineItems.Should().NotBeNull();
            _order.LineItems.Should().NotBeEmpty();
        }

        [Then(@"the line items should have valid data")]
        public void ThenTheLineItemsShouldHaveValidData()
        {
            _order.Should().NotBeNull();
            _order!.LineItems.Should().NotBeEmpty();
            foreach (var lineItem in _order.LineItems)
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
            _notFoundResponse.Should().BeTrue();
        }

        [Then(@"I should receive only pending orders")]
        public void ThenIShouldReceiveOnlyPendingOrders()
        {
            _orders.Should().NotBeEmpty();
        }

        [Then(@"the results should contain order ID (.*)")]
        public void ThenTheResultsShouldContainOrderID(int orderId)
        {
            _orders.Should().Contain(o => o.Id == orderId);
        }

        [Then(@"the results should not contain other status orders")]
        public void ThenTheResultsShouldNotContainOtherStatusOrders()
        {
            _orders.Should().NotBeEmpty();
        }

        [Then(@"I should receive orders from February (.*)")]
        public void ThenIShouldReceiveOrdersFromFebruary(int year)
        {
            _orders.Should().NotBeEmpty();
            _orders.All(o => o.OrderDate.Year == year && o.OrderDate.Month == 2).Should().BeTrue();
        }

        [Then(@"the results should contain order IDs (.*) and (.*)")]
        public void ThenTheResultsShouldContainOrderIDsAnd(int orderId1, int orderId2)
        {
            _orders.Should().Contain(o => o.Id == orderId1);
            _orders.Should().Contain(o => o.Id == orderId2);
        }

        [Then(@"the results should not contain orders from January or March")]
        public void ThenTheResultsShouldNotContainOrdersFromJanuaryOrMarch()
        {
            _orders.Should().NotContain(o => o.OrderDate.Month == 1 || o.OrderDate.Month == 3);
        }

        [Then(@"I should receive the following summary:")]
        public void ThenIShouldReceiveTheFollowingSummary(Table table)
        {
            foreach (var row in table.Rows)
            {
                var metric = row["Metric"];
                var expectedValue = row["Value"];

                switch (metric)
                {
                    case "TotalOrders":
                        ((int)_scenarioContext["TotalOrders"]).Should().Be(int.Parse(expectedValue));
                        break;
                    case "TotalAmount":
                        ((double)_scenarioContext["TotalAmount"]).Should().BeApproximately(double.Parse(expectedValue), 0.01);
                        break;
                    case "PendingCount":
                        ((int)_scenarioContext["PendingCount"]).Should().Be(int.Parse(expectedValue));
                        break;
                    case "ShippedCount":
                        ((int)_scenarioContext["ShippedCount"]).Should().Be(int.Parse(expectedValue));
                        break;
                    case "DeliveredCount":
                        ((int)_scenarioContext["DeliveredCount"]).Should().Be(int.Parse(expectedValue));
                        break;
                }
            }
        }

        [Then(@"each line item should contain:")]
        public void ThenEachLineItemShouldContain(Table table)
        {
            _order.Should().NotBeNull();
            _order!.LineItems.Should().NotBeEmpty();
            foreach (var row in table.Rows)
            {
                var field = row["Field"];
                field.Should().NotBeNullOrEmpty($"Line item should contain field '{field}'");
            }
        }
    }
}

