using System.Text.RegularExpressions;
using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Domain.Restaurants;

public sealed class RestaurantSlug : ValueObject
{
    private static readonly Regex Pattern = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Value { get; }

    private RestaurantSlug(string value) => Value = value;

    public static RestaurantSlug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("INVALID_SLUG", "Slug is required.");

        var normalized = value.Trim().ToLowerInvariant().Replace(' ', '-');
        if (!Pattern.IsMatch(normalized) || normalized.Length > 80)
            throw new DomainException("INVALID_SLUG", "Slug format is invalid.");

        return new RestaurantSlug(normalized);
    }

    public static RestaurantSlug FromName(RestaurantName name)
    {
        var slug = new string(name.Value
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());

        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        slug = slug.Trim('-');
        if (string.IsNullOrEmpty(slug))
            slug = "restaurant";

        return Create(slug);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
