namespace RestaurantOrdersWebApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        public List<int> DishesId { get; set; }

        public string Status { get; set; }
    }
}
