using RestaurantOrdersWebApp.Data;
using RestaurantOrdersWebApp.Models;

namespace RestaurantOrdersWebApp.Services.Interfaces
{
    public interface IRestaurantService
    {
        public List<Restaurant> GetAllRestaurants();

        public Restaurant GetRestaurantByName(string restaurantName);

        public bool AddRestaurant(Restaurant restaurant);
    }
}
