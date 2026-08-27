using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Domain.Restaurants;

public sealed class RestaurantCreatedDomainEvent(RestaurantId restaurantId) : IDomainEvent
{
    public RestaurantId RestaurantId { get; } = restaurantId;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed class RestaurantPublishedDomainEvent(RestaurantId restaurantId) : IDomainEvent
{
    public RestaurantId RestaurantId { get; } = restaurantId;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed class RestaurantClosedDomainEvent(RestaurantId restaurantId, bool permanently) : IDomainEvent
{
    public RestaurantId RestaurantId { get; } = restaurantId;
    public bool Permanently { get; } = permanently;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
