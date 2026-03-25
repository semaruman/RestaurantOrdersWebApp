namespace RestaurantOrdersWebApp.Models
{
    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Ingredients { get; set; }

        public string Photo {  get; set; }

        public Restaurant Restaurant { get; set; }

        public Order Order { get; set; }
    }
}
