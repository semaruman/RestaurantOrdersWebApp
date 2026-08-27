using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;

namespace RestaurantOrders.Application.Restaurants.Queries;

public sealed record SearchRestaurantsQuery(RestaurantSearchFilter Filter);
public sealed class SearchRestaurantsHandler(IRestaurantReadStore readStore)
{
    public Task<PagedResult<RestaurantListItemDto>> Handle(SearchRestaurantsQuery query, CancellationToken ct = default)
        => readStore.SearchAsync(query.Filter, ct);
}

public sealed record GetRestaurantDetailsQuery(Guid? Id, string? Slug);
public sealed class GetRestaurantDetailsHandler(IRestaurantReadStore readStore)
{
    public async Task<Result<RestaurantDetailsDto>> Handle(GetRestaurantDetailsQuery query, CancellationToken ct = default)
    {
        RestaurantDetailsDto? dto = null;
        if (query.Id is Guid id)
            dto = await readStore.GetDetailsAsync(id, ct);
        else if (!string.IsNullOrWhiteSpace(query.Slug))
            dto = await readStore.GetDetailsBySlugAsync(query.Slug, ct);

        return dto is null
            ? Result.Failure<RestaurantDetailsDto>(new Error("RESTAURANT_NOT_FOUND", "Restaurant not found.", ErrorType.NotFound))
            : Result.Success(dto);
    }
}

public sealed record GetFeaturedRestaurantsQuery(int Take = 6);
public sealed class GetFeaturedRestaurantsHandler(IRestaurantReadStore readStore)
{
    public Task<IReadOnlyList<RestaurantListItemDto>> Handle(GetFeaturedRestaurantsQuery query, CancellationToken ct = default)
        => readStore.GetFeaturedAsync(query.Take, ct);
}
