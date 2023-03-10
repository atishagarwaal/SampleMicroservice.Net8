using AutoMapper;
using Retail.Api.Customers.Dto;
using Retail.Api.Customers.Interface;

namespace Retail.Api.Customers.Service
{
    /// <summary>
    /// Customer service class.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Method to fetch customer record based on Id.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Customer object.</returns>
        public CustomerDto GetCustomerById(long id)
        {
           var custObj = this._unitOfWork.CustomerRepository.GetById(id);
           var custDto = this._mapper.Map<CustomerDto>(custObj);

           return custDto;
        }
    }
}
