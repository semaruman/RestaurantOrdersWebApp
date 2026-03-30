namespace RestaurantOrdersWebApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Status { get; set; } // готовится, готов

        public double Price
        {
            get => Dishes.Sum(d => d.Price);
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Restaurant Restaurant { get; set; }

        public List<Dish> Dishes { get; set; }
    }
}
