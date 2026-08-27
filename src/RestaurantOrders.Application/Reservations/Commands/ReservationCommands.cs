using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;
using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Reservations;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Application.Reservations.Commands;

public sealed record CreateReservationCommand(
    Guid RestaurantId,
    Guid UserId,
    DateTime ReservationDateTimeUtc,
    int GuestCount,
    string? Notes);

public sealed class CreateReservationHandler(
    IRestaurantRepository restaurants,
    IReservationRepository reservations,
    IUnitOfWork uow)
{
    public async Task<Result<Guid>> Handle(CreateReservationCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var restaurant = await restaurants.GetByIdAsync(RestaurantId.From(cmd.RestaurantId), ct);
            if (restaurant is null)
                return Result.Failure<Guid>(new Error("RESTAURANT_NOT_FOUND", "Restaurant not found.", ErrorType.NotFound));

            // Concurrency: check overlapping capacity for the day
            if (restaurant.Capacity is int capacity)
            {
                var dayStart = DateTime.SpecifyKind(cmd.ReservationDateTimeUtc.Date, DateTimeKind.Utc);
                var existing = await reservations.GetByRestaurantAsync(restaurant.Id, dayStart, ct);
                var activeGuests = existing
                    .Where(r => r.Status is ReservationStatus.Pending or ReservationStatus.Confirmed)
                    .Where(r => Math.Abs((r.ReservationDateTimeUtc - cmd.ReservationDateTimeUtc).TotalHours) < 2)
                    .Sum(r => r.GuestCount);

                if (activeGuests + cmd.GuestCount > capacity)
                    return Result.Failure<Guid>(new Error("CAPACITY_EXCEEDED", "Restaurant capacity exceeded for this time slot.", ErrorType.Conflict));
            }

            var reservation = Reservation.Create(
                restaurant,
                UserId.From(cmd.UserId),
                cmd.ReservationDateTimeUtc,
                cmd.GuestCount,
                cmd.Notes);

            await reservations.AddAsync(reservation, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(reservation.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record ConfirmReservationCommand(Guid ReservationId);
public sealed class ConfirmReservationHandler(IReservationRepository reservations, IUnitOfWork uow)
{
    public async Task<Result> Handle(ConfirmReservationCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var reservation = await reservations.GetByIdAsync(ReservationId.From(cmd.ReservationId), ct);
            if (reservation is null)
                return Result.Failure(new Error("RESERVATION_NOT_FOUND", "Reservation not found.", ErrorType.NotFound));

            reservation.Confirm();
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record CancelReservationCommand(Guid ReservationId);
public sealed class CancelReservationHandler(IReservationRepository reservations, IUnitOfWork uow)
{
    public async Task<Result> Handle(CancelReservationCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var reservation = await reservations.GetByIdAsync(ReservationId.From(cmd.ReservationId), ct);
            if (reservation is null)
                return Result.Failure(new Error("RESERVATION_NOT_FOUND", "Reservation not found.", ErrorType.NotFound));

            reservation.Cancel();
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}
