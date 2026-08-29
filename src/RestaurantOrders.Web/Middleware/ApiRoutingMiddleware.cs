using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantOrders.Application.Abstractions;
using RestaurantOrders.Application.Common;
using RestaurantOrders.Application.Favorites.Commands;
using RestaurantOrders.Application.Favorites.Queries;
using RestaurantOrders.Application.Orders.Commands;
using RestaurantOrders.Application.Orders.Queries;
using RestaurantOrders.Application.Reservations.Commands;
using RestaurantOrders.Application.Reservations.Queries;
using RestaurantOrders.Application.Restaurants.Commands;
using RestaurantOrders.Application.Restaurants.Queries;
using RestaurantOrders.Application.Reviews.Commands;
using RestaurantOrders.Application.Reviews.Queries;
using RestaurantOrders.Application.Users.Commands;
using RestaurantOrders.Domain.Restaurants;
using RestaurantOrders.Domain.Users;
using RestaurantOrders.Infrastructure.Authentication;
using RestaurantOrders.Infrastructure.Persistence;

namespace RestaurantOrders.Web.Middleware;

public sealed class ApiRoutingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/v1", out var remaining))
        {
            await next(context);
            return;
        }

        var segments = remaining.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var method = context.Request.Method;

        if (segments.Length >= 1 && segments[0] == "auth")
        {
            await HandleAuth(context, segments, method);
            return;
        }

        if (segments is ["restaurants"] && method == "GET")
        {
            var filter = new RestaurantSearchFilter(
                context.Request.Query["q"], context.Request.Query["cuisine"], context.Request.Query["city"],
                Enum.TryParse<PriceCategory>(context.Request.Query["price"], true, out var price) ? price : null,
                decimal.TryParse(context.Request.Query["minRating"], out var rating) ? rating : null,
                context.Request.Query["feature"],
                bool.TryParse(context.Request.Query["openNow"], out var open) ? open : null,
                bool.TryParse(context.Request.Query["reservations"], out var reservations) ? reservations : null,
                context.Request.Query["sort"],
                int.TryParse(context.Request.Query["page"], out var page) ? page : 1,
                int.TryParse(context.Request.Query["pageSize"], out var size) ? size : 12);
            var handler = context.RequestServices.GetRequiredService<SearchRestaurantsHandler>();
            await context.Response.WriteAsJsonAsync(await handler.Handle(new SearchRestaurantsQuery(filter), context.RequestAborted));
            return;
        }

        if (segments is ["restaurants", "featured"] && method == "GET")
        {
            var handler = context.RequestServices.GetRequiredService<GetFeaturedRestaurantsHandler>();
            var take = int.TryParse(context.Request.Query["take"], out var count) ? count : 6;
            await context.Response.WriteAsJsonAsync(await handler.Handle(new GetFeaturedRestaurantsQuery(take), context.RequestAborted));
            return;
        }

        if (segments.Length == 2 && segments[0] == "restaurants" && method == "GET")
        {
            var handler = context.RequestServices.GetRequiredService<GetRestaurantDetailsHandler>();
            var query = Guid.TryParse(segments[1], out var id)
                ? new GetRestaurantDetailsQuery(id, null)
                : new GetRestaurantDetailsQuery(null, segments[1]);
            await WriteResult(context, await handler.Handle(query, context.RequestAborted));
            return;
        }

        if (segments.Length == 3 && segments[0] == "restaurants" && segments[2] == "reviews" && method == "GET"
            && Guid.TryParse(segments[1], out var reviewRestaurantId))
        {
            var handler = context.RequestServices.GetRequiredService<GetRestaurantReviewsHandler>();
            await context.Response.WriteAsJsonAsync(await handler.Handle(
                new GetRestaurantReviewsQuery(reviewRestaurantId), context.RequestAborted));
            return;
        }

        if (segments is ["restaurants"] && method == "POST")
        {
            if (!await Authorize(context, Permissions.RestaurantManage)) return;
            var body = await context.Request.ReadFromJsonAsync<RestaurantRequest>(context.RequestAborted);
            if (body is null) { await BadRequest(context, "Тело запроса обязательно."); return; }
            var handler = context.RequestServices.GetRequiredService<CreateRestaurantHandler>();
            await WriteResult(context, await handler.Handle(body.ToCreate(), context.RequestAborted), StatusCodes.Status201Created);
            return;
        }

        if (segments.Length == 2 && segments[0] == "restaurants" && Guid.TryParse(segments[1], out var restaurantId))
        {
            if (!await Authorize(context, Permissions.RestaurantManage)) return;
            if (method == "PUT")
            {
                var body = await context.Request.ReadFromJsonAsync<RestaurantRequest>(context.RequestAborted);
                if (body is null) { await BadRequest(context, "Тело запроса обязательно."); return; }
                var handler = context.RequestServices.GetRequiredService<UpdateRestaurantHandler>();
                await WriteResult(context, await handler.Handle(body.ToUpdate(restaurantId), context.RequestAborted));
                return;
            }
            if (method == "DELETE")
            {
                var handler = context.RequestServices.GetRequiredService<CloseRestaurantHandler>();
                await WriteResult(context, await handler.Handle(new CloseRestaurantCommand(restaurantId, true), context.RequestAborted));
                return;
            }
        }

        if (segments.Length == 3 && segments[0] == "restaurants" && Guid.TryParse(segments[1], out restaurantId))
        {
            if (segments[2] == "menu" && method == "GET")
            {
                var handler = context.RequestServices.GetRequiredService<GetRestaurantDetailsHandler>();
                var result = await handler.Handle(new GetRestaurantDetailsQuery(restaurantId, null), context.RequestAborted);
                if (result.IsFailure) await WriteError(context, result.Error!);
                else await context.Response.WriteAsJsonAsync(result.Value.Menu);
                return;
            }
            if (segments[2] == "menu" && method == "POST")
            {
                if (!await Authorize(context, Permissions.RestaurantManage)) return;
                var body = await context.Request.ReadFromJsonAsync<MenuItemRequest>(context.RequestAborted);
                if (body is null) { await BadRequest(context, "Тело запроса обязательно."); return; }
                var handler = context.RequestServices.GetRequiredService<AddMenuItemHandler>();
                await WriteResult(context, await handler.Handle(new AddMenuItemCommand(restaurantId, body.Name,
                    body.Description, body.Price, body.Category, body.PhotoUrl, body.Ingredients), context.RequestAborted),
                    StatusCodes.Status201Created);
                return;
            }
            if (segments[2] is "publish" or "unpublish" && method == "POST")
            {
                if (!await Authorize(context, Permissions.RestaurantPublish)) return;
                Result result = segments[2] == "publish"
                    ? await context.RequestServices.GetRequiredService<PublishRestaurantHandler>()
                        .Handle(new PublishRestaurantCommand(restaurantId), context.RequestAborted)
                    : await context.RequestServices.GetRequiredService<UnpublishRestaurantHandler>()
                        .Handle(new UnpublishRestaurantCommand(restaurantId), context.RequestAborted);
                await WriteResult(context, result);
                return;
            }
        }

        if (segments.Length >= 1 && segments[0] == "reservations")
        {
            if (!await Authorize(context, Permissions.ReservationCreate)) return;
            var userId = CurrentUserId(context);
            if (segments.Length == 1 && method == "POST")
            {
                var body = await context.Request.ReadFromJsonAsync<ReservationRequest>(context.RequestAborted);
                if (body is null) { await BadRequest(context, "Тело запроса обязательно."); return; }
                var handler = context.RequestServices.GetRequiredService<CreateReservationHandler>();
                await WriteResult(context, await handler.Handle(new CreateReservationCommand(body.RestaurantId,
                    userId, body.ReservationDateTimeUtc, body.GuestCount, body.Notes), context.RequestAborted),
                    StatusCodes.Status201Created);
                return;
            }
            if (segments.Length == 1 && method == "GET")
            {
                var handler = context.RequestServices.GetRequiredService<GetUserReservationsHandler>();
                await context.Response.WriteAsJsonAsync(await handler.Handle(new GetUserReservationsQuery(userId), context.RequestAborted));
                return;
            }
            if (segments.Length == 3 && Guid.TryParse(segments[1], out var reservationId) && method == "POST")
            {
                Result result = segments[2] == "confirm"
                    ? await context.RequestServices.GetRequiredService<ConfirmReservationHandler>()
                        .Handle(new ConfirmReservationCommand(reservationId), context.RequestAborted)
                    : await context.RequestServices.GetRequiredService<CancelReservationHandler>()
                        .Handle(new CancelReservationCommand(reservationId), context.RequestAborted);
                await WriteResult(context, result);
                return;
            }
        }

        if (segments.Length >= 1 && segments[0] == "orders")
        {
            if (!await Authorize(context, Permissions.OrderCreate)) return;
            var userId = CurrentUserId(context);
            if (segments.Length == 1 && method == "POST")
            {
                var body = await context.Request.ReadFromJsonAsync<OrderRequest>(context.RequestAborted);
                if (body is null) { await BadRequest(context, "Тело запроса обязательно."); return; }
                var handler = context.RequestServices.GetRequiredService<CreateOrderHandler>();
                await WriteResult(context, await handler.Handle(new CreateOrderCommand(body.RestaurantId, userId,
                    body.Items.Select(x => new CreateOrderItemDto(x.MenuItemId, x.Quantity)).ToList(), body.Notes),
                    context.RequestAborted), StatusCodes.Status201Created);
                return;
            }
            if (segments.Length == 1 && method == "GET")
            {
                var handler = context.RequestServices.GetRequiredService<GetUserOrdersHandler>();
                await context.Response.WriteAsJsonAsync(await handler.Handle(new GetUserOrdersQuery(userId), context.RequestAborted));
                return;
            }
            if (segments.Length == 3 && Guid.TryParse(segments[1], out var orderId) && method == "POST")
            {
                Result result = segments[2] == "cancel"
                    ? await context.RequestServices.GetRequiredService<CancelOrderHandler>()
                        .Handle(new CancelOrderCommand(orderId), context.RequestAborted)
                    : await context.RequestServices.GetRequiredService<UpdateOrderStatusHandler>()
                        .Handle(new UpdateOrderStatusCommand(orderId, segments[2]), context.RequestAborted);
                await WriteResult(context, result);
                return;
            }
        }

        if (segments is ["reviews"] && method == "POST")
        {
            if (!await Authorize(context, Permissions.ReviewCreate)) return;
            var body = await context.Request.ReadFromJsonAsync<ReviewRequest>(context.RequestAborted);
            if (body is null) { await BadRequest(context, "Тело запроса обязательно."); return; }
            var handler = context.RequestServices.GetRequiredService<SubmitReviewHandler>();
            await WriteResult(context, await handler.Handle(new SubmitReviewCommand(body.RestaurantId,
                CurrentUserId(context), body.Rating, body.Comment), context.RequestAborted), StatusCodes.Status201Created);
            return;
        }

        if (segments.Length == 3 && segments[0] == "reviews" && Guid.TryParse(segments[1], out var reviewId)
            && method == "POST")
        {
            if (!await Authorize(context, Permissions.ReviewModerate)) return;
            var handler = context.RequestServices.GetRequiredService<ModerateReviewHandler>();
            await WriteResult(context, await handler.Handle(new ModerateReviewCommand(reviewId, segments[2]), context.RequestAborted));
            return;
        }

        if (segments.Length >= 1 && segments[0] == "favorites")
        {
            if (!await Authorize(context, "Authenticated")) return;
            var userId = CurrentUserId(context);
            if (segments.Length == 1 && method == "GET")
            {
                var handler = context.RequestServices.GetRequiredService<GetFavoritesHandler>();
                await context.Response.WriteAsJsonAsync(await handler.Handle(new GetFavoritesQuery(userId), context.RequestAborted));
                return;
            }
            if (segments.Length == 2 && Guid.TryParse(segments[1], out var favoriteRestaurantId))
            {
                if (method == "POST")
                {
                    var handler = context.RequestServices.GetRequiredService<AddFavoriteHandler>();
                    await WriteResult(context, await handler.Handle(new AddFavoriteCommand(userId, favoriteRestaurantId),
                        context.RequestAborted), StatusCodes.Status201Created);
                    return;
                }
                if (method == "DELETE")
                {
                    var handler = context.RequestServices.GetRequiredService<RemoveFavoriteHandler>();
                    await WriteResult(context, await handler.Handle(new RemoveFavoriteCommand(userId, favoriteRestaurantId),
                        context.RequestAborted));
                    return;
                }
            }
        }

        if (segments is ["admin", "stats"] && method == "GET")
        {
            if (!await Authorize(context, "Admin")) return;
            var db = context.RequestServices.GetRequiredService<AppDbContext>();
            await context.Response.WriteAsJsonAsync(new
            {
                restaurants = await db.Restaurants.CountAsync(context.RequestAborted),
                users = await db.Users.CountAsync(context.RequestAborted),
                orders = await db.Orders.CountAsync(context.RequestAborted),
                reservations = await db.Reservations.CountAsync(context.RequestAborted),
                reviews = await db.Reviews.CountAsync(context.RequestAborted)
            });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { error = "Маршрут API не найден." });
    }

    private static async Task HandleAuth(HttpContext context, string[] segments, string method)
    {
        var users = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var signIn = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        if (segments is ["auth", "register"] && method == "POST")
        {
            var body = await context.Request.ReadFromJsonAsync<RegisterRequest>(context.RequestAborted);
            if (body is null) { await BadRequest(context, "Тело запроса обязательно."); return; }
            var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = body.Email, Email = body.Email,
                DisplayName = body.DisplayName, EmailConfirmed = true };
            var created = await users.CreateAsync(user, body.Password);
            if (!created.Succeeded)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { errors = created.Errors.Select(x => x.Description) });
                return;
            }
            await users.AddToRoleAsync(user, Roles.User);
            var profile = context.RequestServices.GetRequiredService<RegisterUserHandler>();
            var result = await profile.Handle(new RegisterUserCommand(user.Id, body.DisplayName, body.Email), context.RequestAborted);
            if (result.IsFailure) { await users.DeleteAsync(user); await WriteResult(context, result); return; }
            await signIn.SignInAsync(user, false);
            context.Response.StatusCode = StatusCodes.Status201Created;
            await context.Response.WriteAsJsonAsync(new { user.Id, user.Email, user.DisplayName, roles = new[] { Roles.User } });
            return;
        }
        if (segments is ["auth", "login"] && method == "POST")
        {
            var body = await context.Request.ReadFromJsonAsync<LoginRequest>(context.RequestAborted);
            var user = body is null ? null : await users.FindByEmailAsync(body.Email);
            if (user is null || !(await signIn.CheckPasswordSignInAsync(user, body!.Password, true)).Succeeded)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Неверный email или пароль." });
                return;
            }
            await signIn.SignInAsync(user, body.RememberMe);
            await context.Response.WriteAsJsonAsync(new { user.Id, user.Email, user.DisplayName,
                roles = await users.GetRolesAsync(user) });
            return;
        }
        if (segments is ["auth", "logout"] && method == "POST")
        {
            await signIn.SignOutAsync();
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }
        if (segments is ["auth", "me"] && method == "GET")
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            var user = await users.GetUserAsync(context.User);
            await context.Response.WriteAsJsonAsync(new { user!.Id, user.Email, user.DisplayName,
                roles = await users.GetRolesAsync(user) });
            return;
        }
        context.Response.StatusCode = StatusCodes.Status404NotFound;
    }

    private static async Task<bool> Authorize(HttpContext context, string policy)
    {
        var authorization = context.RequestServices.GetRequiredService<IAuthorizationService>();
        if ((await authorization.AuthorizeAsync(context.User, policy)).Succeeded)
            return true;
        context.Response.StatusCode = context.User.Identity?.IsAuthenticated == true
            ? StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "Требуется авторизация." });
        return false;
    }

    private static Guid CurrentUserId(HttpContext context) =>
        Guid.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static async Task WriteResult(HttpContext context, Result result)
    {
        if (result.IsSuccess) { context.Response.StatusCode = StatusCodes.Status204NoContent; return; }
        await WriteError(context, result.Error!);
    }

    private static async Task WriteResult<T>(HttpContext context, Result<T> result, int success = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            context.Response.StatusCode = success;
            await context.Response.WriteAsJsonAsync(result.Value);
            return;
        }
        await WriteError(context, result.Error!);
    }

    private static async Task WriteError(HttpContext context, Error error)
    {
        context.Response.StatusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = $"https://restaurant.local/problems/{error.Code.ToLowerInvariant()}",
            title = error.Message,
            status = context.Response.StatusCode,
            code = error.Code,
            traceId = context.TraceIdentifier
        });
    }

    private static Task BadRequest(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsJsonAsync(new { error = message });
    }
}

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
public sealed record LoginRequest(string Email, string Password, bool RememberMe = false);
public sealed record ReservationRequest(Guid RestaurantId, DateTime ReservationDateTimeUtc, int GuestCount, string? Notes);
public sealed record ReviewRequest(Guid RestaurantId, decimal Rating, string Comment);
public sealed record OrderItemRequest(Guid MenuItemId, int Quantity);
public sealed record OrderRequest(Guid RestaurantId, IReadOnlyList<OrderItemRequest> Items, string? Notes);
public sealed record MenuItemRequest(string Name, string Description, decimal Price, string? Category, string? PhotoUrl, string? Ingredients);
public sealed record RestaurantRequest(
    string Name, string? Slug, string Description, string Street, string City, string? PostalCode,
    string Phone, string? Email, string? Website, PriceCategory PriceCategory,
    IReadOnlyList<string> Cuisines, IReadOnlyList<string>? Features, bool AcceptsReservations,
    bool OffersDelivery, int? Capacity, string? CoverImageUrl, IReadOnlyList<string>? PhotoUrls)
{
    public CreateRestaurantCommand ToCreate() => new(Name, Slug, Description, Street, City, PostalCode,
        Phone, Email, Website, PriceCategory, Cuisines, Features, AcceptsReservations, OffersDelivery,
        Capacity, CoverImageUrl);

    public UpdateRestaurantCommand ToUpdate(Guid id) => new(id, Name, Description, Street, City, PostalCode,
        Phone, Email, Website, PriceCategory, Cuisines, Features, AcceptsReservations, OffersDelivery,
        Capacity, CoverImageUrl, PhotoUrls);
}
