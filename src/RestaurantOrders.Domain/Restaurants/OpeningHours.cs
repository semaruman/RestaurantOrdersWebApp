using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Domain.Restaurants;

public sealed class OpeningHours : ValueObject
{
    public DayOfWeek Day { get; }
    public TimeOnly OpenTime { get; }
    public TimeOnly CloseTime { get; }
    public bool IsClosed { get; }

    private OpeningHours(DayOfWeek day, TimeOnly openTime, TimeOnly closeTime, bool isClosed)
    {
        Day = day;
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }

    public static OpeningHours Open(DayOfWeek day, TimeOnly open, TimeOnly close)
    {
        if (close <= open)
            throw new DomainException("INVALID_HOURS", "Close time must be after open time.");

        return new OpeningHours(day, open, close, false);
    }

    public static OpeningHours Closed(DayOfWeek day) =>
        new(day, TimeOnly.MinValue, TimeOnly.MinValue, true);

    public bool IsOpenAt(TimeOnly time) =>
        !IsClosed && time >= OpenTime && time < CloseTime;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Day;
        yield return OpenTime;
        yield return CloseTime;
        yield return IsClosed;
    }
}
