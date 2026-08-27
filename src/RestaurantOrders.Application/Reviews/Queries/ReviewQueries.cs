using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Domain.Restaurants;

namespace RestaurantOrders.Application.Reviews.Queries;

public sealed record ReviewDto(Guid Id, Guid UserId, decimal Rating, string Comment, string Status, DateTime CreatedAtUtc);

public sealed record GetRestaurantReviewsQuery(Guid RestaurantId);
public sealed class GetRestaurantReviewsHandler(IReviewRepository reviews)
{
    public async Task<IReadOnlyList<ReviewDto>> Handle(GetRestaurantReviewsQuery query, CancellationToken ct = default)
    {
        var list = await reviews.GetPublishedByRestaurantAsync(RestaurantId.From(query.RestaurantId), ct);
        return list.Select(r => new ReviewDto(
            r.Id.Value,
            r.UserId.Value,
            r.Rating.Value,
            r.Comment,
            r.Status.ToString(),
            r.CreatedAtUtc)).ToList();
    }
}
