using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Application.Reservations.Queries;

public sealed record ReservationDto(
    Guid Id,
    Guid RestaurantId,
    Guid UserId,
    DateTime ReservationDateTimeUtc,
    int GuestCount,
    string Status,
    string? Notes);

public sealed record GetUserReservationsQuery(Guid UserId);
public sealed class GetUserReservationsHandler(IReservationRepository reservations)
{
    public async Task<IReadOnlyList<ReservationDto>> Handle(GetUserReservationsQuery query, CancellationToken ct = default)
    {
        var list = await reservations.GetByUserAsync(UserId.From(query.UserId), ct);
        return list.Select(r => new ReservationDto(
            r.Id.Value,
            r.RestaurantId.Value,
            r.UserId.Value,
            r.ReservationDateTimeUtc,
            r.GuestCount,
            r.Status.ToString(),
            r.Notes)).ToList();
    }
}
