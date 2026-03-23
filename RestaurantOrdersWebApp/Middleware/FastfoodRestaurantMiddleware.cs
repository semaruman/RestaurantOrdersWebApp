using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RestaurantOrdersWebApp.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class FastfoodRestaurantMiddleware
    {
        private readonly RequestDelegate _next;

        public FastfoodRestaurantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string path = httpContext.Request.Path.Value?.ToLower() ?? "";
            string method = httpContext.Request.Method;

            if (path == "" || path == "/")
            {
                httpContext.Response.ContentType = "application/json";
                var response = new
                {
                    Name = "Fast-food restaurant!",
                    Endpoints = new[]
                    {
                        "GET /menu - Посмотреть меню", // GET зпрос
                        "POST /order - Сделать заказ", // POST запрос
                        "GET /order/{id} - Посмотреть статус зказа", // GET запрос
                    }
                };
                await httpContext.Response.WriteAsJsonAsync(response);
            }
            

            //await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class FastfoodRestaurantMiddlewareExtensions
    {
        public static IApplicationBuilder UseFastfoodRestaurantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FastfoodRestaurantMiddleware>();
        }
    }
}
