using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Application.Favorites.Queries;

public sealed record FavoriteDto(Guid Id, Guid RestaurantId, DateTime CreatedAtUtc);

public sealed record GetFavoritesQuery(Guid UserId);
public sealed class GetFavoritesHandler(IFavoriteRepository favorites)
{
    public async Task<IReadOnlyList<FavoriteDto>> Handle(GetFavoritesQuery query, CancellationToken ct = default)
    {
        var list = await favorites.GetByUserAsync(UserId.From(query.UserId), ct);
        return list.Select(f => new FavoriteDto(f.Id.Value, f.RestaurantId.Value, f.CreatedAtUtc)).ToList();
    }
}
