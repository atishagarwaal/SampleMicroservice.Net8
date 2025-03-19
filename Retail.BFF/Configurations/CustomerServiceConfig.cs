using Ocelot.Middleware;

namespace Retail.BFFWeb.Api.Configurations
{
    public class CustomerServiceConfig
    {
        public string BaseUrl { get; set; }
        public CustomerEndpoints Endpoints { get; set; }
    }

    public class CustomerEndpoints
    {
        public string GetAllCustomersV1 { get; set; }
        public string GetCustomerByIdV1 { get; set; }
    }
}
