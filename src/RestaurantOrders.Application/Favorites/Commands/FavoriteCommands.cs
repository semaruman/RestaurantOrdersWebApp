using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;
using RestaurantOrders.Domain.Favorites;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;

namespace RestaurantOrders.Application.Favorites.Commands;

public sealed record AddFavoriteCommand(Guid UserId, Guid RestaurantId);
public sealed class AddFavoriteHandler(
    IFavoriteRepository favorites,
    IRestaurantRepository restaurants,
    IUnitOfWork uow)
{
    public async Task<Result<Guid>> Handle(AddFavoriteCommand cmd, CancellationToken ct = default)
    {
        var restaurant = await restaurants.GetByIdAsync(RestaurantId.From(cmd.RestaurantId), ct);
        if (restaurant is null)
            return Result.Failure<Guid>(new Error("RESTAURANT_NOT_FOUND", "Ресторан не найден.", ErrorType.NotFound));

        var userId = UserId.From(cmd.UserId);
        var restaurantId = restaurant.Id;
        var existing = await favorites.GetAsync(userId, restaurantId, ct);
        if (existing is not null)
            return Result.Failure<Guid>(new Error("FAVORITE_EXISTS", "Ресторан уже в избранном.", ErrorType.Conflict));

        var favorite = Favorite.Create(userId, restaurantId);
        await favorites.AddAsync(favorite, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(favorite.Id.Value);
    }
}

public sealed record RemoveFavoriteCommand(Guid UserId, Guid RestaurantId);
public sealed class RemoveFavoriteHandler(IFavoriteRepository favorites, IUnitOfWork uow)
{
    public async Task<Result> Handle(RemoveFavoriteCommand cmd, CancellationToken ct = default)
    {
        var favorite = await favorites.GetAsync(UserId.From(cmd.UserId), RestaurantId.From(cmd.RestaurantId), ct);
        if (favorite is null)
            return Result.Failure(new Error("FAVORITE_NOT_FOUND", "Запись в избранном не найдена.", ErrorType.NotFound));

        await favorites.RemoveAsync(favorite, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
