namespace RestaurantOrders.Domain.Restaurants;

public sealed class RestaurantId : Common.StronglyTypedId
{
    private RestaurantId(Guid value) : base(value) { }

    public static RestaurantId New() => new(Guid.NewGuid());

    public static RestaurantId From(Guid value) => new(value);
}
