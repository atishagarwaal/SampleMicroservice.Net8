using Retail.Api.Orders.Model;


namespace Retail.Api.Products.Profiles
{
    public class OrderProfile : AutoMapper.Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>();
            CreateMap<LineItem, LineItemDto>();
        }
    }
}
