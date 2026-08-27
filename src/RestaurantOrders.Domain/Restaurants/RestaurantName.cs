using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Domain.Restaurants;

public sealed class RestaurantName : ValueObject
{
    public string Value { get; }

    private RestaurantName(string value) => Value = value;

    public static RestaurantName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("INVALID_RESTAURANT_NAME", "Restaurant name is required.");

        var trimmed = value.Trim();
        if (trimmed.Length is < 2 or > 120)
            throw new DomainException("INVALID_RESTAURANT_NAME", "Restaurant name must be 2-120 characters.");

        return new RestaurantName(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
