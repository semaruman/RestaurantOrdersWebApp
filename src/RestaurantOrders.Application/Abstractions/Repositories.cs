using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Orders;
using RestaurantOrders.Domain.Reservations;
using RestaurantOrders.Domain.Reviews;
using RestaurantOrders.Domain.Favorites;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Application.Abstractions;

public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(RestaurantId id, CancellationToken ct = default);
    Task<Restaurant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Restaurant restaurant, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByUserAsync(UserId userId, CancellationToken ct = default);
}

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(ReservationId id, CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetByRestaurantAsync(RestaurantId restaurantId, DateTime dayUtc, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetByUserAsync(UserId userId, CancellationToken ct = default);
}

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(ReviewId id, CancellationToken ct = default);
    Task AddAsync(Review review, CancellationToken ct = default);
    Task<IReadOnlyList<Review>> GetPublishedByRestaurantAsync(RestaurantId restaurantId, CancellationToken ct = default);
    Task<(decimal Average, int Count)> GetPublishedStatsAsync(RestaurantId restaurantId, CancellationToken ct = default);
}

public interface IFavoriteRepository
{
    Task<Favorite?> GetAsync(UserId userId, RestaurantId restaurantId, CancellationToken ct = default);
    Task AddAsync(Favorite favorite, CancellationToken ct = default);
    Task RemoveAsync(Favorite favorite, CancellationToken ct = default);
    Task<IReadOnlyList<Favorite>> GetByUserAsync(UserId userId, CancellationToken ct = default);
}

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task AddAsync(UserProfile profile, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
