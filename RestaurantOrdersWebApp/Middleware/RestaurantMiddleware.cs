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
            IRestaurantService restaurantService,
            IOrderService orderService
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
                        "POST /about - Добавить описание ресторана",
                        "GET /contacts - Посмотреть контакты ресторана",
                        "POST /contacts - Добавить контакты ресторана",
                        "GET /reviews - Посмотреть отзывы ресторана",
                        "POST /reviews/add - Добавить отзыв о ресторане",
                        "POST /menu/add - Добавить блюдо в меню"
                    }
                };
                await httpContext.Response.WriteAsJsonAsync(response);
            }
            else if (path == $"/{currentRestaurant.Name}/menu" && method == "GET")
            {
                httpContext.Response.ContentType = "application/json";

                var response = restaurantService.GetRestaurantMenu(currentRestaurant.Name);

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
            else if (path.StartsWith($"/{currentRestaurant.Name}/order/") && method == "GET")
            {
                httpContext.Response.ContentType = "application/json";
                int id = -1;
                if (path.Length > $"/{currentRestaurant.Name}/order/".Length)
                {
                    id = Convert.ToInt32(path.Substring($"/{currentRestaurant.Name}/order/".Length));
                }

                if (id != -1)
                { 
                    var order = orderService.GetOrderById(id);
                    if (order == null)
                    {
                        httpContext.Response.StatusCode = 400;
                        await httpContext.Response.WriteAsJsonAsync(new { message = "Такого заказа не существует" });
                    }
                    else
                    {
                        await httpContext.Response.WriteAsJsonAsync(new { status = order.Status });
                    }
                }
                else
                {
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsJsonAsync(new { message = "Такого заказа не существует" });
                }
            }
            else if (path == $"/{currentRestaurant.Name}/about" && method == "GET")
            {
                httpContext.Response.ContentType = "application/json";
                string about = currentRestaurant.Description;

                await httpContext.Response.WriteAsJsonAsync(about);
            }
            else if (path == $"/{currentRestaurant.Name}/about" && method == "POST")
            {
                httpContext.Response.ContentType = "application/json";

                string about = httpContext.Request.Query["message"];

                restaurantService.ChangeRestaurantDescription(currentRestaurant.Name, about);

                httpContext.Response.StatusCode = 200;
                await httpContext.Response.WriteAsJsonAsync($"Описание изменено на: {about}");
            }
            else if (path == $"/{currentRestaurant.Name}/contacts" && method == "GET")
            {
                httpContext.Response.ContentType= "application/json";
                string contacts = currentRestaurant.Contacts;

                await httpContext.Response.WriteAsJsonAsync(contacts);
            }
            else if (path == $"/{currentRestaurant.Name}/contacts" && method == "POST")
            {
                httpContext.Response.ContentType = "application/json";

                string contacts = httpContext.Request.Query["message"];

                restaurantService.ChangeRestaurantDescription(currentRestaurant.Name, contacts);

                httpContext.Response.StatusCode = 200;
                await httpContext.Response.WriteAsJsonAsync($"Описание изменено на: {contacts}");
            }
            else if (path == $"/{currentRestaurant.Name}/reviews" && method == "GET")
            {
                httpContext.Response.ContentType= "application/json";
                Console.WriteLine(string.Join("!", currentRestaurant.Reviews));
                var reviews = restaurantService.GetRestaurantReviews(currentRestaurant.Name).Select(r => new {r.Rating, r.Text});

                await httpContext.Response.WriteAsJsonAsync(reviews);
            }
            else if (path == $"/{currentRestaurant.Name}/reviews/add" && method == "POST")
            {
                httpContext.Response.ContentType = "application/json";
                using var reader = new StreamReader(httpContext.Request.Body);
                string json = await reader.ReadToEndAsync();

                var review = JsonSerializer.Deserialize<Review>(json);

                if (review == null)
                {
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsJsonAsync(new {Error = "Ошибка при создании отзыва"});
                }
                else
                {
                    restaurantService.AddReview(currentRestaurant, review);
                    httpContext.Response.StatusCode = 201;
                    await httpContext.Response.WriteAsJsonAsync(new { message = "Отзыв добавлен" });
                }
            }
            else if (path == $"/{currentRestaurant.Name}/menu/add" && method == "POST")
            {
                httpContext.Response.ContentType = "application/json";

                using var reader = new StreamReader(httpContext.Request.Body);
                string json = await reader.ReadToEndAsync();

                var dish = JsonSerializer.Deserialize<Dish>(json);

                if (dish == null)
                {
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsJsonAsync(new { Error = "Ошибка при добавлении блюда" });
                }
                else
                {
                    restaurantService.AddMenuDish(currentRestaurant, dish);
                    httpContext.Response.StatusCode = 201;
                    await httpContext.Response.WriteAsJsonAsync(new { message = "Блюдо добавлено успешно" });
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
