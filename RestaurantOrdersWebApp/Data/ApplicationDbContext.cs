using Microsoft.EntityFrameworkCore;
using RestaurantOrdersWebApp.Models;

namespace RestaurantOrdersWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Restaurant> Restaurants { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Dish> Dishes { get; set; }

        public DbSet<Review> Reviews { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Restaurant>(entity =>
            {
                entity.HasKey(r => r.Name);
                entity.ToTable("restaurants");
            });
            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("orders");
            });
            modelBuilder.Entity<Dish>(e =>
            {
                e.ToTable("dishes");
            });
            modelBuilder.Entity<Review>(e =>
            {
                e.ToTable("reviews");
            });
        }
    }
}
