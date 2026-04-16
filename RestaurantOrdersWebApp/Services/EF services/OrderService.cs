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

        public List<Order> GetOdersByIds(List<int> ids)
        {
            using var dbContext = new ApplicationDbContext();
            HashSet<int> idsSet = ids.ToHashSet();

            var orders = dbContext.Orders.Where(o => idsSet.Contains(o.Id))
                .Select(o => new Order
                {
                    Id = o.Id,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate,
                    //RestaurantName = o.Restaurant.Name,
                    Dishes = o.Dishes.Select(d => new Dish
                    {
                        Name = d.Name,
                        Photo = d.Photo,
                        Price = d.Price,
                    }).ToList(),
                }).ToList();

            return orders;
        }
    }
}
