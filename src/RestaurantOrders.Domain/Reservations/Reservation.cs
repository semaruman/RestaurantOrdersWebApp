using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Domain.Reservations;

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    NoShow = 4
}

public sealed class ReservationCreatedDomainEvent(ReservationId id, RestaurantId restaurantId) : IDomainEvent
{
    public ReservationId ReservationId { get; } = id;
    public RestaurantId RestaurantId { get; } = restaurantId;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed class ReservationConfirmedDomainEvent(ReservationId id) : IDomainEvent
{
    public ReservationId ReservationId { get; } = id;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed class ReservationCancelledDomainEvent(ReservationId id) : IDomainEvent
{
    public ReservationId ReservationId { get; } = id;
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed class Reservation : AggregateRoot
{
    public ReservationId Id { get; private set; } = null!;
    public RestaurantId RestaurantId { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public DateTime ReservationDateTimeUtc { get; private set; }
    public int GuestCount { get; private set; }
    public ReservationStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private Reservation() { }

    public static Reservation Create(
        Restaurant restaurant,
        UserId userId,
        DateTime reservationDateTimeUtc,
        int guestCount,
        string? notes = null)
    {
        if (!restaurant.CanAcceptReservation(reservationDateTimeUtc, guestCount))
            throw new DomainException("RESERVATION_NOT_ALLOWED", "Restaurant cannot accept this reservation.");

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            Id = ReservationId.New(),
            RestaurantId = restaurant.Id,
            UserId = userId,
            ReservationDateTimeUtc = reservationDateTimeUtc,
            GuestCount = guestCount,
            Status = ReservationStatus.Pending,
            Notes = notes?.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        reservation.Raise(new ReservationCreatedDomainEvent(reservation.Id, restaurant.Id));
        return reservation;
    }

    public void Confirm()
    {
        if (Status == ReservationStatus.Cancelled)
            throw new DomainException("INVALID_RESERVATION_STATUS", "Cancelled reservation cannot be confirmed.");
        if (Status is ReservationStatus.Completed or ReservationStatus.NoShow)
            throw new DomainException("INVALID_RESERVATION_STATUS", $"Cannot confirm reservation in status {Status}.");
        if (Status == ReservationStatus.Confirmed)
            return;

        Status = ReservationStatus.Confirmed;
        Touch();
        Raise(new ReservationConfirmedDomainEvent(Id));
    }

    public void Cancel()
    {
        if (Status is ReservationStatus.Completed or ReservationStatus.NoShow)
            throw new DomainException("INVALID_RESERVATION_STATUS", $"Cannot cancel reservation in status {Status}.");
        if (Status == ReservationStatus.Cancelled)
            return;

        Status = ReservationStatus.Cancelled;
        Touch();
        Raise(new ReservationCancelledDomainEvent(Id));
    }

    public void Complete()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new DomainException("INVALID_RESERVATION_STATUS", "Only confirmed reservations can be completed.");

        Status = ReservationStatus.Completed;
        Touch();
    }

    public void MarkNoShow()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new DomainException("INVALID_RESERVATION_STATUS", "Only confirmed reservations can be marked as no-show.");

        Status = ReservationStatus.NoShow;
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
