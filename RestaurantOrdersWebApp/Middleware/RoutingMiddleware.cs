using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RestaurantOrdersWebApp.Services.Interfaces;
using RestaurantOrdersWebApp.Models;

namespace RestaurantOrdersWebApp.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class RoutingMiddleware
    {
        private readonly RequestDelegate _next;

        public RoutingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext httpContext, 
            IRestaurantContext restaurantContext, 
            IRestaurantService restaurantService)
        {
            string restaurantName = httpContext.Request.Path.Value?.TrimStart('/').Split('/').FirstOrDefault() ?? "default";

            //если название ресторана введено в url и ресторан с таким названием есть в БД, и метод - GET
            if (restaurantName != "default" && restaurantName != string.Empty && restaurantService.GetRestaurantByName(restaurantName) != null && httpContext.Request.Method == "GET")
            {
                restaurantContext.RestaurantName = restaurantName;
                await _next(httpContext);
            }

            //если название также введено и метод - POST
            else if (restaurantName != "default" && restaurantName != string.Empty && httpContext.Request.Method == "POST")
            {
                httpContext.Response.ContentType = "application/json";
                if (restaurantService.AddRestaurant(new Restaurant { Name = restaurantName }))
                {
                    await httpContext.Response.WriteAsJsonAsync(new { message = "Ресторан добавлен успешно" });
                }
                else
                {
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsJsonAsync(new { message = "Ресторан уже существует" });
                }
            }
            else
            {
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsJsonAsync(new {message = "Такого ресторана не существует"});
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class RoutingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoutingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoutingMiddleware>();
        }
    }
}
