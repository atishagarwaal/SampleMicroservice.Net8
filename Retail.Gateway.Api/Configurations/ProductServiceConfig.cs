using Ocelot.Middleware;

namespace Retail.BFFWeb.Api.Configurations
{
    public class ProductServiceConfig
    {
        public string BaseUrl { get; set; }
        public ProductEndpoints Endpoints { get; set; }
    }

    public class ProductEndpoints
    {
        public string GetAllProductsV1 { get; set; }
        public string GetProductByIdV1 { get; set; }
    }
}
