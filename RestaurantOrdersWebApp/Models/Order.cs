namespace RestaurantOrdersWebApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        public List<Dish> Dishes { get; set; }

        public string Status { get; set; }
    }
}
