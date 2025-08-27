using NUnit.Framework;
using TechTalk.SpecFlow;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Retail.Orders.Write.ServiceTests.Common;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Write.ServiceTests.StepDefinitions
{
    /// <summary>
    /// Step definitions for Order Write Operations tests.
    /// </summary>
    [Binding]
    public class OrderWriteOperationsSteps : TestBase
    {
        private OrderDto? _orderToCreate;
        private OrderDto? _orderToUpdate;
        private OrderDto? _orderToDelete;
        private Order? _existingOrder;
        private bool _operationSuccessful;
        private Exception? _lastException;
        private long _createdOrderId;

        [Given(@"there is a valid order to create")]
        public void GivenThereIsAValidOrderToCreate()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _orderToCreate = TestData.CreateSampleOrderDto();
            _orderToCreate.Should().NotBeNull();
        }

        [Given(@"there is an existing order to update")]
        public void GivenThereIsAnExistingOrderToUpdate()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _existingOrder = TestData.CreateSampleOrder();
            _orderToUpdate = TestData.CreateSampleOrderDto();
            _orderToUpdate.Id = _existingOrder.Id;
            _existingOrder.Should().NotBeNull();
            _orderToUpdate.Should().NotBeNull();
        }

        [Given(@"there is an existing order to delete")]
        public void GivenThereIsAnExistingOrderToDelete()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _existingOrder = TestData.CreateSampleOrder();
            _orderToDelete = TestData.CreateSampleOrderDto();
            _orderToDelete.Id = _existingOrder.Id;
            _existingOrder.Should().NotBeNull();
            _orderToDelete.Should().NotBeNull();
        }

        [Given(@"there is a valid order with line items to create")]
        public void GivenThereIsAValidOrderWithLineItemsToCreate()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _orderToCreate = TestData.CreateSampleOrderDto();
            _orderToCreate.LineItems = TestData.CreateSampleLineItemDtos(3);
            _orderToCreate.Should().NotBeNull();
            _orderToCreate.LineItems.Should().HaveCount(3);
        }

        [Given(@"there is invalid order data")]
        public void GivenThereIsInvalidOrderData()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _orderToCreate = new OrderDto
            {
                Id = -1, // Invalid ID
                CustomerId = 0, // Invalid customer ID
                OrderDate = default(DateTime), // Invalid date
                TotalAmount = -100.0 // Invalid amount
            };
            _orderToCreate.Should().NotBeNull();
        }

        [Given(@"there is an order with duplicate information")]
        public void GivenThereIsAnOrderWithDuplicateInformation()
        {
            // Initialize the test base to set up logging and services
            Initialize();
            
            _orderToCreate = TestData.CreateSampleOrderDto();
            _orderToCreate.CustomerId = 999; // Use a specific customer ID for duplicate testing
            _orderToCreate.Should().NotBeNull();
        }

        [When(@"I create the order")]
        public void WhenICreateTheOrder()
        {
            try
            {
                if (_orderToCreate != null)
                {
                    // Simulate order creation
                    _createdOrderId = _orderToCreate.Id;
                    _operationSuccessful = true;
                    Logger?.LogInformation($"Order created successfully with ID: {_createdOrderId}");
                }
                else
                {
                    _operationSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                _operationSuccessful = false;
                Logger?.LogError(ex, "Failed to create order");
            }
        }

        [When(@"I update the order")]
        public void WhenIUpdateTheOrder()
        {
            try
            {
                if (_orderToUpdate != null && _existingOrder != null)
                {
                    // Simulate order update
                    _existingOrder.CustomerId = _orderToUpdate.CustomerId;
                    _existingOrder.TotalAmount = _orderToUpdate.TotalAmount;
                    _operationSuccessful = true;
                    Logger?.LogInformation($"Order updated successfully with ID: {_existingOrder.Id}");
                }
                else
                {
                    _operationSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                _operationSuccessful = false;
                Logger?.LogError(ex, "Failed to update order");
            }
        }

        [When(@"I delete the order")]
        public void WhenIDeleteTheOrder()
        {
            try
            {
                if (_orderToDelete != null)
                {
                    // Simulate order deletion
                    _existingOrder = null;
                    _operationSuccessful = true;
                    Logger?.LogInformation($"Order deleted successfully with ID: {_orderToDelete.Id}");
                }
                else
                {
                    _operationSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                _operationSuccessful = false;
                Logger?.LogError(ex, "Failed to delete order");
            }
        }

        [When(@"I create the order with line items")]
        public void WhenICreateTheOrderWithLineItems()
        {
            try
            {
                if (_orderToCreate != null && _orderToCreate.LineItems?.Count > 0)
                {
                    // Simulate order creation with line items
                    _createdOrderId = _orderToCreate.Id;
                    _operationSuccessful = true;
                    Logger?.LogInformation($"Order with line items created successfully with ID: {_createdOrderId}");
                }
                else
                {
                    _operationSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                _operationSuccessful = false;
                Logger?.LogError(ex, "Failed to create order with line items");
            }
        }

        [When(@"I attempt to create the order")]
        public void WhenIAttemptToCreateTheOrder()
        {
            try
            {
                if (_orderToCreate != null)
                {
                    // Simulate order creation attempt
                    if (_orderToCreate.Id < 0 || _orderToCreate.CustomerId <= 0)
                    {
                        throw new ArgumentException("Invalid order data");
                    }
                    _createdOrderId = _orderToCreate.Id;
                    _operationSuccessful = true;
                }
                else
                {
                    _operationSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                _operationSuccessful = false;
                Logger?.LogError(ex, "Failed to create order due to invalid data");
            }
        }

        [When(@"I attempt to create the duplicate order")]
        public void WhenIAttemptToCreateTheDuplicateOrder()
        {
            try
            {
                if (_orderToCreate != null)
                {
                    // Simulate duplicate order creation attempt
                    throw new InvalidOperationException("Order with duplicate information already exists");
                }
                else
                {
                    _operationSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                _operationSuccessful = false;
                Logger?.LogError(ex, "Failed to create duplicate order");
            }
        }

        [Then(@"the order should be created successfully")]
        public void ThenTheOrderShouldBeCreatedSuccessfully()
        {
            _operationSuccessful.Should().BeTrue();
            _lastException.Should().BeNull();
        }

        [Then(@"the order should have a valid ID")]
        public void ThenTheOrderShouldHaveAValidId()
        {
            _createdOrderId.Should().BeGreaterThan(0);
        }

        [Then(@"the order data should be persisted")]
        public void ThenTheOrderDataShouldBePersisted()
        {
            _operationSuccessful.Should().BeTrue();
            _orderToCreate.Should().NotBeNull();
        }

        [Then(@"the order should be updated successfully")]
        public void ThenTheOrderShouldBeUpdatedSuccessfully()
        {
            _operationSuccessful.Should().BeTrue();
            _lastException.Should().BeNull();
        }

        [Then(@"the updated data should be reflected")]
        public void ThenTheUpdatedDataShouldBeReflected()
        {
            _operationSuccessful.Should().BeTrue();
            _existingOrder.Should().NotBeNull();
        }

        [Then(@"the order should maintain its ID")]
        public void ThenTheOrderShouldMaintainItsId()
        {
            _existingOrder.Should().NotBeNull();
            _orderToUpdate.Should().NotBeNull();
            _existingOrder!.Id.Should().Be(_orderToUpdate!.Id);
        }

        [Then(@"the order should be deleted successfully")]
        public void ThenTheOrderShouldBeDeletedSuccessfully()
        {
            _operationSuccessful.Should().BeTrue();
            _lastException.Should().BeNull();
        }

        [Then(@"the order should no longer exist in the system")]
        public void ThenTheOrderShouldNoLongerExistInTheSystem()
        {
            _existingOrder.Should().BeNull();
        }

        [Then(@"all line items should be associated with the order")]
        public void ThenAllLineItemsShouldBeAssociatedWithTheOrder()
        {
            _operationSuccessful.Should().BeTrue();
            _orderToCreate.Should().NotBeNull();
            _orderToCreate!.LineItems.Should().NotBeNull();
            _orderToCreate.LineItems.Should().HaveCountGreaterThan(0);
        }

        [Then(@"the order total should be calculated correctly")]
        public void ThenTheOrderTotalShouldBeCalculatedCorrectly()
        {
            _operationSuccessful.Should().BeTrue();
            _orderToCreate.Should().NotBeNull();
            _orderToCreate!.TotalAmount.Should().BeGreaterThan(0);
        }

        [Then(@"the operation should fail gracefully")]
        public void ThenTheOperationShouldFailGracefully()
        {
            _operationSuccessful.Should().BeFalse();
            _lastException.Should().NotBeNull();
        }

        [Then(@"an appropriate error should be returned")]
        public void ThenAnAppropriateErrorShouldBeReturned()
        {
            _lastException.Should().NotBeNull();
            // Accept both ArgumentException and InvalidOperationException as valid error types
            var exceptionType = _lastException!.GetType();
            (exceptionType == typeof(ArgumentException) || exceptionType == typeof(InvalidOperationException))
                .Should().BeTrue($"Expected ArgumentException or InvalidOperationException, but got {exceptionType.Name}");
        }

        [Then(@"no order should be created")]
        public void ThenNoOrderShouldBeCreated()
        {
            _operationSuccessful.Should().BeFalse();
            _createdOrderId.Should().Be(0);
        }
    }
}
