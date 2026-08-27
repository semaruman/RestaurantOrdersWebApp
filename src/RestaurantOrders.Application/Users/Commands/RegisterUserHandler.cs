using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;
using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Application.Users.Commands;

public sealed record RegisterUserCommand(Guid UserId, string DisplayName, string Email);

public sealed class RegisterUserHandler(IUserProfileRepository users, IUnitOfWork uow)
{
    public async Task<Result> Handle(RegisterUserCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var existing = await users.GetByIdAsync(UserId.From(cmd.UserId), ct);
            if (existing is not null)
                return Result.Success();

            var profile = UserProfile.Create(UserId.From(cmd.UserId), cmd.DisplayName, cmd.Email);
            await users.AddAsync(profile, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}
