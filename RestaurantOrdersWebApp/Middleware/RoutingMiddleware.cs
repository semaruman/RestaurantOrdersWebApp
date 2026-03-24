using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string restaurantName = httpContext.Request.Path.Value?.TrimStart('/').Split('/').FirstOrDefault() ?? "default";
            if (restaurantName != "default")
            {
                var restaurantMiddleware = new RestaurantMiddleware(_next, restaurantName);
                await restaurantMiddleware.InvokeAsync(httpContext);
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
