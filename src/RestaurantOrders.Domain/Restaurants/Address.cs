using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Domain.Restaurants;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string? PostalCode { get; }
    public string Country { get; }
    public double? Latitude { get; }
    public double? Longitude { get; }

    private Address(
        string street,
        string city,
        string? postalCode,
        string country,
        double? latitude,
        double? longitude)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Address Create(
        string street,
        string city,
        string? postalCode = null,
        string country = "Russia",
        double? latitude = null,
        double? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("INVALID_ADDRESS", "Street is required.");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("INVALID_ADDRESS", "City is required.");

        return new Address(street.Trim(), city.Trim(), postalCode?.Trim(), country.Trim(), latitude, longitude);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"{Street}, {City}";
}
