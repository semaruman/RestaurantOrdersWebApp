namespace RestaurantOrders.Domain.Orders;

public sealed class OrderId : Common.StronglyTypedId
{
    private OrderId(Guid value) : base(value) { }

    public static OrderId New() => new(Guid.NewGuid());

    public static OrderId From(Guid value) => new(value);
}
