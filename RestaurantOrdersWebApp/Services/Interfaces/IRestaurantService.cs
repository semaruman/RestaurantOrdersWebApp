using RestaurantOrdersWebApp.Data;
using RestaurantOrdersWebApp.Models;

namespace RestaurantOrdersWebApp.Services.Interfaces
{
    public interface IRestaurantService
    {
        public List<Restaurant> GetAllRestaurants();

        public Restaurant GetRestaurantByName(string restaurantName);

        public bool AddRestaurant(Restaurant restaurant);

        public void AddReview(Restaurant restaurantP, Review review);

        public void AddOrder(Restaurant restaurantP, Order order);

        public void AddMenuDish(Restaurant restaurantP, Dish dish);

        public List<Dish> GetRestaurantMenu(string name);

        public List<Review> GetRestaurantReviews(string name);
    }
}
