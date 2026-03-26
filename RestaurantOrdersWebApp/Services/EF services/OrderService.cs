using Microsoft.EntityFrameworkCore;
using RestaurantOrdersWebApp.Data;
using RestaurantOrdersWebApp.Models;
using RestaurantOrdersWebApp.Services.Interfaces;

namespace RestaurantOrdersWebApp.Services.EF_services
{
    public class OrderService : IOrderService
    {
        public List<Order> GetAllOrders(string restaurantName)
        {
            using var dbContext = new ApplicationDbContext();

            return dbContext.Orders.AsNoTracking().Where(o => o.Restaurant.Name == restaurantName).ToList();
        }

        public List<Order> GetActiveOrders(string restaurantName)
        {
            using var dbContext = new ApplicationDbContext();

            return dbContext.Orders.AsNoTracking().Where(o => o.Restaurant.Name == restaurantName).ToList();
        }

        public Order GetOrderById(int id)
        {
            using var dbContext = new ApplicationDbContext();

            return dbContext.Orders.AsNoTracking().FirstOrDefault(o => o.Id == id);
        }

        public bool AddOrder(Order order)
        {
            try
            {
                using var dbContext = new ApplicationDbContext();

                dbContext.Orders.Add(order);
                dbContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveOrder(int id)
        {
            using var dbContext = new ApplicationDbContext();
            Order order = dbContext.Orders.Find(id);
            if (order != null)
            {
                dbContext.Orders.Remove(order);
                dbContext.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
