namespace RestaurantOrdersWebApp.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public double Rating { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
