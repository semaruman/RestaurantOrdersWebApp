using RestaurantOrdersWebApp.Models;

namespace RestaurantOrdersWebApp.Services.Interfaces
{
    public interface IOrderService
    {
        public List<Order> GetAllOrders(string restaurantName);

        public List<Order> GetActiveOrders(string restaurantName);

        public Order GetOrderById(int id);

        public bool RemoveOrder(int id);

        public List<Order> GetOdersByIds(List<int> ids);
    }
}
