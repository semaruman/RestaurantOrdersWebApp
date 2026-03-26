using System.ComponentModel.DataAnnotations;

namespace RestaurantOrdersWebApp.Models
{
    public class Restaurant
    {
        public string Name { get; set; }

        public List<Dish> MenuDishes { get; set; }

        public List<Order> Orders {  get; set; }
    }
}
