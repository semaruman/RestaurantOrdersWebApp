namespace RestaurantOrders.Domain.Reservations;

public sealed class ReservationId : Common.StronglyTypedId
{
    private ReservationId(Guid value) : base(value) { }

    public static ReservationId New() => new(Guid.NewGuid());

    public static ReservationId From(Guid value) => new(value);
}
