using Retail.Api.Products.Dto;
using Retail.Api.Products.Model;

namespace Retail.Api.Products.Profiles
{
    public class ProductProfile : AutoMapper.Profile
    {
        public ProductProfile()
        {
            CreateMap<Sku, SkuDto>();
        }
    }
}
