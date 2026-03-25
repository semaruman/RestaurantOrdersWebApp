namespace RestaurantOrdersWebApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public Restaurant Restaurant { get; set; }

        public List<Dish> Dishes { get; set; }
    }
}
