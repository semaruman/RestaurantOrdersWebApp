namespace RestaurantOrdersWebApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        public HashSet<int> DishesId { get; set; }

        public string Status { get; set; }

        public List<Dish> GetDishes()
        {
            return new List<Dish>();
        }
    }
}
