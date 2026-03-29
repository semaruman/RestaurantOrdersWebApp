using System.ComponentModel.DataAnnotations;

namespace RestaurantOrdersWebApp.Models
{
    public class Restaurant
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Contacts { get; set; }

        public double Rating
        {
            get => Reviews.Average(r => r.Rating);
        }

        public List<Review> Reviews { get; set; }

        public List<Dish> MenuDishes { get; set; }

        public List<Order> Orders {  get; set; }
    }
}
