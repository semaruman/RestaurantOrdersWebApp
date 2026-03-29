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

        public void AddReview(Restaurant restaurantP, Review review)
        {
            using var dbContext = new ApplicationDbContext();
            var restaurant = dbContext.Restaurants.Find(restaurantP.Name);
            restaurant.Reviews.Add(review);
            dbContext.SaveChanges();
        }

        public void AddOrder(Restaurant restaurantP, Order order)
        {
            using var dbContext = new ApplicationDbContext();
            var restaurant = dbContext.Restaurants.Find(restaurantP.Name);

            restaurant.Orders.Add(order);
            dbContext.SaveChanges();
        }

        public void AddMenuDish(Restaurant restaurantP, Dish dish)
        {
            using var dbContext = new ApplicationDbContext();
            var restaurant = dbContext.Restaurants.Find(restaurantP.Name);

            dish.Order = null;
            dish.OrderId = null;

            restaurant.MenuDishes.Add(dish);
            dbContext.SaveChanges();
        }

        public List<Dish> GetRestaurantMenu(string name)
        {
            using var dbContext = new ApplicationDbContext();

            return dbContext.Dishes.Where(d => d.RestaurantName == name && d.OrderId == null).ToList();
        }
    }
}
