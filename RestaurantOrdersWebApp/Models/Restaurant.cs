using System.ComponentModel.DataAnnotations;

namespace RestaurantOrdersWebApp.Models
{
    public class Restaurant
    {
        public string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Contacts { get; set; } = string.Empty;

        public List<Review> Reviews { get; set; } = new List<Review>();

        public List<Dish> MenuDishes { get; set; } = new List<Dish>();

        public List<Order> Orders {  get; set; } = new List<Order>();
    }
}
