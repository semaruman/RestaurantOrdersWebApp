using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RestaurantOrdersWebApp.Models;

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
            else if (path == "/menu" && method == "GET")
            {
                httpContext.Response.ContentType = "application/json";
                var response = new List<Dish>
                {
                    new Dish{Id = 1, Name = "Блюдо первое", Ingredients = "перечисление ингредиентов", Photo = ""}
                };

                await httpContext.Response.WriteAsJsonAsync(response);
            }
            else if (path == "/order" && method == "POST")
            {
                httpContext.Response.ContentType = "application/json";

                using var reader = new StreamReader(httpContext.Request.Body);
                var json = await reader.ReadToEndAsync();

                var order = JsonSerializer.Deserialize<Order>(json);

                if (order != null)
                {
                    // Добавление заказа в БД будет на этой строке

                    httpContext.Response.StatusCode = 200;
                    await httpContext.Response.WriteAsJsonAsync(new { message = "Заказ создан" });
                }
                else
                {
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsJsonAsync(new { message = "Ошибка при создании заказа" });
                }
            }
            else if (path == "/order/")
            {
                httpContext.Response.ContentType = "application/json";
                int id = -1;
                if (path != null && path.StartsWith("/order/") && path.Length > "/order/".Length)
                {
                    id = Convert.ToInt32(path.Substring("/order/".Length));
                }

                if (id != -1)
                {
                    // В этой строке будет получение заказа из БД
                    // А в этой будет отправка статуса заказа
                }
                //await _next(httpContext);
            }
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
