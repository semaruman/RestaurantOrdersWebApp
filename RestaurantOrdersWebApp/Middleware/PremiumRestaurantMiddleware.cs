using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RestaurantOrdersWebApp.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class PremiumRestaurantMiddleware
    {
        private readonly RequestDelegate _next;

        public PremiumRestaurantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class PremiumRestaurantMiddlewareExtensions
    {
        public static IApplicationBuilder UsePremiumRestaurantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PremiumRestaurantMiddleware>();
        }
    }
}
