using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;
using RestaurantOrders.Domain.Common;
using RestaurantOrders.Domain.Restaurants;

namespace RestaurantOrders.Application.Restaurants.Commands;

public sealed record CreateRestaurantCommand(
    string Name,
    string? Slug,
    string Description,
    string Street,
    string City,
    string? PostalCode,
    string Phone,
    string? Email,
    string? Website,
    PriceCategory PriceCategory,
    IReadOnlyList<string> Cuisines,
    IReadOnlyList<string>? Features,
    bool AcceptsReservations,
    bool OffersDelivery,
    int? Capacity,
    string? CoverImageUrl);

public sealed class CreateRestaurantHandler(IRestaurantRepository restaurants, IUnitOfWork uow)
{
    public async Task<Result<Guid>> Handle(CreateRestaurantCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var slug = string.IsNullOrWhiteSpace(cmd.Slug)
                ? RestaurantSlug.FromName(RestaurantName.Create(cmd.Name)).Value
                : RestaurantSlug.Create(cmd.Slug).Value;

            if (await restaurants.SlugExistsAsync(slug, ct))
                return Result.Failure<Guid>(new Error("SLUG_EXISTS", "Restaurant slug already exists.", ErrorType.Conflict));

            var restaurant = Restaurant.Create(cmd.Name, slug);
            restaurant.UpdateDescription(cmd.Description);
            restaurant.SetAddress(Address.Create(cmd.Street, cmd.City, cmd.PostalCode));
            restaurant.SetContacts(ContactInformation.Create(cmd.Phone, cmd.Email, cmd.Website));
            restaurant.SetPriceCategory(cmd.PriceCategory);
            restaurant.SetCuisineTypes(cmd.Cuisines);
            if (cmd.Features is not null)
                restaurant.SetFeatures(cmd.Features);
            restaurant.ConfigureOptions(cmd.AcceptsReservations, cmd.OffersDelivery, cmd.Capacity);
            restaurant.SetPhotos(cmd.CoverImageUrl, null);

            await restaurants.AddAsync(restaurant, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(restaurant.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record UpdateRestaurantCommand(
    Guid RestaurantId,
    string Name,
    string Description,
    string Street,
    string City,
    string? PostalCode,
    string Phone,
    string? Email,
    string? Website,
    PriceCategory PriceCategory,
    IReadOnlyList<string> Cuisines,
    IReadOnlyList<string>? Features,
    bool AcceptsReservations,
    bool OffersDelivery,
    int? Capacity,
    string? CoverImageUrl,
    IReadOnlyList<string>? PhotoUrls);

public sealed class UpdateRestaurantHandler(IRestaurantRepository restaurants, IUnitOfWork uow)
{
    public async Task<Result> Handle(UpdateRestaurantCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var restaurant = await restaurants.GetByIdAsync(RestaurantId.From(cmd.RestaurantId), ct);
            if (restaurant is null)
                return Result.Failure(new Error("RESTAURANT_NOT_FOUND", "Restaurant not found.", ErrorType.NotFound));

            restaurant.ChangeName(cmd.Name);
            restaurant.UpdateDescription(cmd.Description);
            restaurant.SetAddress(Address.Create(cmd.Street, cmd.City, cmd.PostalCode));
            restaurant.SetContacts(ContactInformation.Create(cmd.Phone, cmd.Email, cmd.Website));
            restaurant.SetPriceCategory(cmd.PriceCategory);
            restaurant.SetCuisineTypes(cmd.Cuisines);
            if (cmd.Features is not null)
                restaurant.SetFeatures(cmd.Features);
            restaurant.ConfigureOptions(cmd.AcceptsReservations, cmd.OffersDelivery, cmd.Capacity);
            restaurant.SetPhotos(cmd.CoverImageUrl, cmd.PhotoUrls);

            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record PublishRestaurantCommand(Guid RestaurantId);
public sealed class PublishRestaurantHandler(IRestaurantRepository restaurants, IUnitOfWork uow)
{
    public async Task<Result> Handle(PublishRestaurantCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var restaurant = await restaurants.GetByIdAsync(RestaurantId.From(cmd.RestaurantId), ct);
            if (restaurant is null)
                return Result.Failure(new Error("RESTAURANT_NOT_FOUND", "Restaurant not found.", ErrorType.NotFound));

            restaurant.Publish();
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record UnpublishRestaurantCommand(Guid RestaurantId);
public sealed class UnpublishRestaurantHandler(IRestaurantRepository restaurants, IUnitOfWork uow)
{
    public async Task<Result> Handle(UnpublishRestaurantCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var restaurant = await restaurants.GetByIdAsync(RestaurantId.From(cmd.RestaurantId), ct);
            if (restaurant is null)
                return Result.Failure(new Error("RESTAURANT_NOT_FOUND", "Restaurant not found.", ErrorType.NotFound));

            restaurant.Unpublish();
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record CloseRestaurantCommand(Guid RestaurantId, bool Permanently);
public sealed class CloseRestaurantHandler(IRestaurantRepository restaurants, IUnitOfWork uow)
{
    public async Task<Result> Handle(CloseRestaurantCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var restaurant = await restaurants.GetByIdAsync(RestaurantId.From(cmd.RestaurantId), ct);
            if (restaurant is null)
                return Result.Failure(new Error("RESTAURANT_NOT_FOUND", "Restaurant not found.", ErrorType.NotFound));

            if (cmd.Permanently) restaurant.ClosePermanently();
            else restaurant.CloseTemporarily();

            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}

public sealed record AddMenuItemCommand(
    Guid RestaurantId,
    string Name,
    string Description,
    decimal Price,
    string? Category,
    string? PhotoUrl,
    string? Ingredients);

public sealed class AddMenuItemHandler(IRestaurantRepository restaurants, IUnitOfWork uow)
{
    public async Task<Result<Guid>> Handle(AddMenuItemCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var restaurant = await restaurants.GetByIdAsync(RestaurantId.From(cmd.RestaurantId), ct);
            if (restaurant is null)
                return Result.Failure<Guid>(new Error("RESTAURANT_NOT_FOUND", "Restaurant not found.", ErrorType.NotFound));

            var item = restaurant.AddMenuItem(cmd.Name, cmd.Description, cmd.Price, cmd.Category, cmd.PhotoUrl, cmd.Ingredients);
            await uow.SaveChangesAsync(ct);
            return Result.Success(item.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(new Error(ex.Code, ex.Message, ErrorType.BusinessRule));
        }
    }
}
