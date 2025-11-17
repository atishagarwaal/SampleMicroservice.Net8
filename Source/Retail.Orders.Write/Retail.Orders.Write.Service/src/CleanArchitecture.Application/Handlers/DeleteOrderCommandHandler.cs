using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Handlers
{
    /// <summary>
    /// Handler for DeleteOrderCommand.
    /// </summary>
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteOrderCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteOrderCommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="logger">Instance of logger.</param>
        public DeleteOrderCommandHandler(
            IUnitOfWork unitOfWork,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DeleteOrderCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Handles the DeleteOrderCommand request.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DeleteOrderCommand. OrderId: {OrderId}", request.Id);

            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var order = await unitOfWork.Orders.GetByIdAsync(request.Id);
                if (order == null)
                {
                    _logger.LogWarning("Order not found for deletion. OrderId: {OrderId}", request.Id);
                    return false;
                }

                _logger.LogDebug("Found order to delete. OrderId: {OrderId}, CustomerId: {CustomerId}, LineItemsCount: {LineItemsCount}",
                    order.Id, order.CustomerId, order.LineItems?.Count ?? 0);

                await unitOfWork.BeginTransactionAsync();
                unitOfWork.Orders.Remove(order);

                if (order.LineItems != null)
                {
                    _logger.LogDebug("Removing {LineItemsCount} line items for order", order.LineItems.Count);
                    foreach (var lineItem in order.LineItems)
                    {
                        unitOfWork.LineItems.Remove(lineItem);
                    }
                }

                await unitOfWork.CompleteAsync();
                await unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Order deleted successfully. OrderId: {OrderId}, CustomerId: {CustomerId}",
                    order.Id, order.CustomerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order. OrderId: {OrderId}", request.Id);
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
