using AutoMapper;
using Retail.Api.Customers.Dto;
using Retail.Api.Customers.Interface;

namespace Retail.Api.Customers.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;

        public CustomerService(ICustomerRepository customerRepository, IMapper mapper)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
        }

        public CustomerDto GetCustomerById(int id)
        {
           var custObj = this._customerRepository.GetById(id);
           var custDto = this._mapper.Map<CustomerDto>(custObj);

           return custDto;
        }
    }
}
