namespace RestaurantOrders.Domain.Users;

public sealed class UserId : Common.StronglyTypedId
{
    private UserId(Guid value) : base(value) { }

    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value) => new(value);
}
