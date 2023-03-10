using AutoMapper;
using Retail.Api.Customers.Dto;
using Retail.Api.Customers.Interface;

namespace Retail.Api.Customers.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public CustomerDto GetCustomerById(long id)
        {
           var custObj = this._unitOfWork.customerRepository.GetById(id);
           var custDto = this._mapper.Map<CustomerDto>(custObj);

           return custDto;
        }
    }
}
