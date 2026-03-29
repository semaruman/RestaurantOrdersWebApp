using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RestaurantOrdersWebApp.Models;
using RestaurantOrdersWebApp.Services.Interfaces;

namespace RestaurantOrdersWebApp.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class RestaurantMiddleware
    {
        private readonly RequestDelegate _next;

        public RestaurantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext httpContext,
            IRestaurantContext restaurantContext,
            IRestaurantService restaurantService
            )
        {
            string path = httpContext.Request.Path.Value?.ToLower() ?? "";
            string method = httpContext.Request.Method;

            //получение текущего ресторана
            var currentRestaurant = restaurantService.GetRestaurantByName(restaurantContext.RestaurantName);

            if (path == $"/{currentRestaurant.Name}")
            {
                httpContext.Response.ContentType = "application/json";
                var response = new
                {
                    Name = currentRestaurant.Name,
                    Endpoints = new[]
                    {
                        "GET /menu - Посмотреть меню", // GET зпрос
                        "POST /order - Сделать заказ", // POST запрос
                        "GET /order/{id} - Посмотреть статус зказа", // GET запрос
                        "GET /about - Посмотреть описание ресторана",
                        "GET /contacts - Посмотреть контакты ресторана",
                        "GET /reviews - Посмотреть отзывы ресторана",
                        "POST /reviews/add - Добавить отзыв о ресторане"
                    }
                };
                await httpContext.Response.WriteAsJsonAsync(response);
            }
            else if (path == $"/{currentRestaurant.Name}/menu" && method == "GET")
            {
                httpContext.Response.ContentType = "application/json";

                var response = currentRestaurant.MenuDishes;

                await httpContext.Response.WriteAsJsonAsync(response);
            }
            else if (path == $"/{currentRestaurant.Name}/order" && method == "POST")
            {
                httpContext.Response.ContentType = "application/json";

                using var reader = new StreamReader(httpContext.Request.Body);
                var json = await reader.ReadToEndAsync();

                var order = JsonSerializer.Deserialize<Order>(json);

                if (order != null)
                {
                    // Добавление заказа в БД будет на этой строке
                    currentRestaurant.Orders.Add(order);
                    restaurantService.AddOrder(currentRestaurant, order);

                    httpContext.Response.StatusCode = 200;
                    await httpContext.Response.WriteAsJsonAsync(new { message = "Заказ создан" });
                }
                else
                {
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsJsonAsync(new { message = "Ошибка при создании заказа" });
                }
            }
            else if (path == $"/{currentRestaurant.Name}/order/" && method == "GET")
            {
                httpContext.Response.ContentType = "application/json";
                int id = -1;
                if (path != null && path.StartsWith("/order/") && path.Length > "/order/".Length)
                {
                    id = Convert.ToInt32(path.Substring("/order/".Length));
                }

                if (id != -1)
                {
                    var order = currentRestaurant.Orders.FirstOrDefault(o => o.Id == id);
                    if (order == null)
                    {
                        await httpContext.Response.WriteAsJsonAsync(new { message = "Такого заказа не существует" });
                    }
                    else
                    {
                        await httpContext.Response.WriteAsJsonAsync(order.Status);
                    }
                }
                //await _next(httpContext);
            }
            else if (path == $"/{currentRestaurant.Name}/about" && method == "GET")
            {
                httpContext.Response.ContentType = "application/json";
                string about = currentRestaurant.Description;

                await httpContext.Response.WriteAsJsonAsync(about);
            }
            else if (path == $"/{currentRestaurant.Name}/contacts" && method == "GET")
            {
                httpContext.Response.ContentType= "application/json";
                string contacts = currentRestaurant.Contacts;

                await httpContext.Response.WriteAsJsonAsync(contacts);
            }
            else if (path == $"/{currentRestaurant.Name}/reviews" && method == "GET")
            {
                httpContext.Response.ContentType= "application/json";
                
                var reviews = currentRestaurant.Reviews.Select(r => new {r.Rating, r.Text});

                await httpContext.Response.WriteAsJsonAsync(reviews);
            }
            else if (path == $"/{currentRestaurant.Name}/reviews/add" && method == "POST")
            {
                httpContext.Response.ContentType = "application/json";
                using var reader = new StreamReader(httpContext.Request.Body);
                var json = await reader.ReadToEndAsync();

                var review = JsonSerializer.Deserialize<Review>(json);

                if (review == null)
                {
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsJsonAsync(new {Error = "Ошибка при создании отзыва"});
                }
                else
                {
                    currentRestaurant.Reviews.Add(review);
                    restaurantService.AddReview(currentRestaurant, review);
                }
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class RestaurantMiddlewareExtensions
    {
        public static IApplicationBuilder UseRestaurantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RestaurantMiddleware>();
        }
    }
}
