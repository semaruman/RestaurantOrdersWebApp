using Microsoft.EntityFrameworkCore;
using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;
using RestaurantOrders.Domain.Restaurants;

namespace RestaurantOrders.Infrastructure.Persistence;

internal sealed class RestaurantReadStore(AppDbContext db) : IRestaurantReadStore
{
    public async Task<PagedResult<RestaurantListItemDto>> SearchAsync(
        RestaurantSearchFilter filter, CancellationToken ct = default)
    {
        var restaurants = await db.Restaurants.AsNoTracking()
            .Where(x => x.Status == RestaurantStatus.Published)
            .ToListAsync(ct);

        IEnumerable<Restaurant> query = restaurants;
        if (!string.IsNullOrWhiteSpace(filter.Text))
        {
            var text = filter.Text.Trim();
            query = query.Where(x =>
                x.Name.Value.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                x.Description.Contains(text, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(filter.Cuisine))
            query = query.Where(x => x.CuisineTypes.Contains(filter.Cuisine, StringComparer.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(x => string.Equals(x.Address?.City, filter.City, StringComparison.OrdinalIgnoreCase));
        if (filter.PriceCategory is not null)
            query = query.Where(x => x.PriceCategory == filter.PriceCategory);
        if (filter.MinRating is not null)
            query = query.Where(x => x.AverageRating.Value >= filter.MinRating);
        if (!string.IsNullOrWhiteSpace(filter.Feature))
            query = query.Where(x => x.Features.Contains(filter.Feature, StringComparer.OrdinalIgnoreCase));
        if (filter.AcceptsReservations is not null)
            query = query.Where(x => x.AcceptsReservations == filter.AcceptsReservations);
        if (filter.OpenNow == true)
            query = query.Where(x => x.IsOpenAt(DateTime.UtcNow));

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.OrderBy(x => x.Name.Value),
            "newest" => query.OrderByDescending(x => x.CreatedAtUtc),
            _ => query.OrderByDescending(x => x.AverageRating.Value).ThenByDescending(x => x.ReviewCount)
        };

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);
        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).Select(ToListItem).ToList();
        return new PagedResult<RestaurantListItemDto>(items, page, pageSize, total);
    }

    public async Task<RestaurantDetailsDto?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var restaurant = await db.Restaurants.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == RestaurantId.From(id), ct);
        return restaurant is null ? null : ToDetails(restaurant);
    }

    public async Task<RestaurantDetailsDto?> GetDetailsBySlugAsync(string slug, CancellationToken ct = default)
    {
        var value = RestaurantSlug.Create(slug);
        var restaurant = await db.Restaurants.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == value, ct);
        return restaurant is null ? null : ToDetails(restaurant);
    }

    public async Task<IReadOnlyList<RestaurantListItemDto>> GetFeaturedAsync(
        int take, CancellationToken ct = default)
    {
        var restaurants = await db.Restaurants.AsNoTracking()
            .Where(x => x.Status == RestaurantStatus.Published)
            .OrderByDescending(x => x.ReviewCount)
            .Take(Math.Clamp(take, 1, 20))
            .ToListAsync(ct);
        return restaurants.OrderByDescending(x => x.AverageRating.Value).Select(ToListItem).ToList();
    }

    private static RestaurantListItemDto ToListItem(Restaurant x) => new(
        x.Id.Value, x.Name.Value, x.Slug.Value, x.Description, x.Address?.City, x.CoverImageUrl,
        x.CuisineTypes.ToList(), x.PriceCategory.ToString(), x.AverageRating.Value, x.ReviewCount,
        x.Status.ToString(), x.AcceptsReservations, x.IsOpenAt(DateTime.UtcNow));

    private static RestaurantDetailsDto ToDetails(Restaurant x) => new(
        x.Id.Value, x.Name.Value, x.Slug.Value, x.Description, x.Status.ToString(),
        x.PriceCategory.ToString(), x.AverageRating.Value, x.ReviewCount, x.CoverImageUrl,
        x.PhotoUrls.ToList(), x.CuisineTypes.ToList(), x.Features.ToList(),
        x.Address is null ? null : new AddressDto(x.Address.Street, x.Address.City, x.Address.PostalCode,
            x.Address.Country, x.Address.Latitude, x.Address.Longitude),
        x.Contacts is null ? null : new ContactDto(x.Contacts.Phone.Value, x.Contacts.Email?.Value,
            x.Contacts.Website),
        x.OpeningHours.Select(h => new OpeningHoursDto(h.Day.ToString(),
            h.IsClosed ? null : h.OpenTime.ToString("HH:mm"),
            h.IsClosed ? null : h.CloseTime.ToString("HH:mm"), h.IsClosed)).ToList(),
        x.MenuItems.Select(m => new MenuItemDto(m.Id.Value, m.Name, m.Description, m.Category,
            m.Price.Amount, m.Price.Currency, m.PhotoUrl, m.IsAvailable, m.Ingredients)).ToList(),
        x.AcceptsReservations, x.OffersDelivery, x.Capacity, x.IsOpenAt(DateTime.UtcNow));
}
