using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Domain.Favorites;

public sealed class Favorite : AggregateRoot
{
    public FavoriteId Id { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public RestaurantId RestaurantId { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private Favorite() { }

    public static Favorite Create(UserId userId, RestaurantId restaurantId)
    {
        return new Favorite
        {
            Id = FavoriteId.New(),
            UserId = userId,
            RestaurantId = restaurantId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
