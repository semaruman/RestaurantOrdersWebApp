namespace RestaurantOrders.Domain.Common;

public sealed class Rating : ValueObject
{
    public decimal Value { get; }

    private Rating(decimal value) => Value = value;

    public static Rating Create(decimal value)
    {
        if (value is < 1 or > 5)
            throw new DomainException("INVALID_RATING", "Rating must be between 1 and 5.");

        return new Rating(decimal.Round(value, 1));
    }

    public static Rating FromAverage(decimal average)
    {
        if (average is < 0 or > 5)
            throw new DomainException("INVALID_RATING", "Average rating is out of range.");

        return new Rating(decimal.Round(average, 1));
    }

    public static Rating Zero => new(0);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("0.0");
}
