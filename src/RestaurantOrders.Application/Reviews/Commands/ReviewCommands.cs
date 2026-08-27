using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;
using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Reviews;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Application.Reviews.Commands;

public sealed record SubmitReviewCommand(Guid RestaurantId, Guid UserId, decimal Rating, string Comment);
public sealed class SubmitReviewHandler(
    IRestaurantRepository restaurants,
    IReviewRepository reviews,
    IUnitOfWork uow)
{
    public async Task<Result<Guid>> Handle(SubmitReviewCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var restaurant = await restaurants.GetByIdAsync(RestaurantId.From(cmd.RestaurantId), ct);
            if (restaurant is null)
                return Result.Failure<Guid>(new Error("RESTAURANT_NOT_FOUND", "Restaurant not found.", ErrorType.NotFound));

            if (restaurant.Status == RestaurantStatus.PermanentlyClosed)
                return Result.Failure<Guid>(new Error("RESTAURANT_CLOSED", "Cannot review a closed restaurant.", ErrorType.BusinessRule));

            var review = Review.Submit(restaurant.Id, UserId.From(cmd.UserId), cmd.Rating, cmd.Comment);
            // Auto-publish for better UX in demo; moderation still available
            review.Publish();

            await reviews.AddAsync(review, ct);

            var (avg, count) = await reviews.GetPublishedStatsAsync(restaurant.Id, ct);
            // include the new review in stats (not yet saved, so add manually)
            var newAvg = count == 0 ? cmd.Rating : ((avg * count) + cmd.Rating) / (count + 1);
            restaurant.ApplyReviewStats(newAvg, count + 1);

            await uow.SaveChangesAsync(ct);
            return Result.Success(review.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record ModerateReviewCommand(Guid ReviewId, string Action);
public sealed class ModerateReviewHandler(
    IReviewRepository reviews,
    IRestaurantRepository restaurants,
    IUnitOfWork uow)
{
    public async Task<Result> Handle(ModerateReviewCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var review = await reviews.GetByIdAsync(ReviewId.From(cmd.ReviewId), ct);
            if (review is null)
                return Result.Failure(new Error("REVIEW_NOT_FOUND", "Review not found.", ErrorType.NotFound));

            switch (cmd.Action.ToLowerInvariant())
            {
                case "publish": review.Publish(); break;
                case "reject": review.Reject(); break;
                case "hide": review.Hide(); break;
                default:
                    return Result.Failure(new Error("INVALID_ACTION", "Unknown moderation action.", ErrorType.Validation));
            }

            var restaurant = await restaurants.GetByIdAsync(review.RestaurantId, ct);
            if (restaurant is not null)
            {
                var (avg, count) = await reviews.GetPublishedStatsAsync(review.RestaurantId, ct);
                restaurant.ApplyReviewStats(avg, count);
            }

            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}
