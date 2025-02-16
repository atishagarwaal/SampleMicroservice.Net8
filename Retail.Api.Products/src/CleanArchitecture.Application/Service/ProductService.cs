using AutoMapper;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Api.Products.src.CleanArchitecture.Application.Service
{
    /// <summary>
    /// Product service class.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Method to fetch all products asynchronously.
        /// </summary>
        /// <returns>List of products.</returns>
        public async Task<IEnumerable<SkuDto>> GetAllProductsAsync()
        {
            var returnList = new List<SkuDto>();

            // Get all customers
            var list = await _unitOfWork.ProductRepository.GetAllAsync();

            // Transform data
            foreach (var item in list)
            {
                var custDto = _mapper.Map<SkuDto>(item);
                returnList.Add(custDto);
            }

            return returnList;
        }

        /// <summary>
        /// Method to fetch product record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> GetProductByIdAsync(long id)
        {
            // Find record
            var record = await _unitOfWork.ProductRepository.GetByIdAsync(id);

            // Transform data
            var custDto = _mapper.Map<SkuDto>(record);

            return custDto;
        }

        /// <summary>
        /// Method to add a new product record asynchronously.
        /// </summary>
        /// <param name="custDto">Product record.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> AddProductAsync(SkuDto custDto)
        {
            // Transform data
            var custObj = _mapper.Map<Sku>(custDto);

            // Add customer
            _unitOfWork.BeginTransaction();
            var result = await _unitOfWork.ProductRepository.AddAsync(custObj);
            _unitOfWork.Commit();

            // Transform data
            custDto = _mapper.Map<SkuDto>(result);

            return custDto;

        }

        /// <summary>
        /// Method to update product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <param name="custDto">Product record.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> UpdateProductAsync(long id, SkuDto custDto)
        {
            var record = _mapper.Map<Sku>(custDto);

            // Update record
            _unitOfWork.BeginTransaction();
            _unitOfWork.ProductRepository.Update(record);
            _unitOfWork.Commit();

            record = await _unitOfWork.ProductRepository.GetByIdAsync(id);

            // Transform data
            custDto = _mapper.Map<SkuDto>(record);

            return custDto;
        }

        /// <summary>
        /// Method to delete product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Product object.</returns>
        public async Task<bool> DeleteProductAsync(long id)
        {
            // Find record
            var record = await _unitOfWork.ProductRepository.GetByIdAsync(id);

            if (record != null)
            {
                // Delete record
                _unitOfWork.BeginTransaction();
                _unitOfWork.ProductRepository.Remove(record);
                _unitOfWork.Commit();

                return true;
            }

            return false;
        }
    }
}
