using RestaurantOrdersWebApp.Data;
using RestaurantOrdersWebApp.Models;
using RestaurantOrdersWebApp.Services.Interfaces;

namespace RestaurantOrdersWebApp.Services.EF_services
{
    public class RestaurantService : IRestaurantService
    {
        public List<Restaurant> GetAllRestaurants()
        {
            using var dbContext = new ApplicationDbContext();

            return dbContext.Restaurants.ToList();
        }

        public Restaurant GetRestaurantByName(string restaurantName)
        {
            using var dbContext = new ApplicationDbContext();

            //нахожу ресторан по первичному ключу
            var restaurant = dbContext.Restaurants.Find(restaurantName);

            return restaurant;
        }

        public bool AddRestaurant(Restaurant restaurant)
        {

            using var dbContext = new ApplicationDbContext();

            //если ресторан уже существует, то не добавляю
            if (dbContext.Restaurants.Find(restaurant.Name) != null)
            {
                return false;
            }
            else
            {
                dbContext.Restaurants.Add(restaurant);
                dbContext.SaveChanges();
                return true;
            }
        }

        public void UpdateRestaurant(Restaurant restaurantP)
        {
            using var dbContext = new ApplicationDbContext();

            var restaurant = dbContext.Restaurants.Find(restaurantP.Name);

            restaurant.Orders = restaurantP.Orders;
            restaurant.Dishes = restaurantP.Dishes;

            dbContext.SaveChanges();
        }
    }
}
