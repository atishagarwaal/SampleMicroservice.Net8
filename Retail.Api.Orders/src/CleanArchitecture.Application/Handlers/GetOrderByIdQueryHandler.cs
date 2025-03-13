using AutoMapper;
using MediatR;
using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.src.CleanArchitecture.Application.Queries;

namespace Retail.Orders.src.CleanArchitecture.Application.Handlers
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var order = await unitOfWork.Orders.GetByIdAsync(request.Id);
            return _mapper.Map<OrderDto>(order);
        }
    }
}
