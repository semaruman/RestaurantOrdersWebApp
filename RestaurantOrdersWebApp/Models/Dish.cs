using System.Text.Json.Serialization;

namespace RestaurantOrdersWebApp.Models
{
    public class Dish
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("ingredients")]
        public string Ingredients { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        public string RestaurantName { get; set; }
        public Restaurant Restaurant { get; set; }

        public int? OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
