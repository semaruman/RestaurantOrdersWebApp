using System.ComponentModel.DataAnnotations;

namespace RestaurantOrdersWebApp.Models
{
    public class Restaurant
    {
        public string Name { get; set; }

        public List<Dish> Dishes { get; set; }

        public List<Order> Orders {  get; set; }
    }
}
