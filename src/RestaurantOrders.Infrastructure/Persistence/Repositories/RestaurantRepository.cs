using Microsoft.EntityFrameworkCore;
using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Domain.Restaurants;

namespace RestaurantOrders.Infrastructure.Persistence.Repositories;

internal sealed class RestaurantRepository(AppDbContext db) : IRestaurantRepository
{
    public Task<Restaurant?> GetByIdAsync(RestaurantId id, CancellationToken ct = default) =>
        db.Restaurants.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Restaurant?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var value = RestaurantSlug.Create(slug);
        return db.Restaurants.FirstOrDefaultAsync(x => x.Slug == value, ct);
    }

    public async Task AddAsync(Restaurant restaurant, CancellationToken ct = default) =>
        await db.Restaurants.AddAsync(restaurant, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
    {
        var value = RestaurantSlug.Create(slug);
        return db.Restaurants.AnyAsync(x => x.Slug == value, ct);
    }
}
