namespace RestaurantOrders.Domain.Favorites;

public sealed class FavoriteId : Common.StronglyTypedId
{
    private FavoriteId(Guid value) : base(value) { }

    public static FavoriteId New() => new(Guid.NewGuid());

    public static FavoriteId From(Guid value) => new(value);
}
