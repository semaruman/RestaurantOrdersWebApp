using RestaurantOrders.Domain.Restaurants;

namespace RestaurantOrders.Application.Abstractions;

public sealed record RestaurantSearchFilter(
    string? Text = null,
    string? Cuisine = null,
    string? City = null,
    PriceCategory? PriceCategory = null,
    decimal? MinRating = null,
    string? Feature = null,
    bool? OpenNow = null,
    bool? AcceptsReservations = null,
    string? SortBy = null,
    int Page = 1,
    int PageSize = 12);

public sealed record RestaurantListItemDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string? City,
    string? CoverImageUrl,
    IReadOnlyList<string> Cuisines,
    string PriceCategory,
    decimal AverageRating,
    int ReviewCount,
    string Status,
    bool AcceptsReservations,
    bool IsOpenNow);

public sealed record RestaurantDetailsDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string Status,
    string PriceCategory,
    decimal AverageRating,
    int ReviewCount,
    string? CoverImageUrl,
    IReadOnlyList<string> PhotoUrls,
    IReadOnlyList<string> Cuisines,
    IReadOnlyList<string> Features,
    AddressDto? Address,
    ContactDto? Contacts,
    IReadOnlyList<OpeningHoursDto> OpeningHours,
    IReadOnlyList<MenuItemDto> Menu,
    bool AcceptsReservations,
    bool OffersDelivery,
    int? Capacity,
    bool IsOpenNow);

public sealed record AddressDto(string Street, string City, string? PostalCode, string Country, double? Latitude, double? Longitude);
public sealed record ContactDto(string Phone, string? Email, string? Website);
public sealed record OpeningHoursDto(string Day, string? Open, string? Close, bool IsClosed);
public sealed record MenuItemDto(Guid Id, string Name, string Description, string? Category, decimal Price, string Currency, string? PhotoUrl, bool IsAvailable, string? Ingredients);

public interface IRestaurantReadStore
{
    Task<Common.PagedResult<RestaurantListItemDto>> SearchAsync(RestaurantSearchFilter filter, CancellationToken ct = default);
    Task<RestaurantDetailsDto?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<RestaurantDetailsDto?> GetDetailsBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<RestaurantListItemDto>> GetFeaturedAsync(int take, CancellationToken ct = default);
}
