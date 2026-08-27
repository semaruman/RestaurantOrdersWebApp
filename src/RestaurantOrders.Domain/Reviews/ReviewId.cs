namespace RestaurantOrders.Domain.Reviews;

public sealed class ReviewId : Common.StronglyTypedId
{
    private ReviewId(Guid value) : base(value) { }

    public static ReviewId New() => new(Guid.NewGuid());

    public static ReviewId From(Guid value) => new(value);
}
