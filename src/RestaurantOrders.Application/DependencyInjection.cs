using Microsoft.Extensions.DependencyInjection;
using RestaurantOrders.Application.Favorites.Commands;
using RestaurantOrders.Application.Favorites.Queries;
using RestaurantOrders.Application.Orders.Commands;
using RestaurantOrders.Application.Orders.Queries;
using RestaurantOrders.Application.Reservations.Commands;
using RestaurantOrders.Application.Restaurants.Commands;
using RestaurantOrders.Application.Restaurants.Queries;
using RestaurantOrders.Application.Reviews.Commands;
using RestaurantOrders.Application.Reviews.Queries;
using RestaurantOrders.Application.Users.Commands;

namespace RestaurantOrders.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateRestaurantHandler>();
        services.AddScoped<UpdateRestaurantHandler>();
        services.AddScoped<PublishRestaurantHandler>();
        services.AddScoped<UnpublishRestaurantHandler>();
        services.AddScoped<CloseRestaurantHandler>();
        services.AddScoped<AddMenuItemHandler>();
        services.AddScoped<SearchRestaurantsHandler>();
        services.AddScoped<GetRestaurantDetailsHandler>();
        services.AddScoped<GetFeaturedRestaurantsHandler>();

        services.AddScoped<CreateReservationHandler>();
        services.AddScoped<ConfirmReservationHandler>();
        services.AddScoped<CancelReservationHandler>();
        services.AddScoped<RestaurantOrders.Application.Reservations.Queries.GetUserReservationsHandler>();

        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<CancelOrderHandler>();
        services.AddScoped<GetUserOrdersHandler>();
        services.AddScoped<UpdateOrderStatusHandler>();

        services.AddScoped<SubmitReviewHandler>();
        services.AddScoped<ModerateReviewHandler>();
        services.AddScoped<GetRestaurantReviewsHandler>();

        services.AddScoped<AddFavoriteHandler>();
        services.AddScoped<RemoveFavoriteHandler>();
        services.AddScoped<GetFavoritesHandler>();

        services.AddScoped<RegisterUserHandler>();

        return services;
    }
}
