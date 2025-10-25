using Ocelot.Middleware;

namespace Retail.BFFWeb.Api.Configurations
{
    public class OrderServiceConfig
    {
        public string BaseUrl { get; set; }
        public OrderEndpoints Endpoints { get; set; }
    }

    public class OrderEndpoints
    {
        public string GetAllOrdersV1 { get; set; }
        public string GetOrderByIdV1 { get; set; }
    }
}
