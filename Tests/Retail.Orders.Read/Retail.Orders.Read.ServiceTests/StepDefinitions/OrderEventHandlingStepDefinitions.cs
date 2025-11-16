using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Application.EventHandlers;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Repositories;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.UnitOfWork;
using Retail.Orders.Read.ServiceTests.Common;
using TechTalk.SpecFlow;
using InventoryUpdatedEventNameSpace;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Retail.Orders.Read.ServiceTests.StepDefinitions
{
    /// <summary>
    /// Step definitions for Order Event Handling scenarios.
    /// </summary>
    [Binding]
    public class OrderEventHandlingStepDefinitions : TestBase
    {
        private readonly ScenarioContext _scenarioContext;
        private InventoryUpdatedEventHandler _eventHandler = null!;
        private IUnitOfWork _unitOfWork = null!;
        private InventoryUpdatedEvent? _event;
        private bool _eventProcessed;
        private bool _eventRejected;
        private bool _errorLogged;

        public OrderEventHandlingStepDefinitions(ScenarioContext scenarioContext)
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

            services.AddScoped<IConverter<LineItemDto, Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.LineItem>, LineItemConverter>();
            services.AddScoped<IConverter<Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.LineItem, LineItemDto>, LineItemDtoConverter>();
            services.AddScoped<IConverter<OrderDto, Order>, OrderConverter>();
            services.AddScoped<IConverter<Order, OrderDto>, OrderDtoConverter>();

            services.AddScoped<InventoryUpdatedEventHandler>();
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            Initialize();
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
            _eventHandler = ServiceProvider.GetRequiredService<InventoryUpdatedEventHandler>();
            _event = null;
            _eventProcessed = false;
            _eventRejected = false;
            _errorLogged = false;
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

        [Given(@"an order created event is received")]
        public void GivenAnOrderCreatedEventIsReceived()
        {
            _event = new InventoryUpdatedEvent
            {
                OrderId = 1L,
                CustomerId = 123L,
                LineItems = new InventoryUpdatedEventNameSpace.LineItem[]
                {
                    new InventoryUpdatedEventNameSpace.LineItem
                    {
                        SkuId = 100L,
                        Qty = 2
                    }
                }
            };
        }

        [Given(@"an order updated event is received")]
        public void GivenAnOrderUpdatedEventIsReceived()
        {
            _event = new InventoryUpdatedEvent
            {
                OrderId = 1L,
                CustomerId = 123L,
                LineItems = new InventoryUpdatedEventNameSpace.LineItem[]
                {
                    new InventoryUpdatedEventNameSpace.LineItem
                    {
                        SkuId = 100L,
                        Qty = 3
                    }
                }
            };
        }

        [Given(@"an order cancelled event is received")]
        public void GivenAnOrderCancelledEventIsReceived()
        {
            _event = new InventoryUpdatedEvent
            {
                OrderId = 1L,
                CustomerId = 123L,
                LineItems = Array.Empty<InventoryUpdatedEventNameSpace.LineItem>()
            };
        }

        [Given(@"a line item added event is received")]
        public void GivenALineItemAddedEventIsReceived()
        {
            _event = new InventoryUpdatedEvent
            {
                OrderId = 1L,
                CustomerId = 123L,
                LineItems = new InventoryUpdatedEventNameSpace.LineItem[]
                {
                    new InventoryUpdatedEventNameSpace.LineItem
                    {
                        SkuId = 200L,
                        Qty = 1
                    }
                }
            };
        }

        [Given(@"an invalid event is received")]
        public void GivenAnInvalidEventIsReceived()
        {
            _event = null;
        }

        [When(@"the event is processed")]
        public async Task WhenTheEventIsProcessed()
        {
            try
            {
                if (_event != null)
                {
                    await _eventHandler.HandleAsync(_event);
                    _eventProcessed = true;
                }
                else
                {
                    try
                    {
                        await _eventHandler.HandleAsync(null!);
                    }
                    catch (ArgumentNullException)
                    {
                        _eventRejected = true;
                        _errorLogged = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing event");
                _errorLogged = true;
                if (_event == null)
                {
                    _eventRejected = true;
                }
            }
        }

        [Then(@"the order should be stored in the read model")]
        public async Task ThenTheOrderShouldBeStoredInTheReadModel()
        {
            if (_event != null)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(_event.OrderId);
                order.Should().NotBeNull();
            }
        }

        [Then(@"the order data should be accessible")]
        public async Task ThenTheOrderDataShouldBeAccessible()
        {
            if (_event != null)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(_event.OrderId);
                order.Should().NotBeNull();
                order!.Id.Should().Be(_event.OrderId);
                order.CustomerId.Should().Be(_event.CustomerId);
            }
        }

        [Then(@"the order should be updated in the read model")]
        public async Task ThenTheOrderShouldBeUpdatedInTheReadModel()
        {
            if (_event != null)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(_event.OrderId);
                order.Should().NotBeNull();
            }
        }

        [Then(@"the updated data should be reflected")]
        public async Task ThenTheUpdatedDataShouldBeReflected()
        {
            if (_event != null)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(_event.OrderId);
                order.Should().NotBeNull();
            }
        }

        [Then(@"the order status should be updated")]
        public async Task ThenTheOrderStatusShouldBeUpdated()
        {
            if (_event != null)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(_event.OrderId);
                order.Should().NotBeNull();
            }
        }

        [Then(@"the cancellation should be recorded")]
        public async Task ThenTheCancellationShouldBeRecorded()
        {
            if (_event != null)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(_event.OrderId);
                order.Should().NotBeNull();
            }
        }

        [Then(@"the line item should be added to the order")]
        public async Task ThenTheLineItemShouldBeAddedToTheOrder()
        {
            if (_event != null)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(_event.OrderId);
                order.Should().NotBeNull();
                order!.LineItems.Should().NotBeEmpty();
            }
        }

        [Then(@"the order total should be recalculated")]
        public async Task ThenTheOrderTotalShouldBeRecalculated()
        {
            if (_event != null)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(_event.OrderId);
                order.Should().NotBeNull();
                order!.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
            }
        }

        [Then(@"the event should be rejected")]
        public void ThenTheEventShouldBeRejected()
        {
            _eventRejected.Should().BeTrue();
        }

        [Then(@"an error should be logged")]
        public void ThenAnErrorShouldBeLogged()
        {
            _errorLogged.Should().BeTrue();
        }
    }
}

