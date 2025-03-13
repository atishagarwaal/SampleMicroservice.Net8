using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.src.CleanArchitecture.Application.Commands;

namespace Retail.Orders.src.CleanArchitecture.Application.Handlers
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteOrderCommandHandler(IUnitOfWork unitOfWork, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var order = await unitOfWork.Orders.GetByIdAsync(request.Id);
                if (order == null)
                {
                    return false;
                }

                await unitOfWork.BeginTransactionAsync();
                unitOfWork.Orders.Remove(order);

                if (order.LineItems != null)
                {
                    foreach (var lineItem in order.LineItems)
                    {
                        unitOfWork.LineItems.Remove(lineItem);
                    }
                }

                await unitOfWork.CompleteAsync();
                await unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
