using Microsoft.EntityFrameworkCore;
using RestaurantOrdersWebApp.Models;

namespace RestaurantOrdersWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Restaurant> Restaurants { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Dish> Dishes {  get; set; }
    }
}
