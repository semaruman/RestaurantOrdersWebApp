using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Domain.Reviews;

public enum ReviewStatus
{
    Pending = 0,
    Published = 1,
    Rejected = 2,
    Hidden = 3
}

public sealed class ReviewSubmittedDomainEvent(ReviewId id, RestaurantId restaurantId) : IDomainEvent
{
    public ReviewId ReviewId { get; } = id;
    public RestaurantId RestaurantId { get; } = restaurantId;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed class ReviewPublishedDomainEvent(ReviewId id, RestaurantId restaurantId, Rating rating) : IDomainEvent
{
    public ReviewId ReviewId { get; } = id;
    public RestaurantId RestaurantId { get; } = restaurantId;
    public Rating Rating { get; } = rating;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed class Review : AggregateRoot
{
    public ReviewId Id { get; private set; } = null!;
    public RestaurantId RestaurantId { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public Rating Rating { get; private set; } = null!;
    public string Comment { get; private set; } = string.Empty;
    public ReviewStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Review() { }

    public static Review Submit(RestaurantId restaurantId, UserId userId, decimal rating, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment) || comment.Trim().Length < 5)
            throw new DomainException("INVALID_REVIEW", "Comment must be at least 5 characters.");

        var now = DateTime.UtcNow;
        var review = new Review
        {
            Id = ReviewId.New(),
            RestaurantId = restaurantId,
            UserId = userId,
            Rating = Rating.Create(rating),
            Comment = comment.Trim(),
            Status = ReviewStatus.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        review.Raise(new ReviewSubmittedDomainEvent(review.Id, restaurantId));
        return review;
    }

    public void Publish()
    {
        if (Status is ReviewStatus.Rejected or ReviewStatus.Hidden)
            throw new DomainException("INVALID_REVIEW_STATUS", $"Cannot publish review in status {Status}.");

        Status = ReviewStatus.Published;
        Touch();
        Raise(new ReviewPublishedDomainEvent(Id, RestaurantId, Rating));
    }

    public void Reject()
    {
        if (Status == ReviewStatus.Published)
            throw new DomainException("INVALID_REVIEW_STATUS", "Published review cannot be rejected. Hide it instead.");

        Status = ReviewStatus.Rejected;
        Touch();
    }

    public void Hide()
    {
        if (Status != ReviewStatus.Published)
            throw new DomainException("INVALID_REVIEW_STATUS", "Only published reviews can be hidden.");

        Status = ReviewStatus.Hidden;
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
