using Microsoft.EntityFrameworkCore;
using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Domain.Favorites;
using RestaurantOrders.Domain.Orders;
using RestaurantOrders.Domain.Reservations;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Reviews;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(AppDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default) =>
        db.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await db.Orders.AddAsync(order, ct);

    public async Task<IReadOnlyList<Order>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await db.Orders.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
}

internal sealed class ReservationRepository(AppDbContext db) : IReservationRepository
{
    public Task<Reservation?> GetByIdAsync(ReservationId id, CancellationToken ct = default) =>
        db.Reservations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Reservation reservation, CancellationToken ct = default) =>
        await db.Reservations.AddAsync(reservation, ct);

    public async Task<IReadOnlyList<Reservation>> GetByRestaurantAsync(
        RestaurantId restaurantId, DateTime dayUtc, CancellationToken ct = default)
    {
        var end = dayUtc.Date.AddDays(1);
        return await db.Reservations
            .Where(x => x.RestaurantId == restaurantId &&
                        x.ReservationDateTimeUtc >= dayUtc.Date &&
                        x.ReservationDateTimeUtc < end)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Reservation>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await db.Reservations.Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ReservationDateTimeUtc).ToListAsync(ct);
}

internal sealed class ReviewRepository(AppDbContext db) : IReviewRepository
{
    public Task<Review?> GetByIdAsync(ReviewId id, CancellationToken ct = default) =>
        db.Reviews.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Review review, CancellationToken ct = default) =>
        await db.Reviews.AddAsync(review, ct);

    public async Task<IReadOnlyList<Review>> GetPublishedByRestaurantAsync(
        RestaurantId restaurantId, CancellationToken ct = default) =>
        await db.Reviews.Where(x => x.RestaurantId == restaurantId && x.Status == ReviewStatus.Published)
            .OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);

    public async Task<(decimal Average, int Count)> GetPublishedStatsAsync(
        RestaurantId restaurantId, CancellationToken ct = default)
    {
        var ratings = await db.Reviews
            .Where(x => x.RestaurantId == restaurantId && x.Status == ReviewStatus.Published)
            .Select(x => x.Rating.Value).ToListAsync(ct);
        return ratings.Count == 0 ? (0, 0) : (ratings.Average(), ratings.Count);
    }
}

internal sealed class FavoriteRepository(AppDbContext db) : IFavoriteRepository
{
    public Task<Favorite?> GetAsync(UserId userId, RestaurantId restaurantId, CancellationToken ct = default) =>
        db.Favorites.FirstOrDefaultAsync(x => x.UserId == userId && x.RestaurantId == restaurantId, ct);

    public async Task AddAsync(Favorite favorite, CancellationToken ct = default) =>
        await db.Favorites.AddAsync(favorite, ct);

    public Task RemoveAsync(Favorite favorite, CancellationToken ct = default)
    {
        db.Favorites.Remove(favorite);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Favorite>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await db.Favorites.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
}

internal sealed class UserProfileRepository(AppDbContext db) : IUserProfileRepository
{
    public Task<UserProfile?> GetByIdAsync(UserId id, CancellationToken ct = default) =>
        db.UserProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(UserProfile profile, CancellationToken ct = default) =>
        await db.UserProfiles.AddAsync(profile, ct);
}
