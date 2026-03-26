using RestaurantOrdersWebApp.Services.Interfaces;

namespace RestaurantOrdersWebApp.Services
{
    public class RestaurantContext : IRestaurantContext
    {
        public string RestaurantName { get; set; }
    }
}
