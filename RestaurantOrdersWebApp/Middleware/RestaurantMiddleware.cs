using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RestaurantOrdersWebApp.Models;
using RestaurantOrdersWebApp.Services.Interfaces;
using RestaurantOrdersWebApp.Infrastructure;

namespace RestaurantOrdersWebApp.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class RestaurantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RestaurantMiddleware> _logger;

        public RestaurantMiddleware(RequestDelegate next, ILogger<RestaurantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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
                        "GET /orders/{idList} - Получить список заказов по списку id",
                        "GET /about - Посмотреть описание ресторана",
                        "POST /about - Добавить описание ресторана",
                        "GET /contacts - Посмотреть контакты ресторана",
                        "POST /contacts - Добавить контакты ресторана",
                        "GET /reviews - Посмотреть отзывы ресторана",
                        "POST /reviews/add - Добавить отзыв о ресторане",
                        "POST /menu/add - Добавить блюдо в меню",
                        "POST /basket/add?dishId={id блюда} - добавить блюдо в корзину"
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
                    int orderId = restaurantService.AddOrder(currentRestaurant, order);

                    List<int> ordersId = httpContext.Session.Get<List<int>>("ordersId") ?? new List<int>();
                    ordersId.Add(orderId);

                    httpContext.Session.Set("ordersId", ordersId);

                    httpContext.Response.StatusCode = 200;

                    _logger.LogInformation("{Date}. Заказ создан. Блюда: {dishList}",
                        order.CreatedDate, string.Join(", ", order.Dishes.Select(d => d.Name))
                        );

                    await httpContext.Response.WriteAsJsonAsync(new { message = "Заказ создан" });
                }
                else
                {
                    httpContext.Response.StatusCode = 400;

                    _logger.LogWarning("{Date}. Ошибка при создании заказа", DateTime.Now);

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

            else if (path.StartsWith($"/{currentRestaurant.Name}/orders") && method == "GET")
            {
                httpContext.Response.ContentType = "application/json";

                //получаю список id заказов из строки запроса
                string query = httpContext.Request.Query["idList"].ToString();
                _logger.LogInformation("Список ID заказов: " + query);
                if (string.IsNullOrEmpty(query))
                {
                    _logger.LogWarning("Список ID заказов отсутствует!!!");
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsJsonAsync(new { error = "Заказы не найдены" });
                    return;
                }

                _logger.LogInformation("Начинаю преобразовывать строку запроса в список чисел");
                var orderIds = query.Split(',').Select(int.Parse).ToList();
                
                var orders = orderService.GetOdersByIds(orderIds);
                if (orders == null || orders.Count == 0)
                {
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteAsJsonAsync(new { error = "Заказы не найдены" });
                }
                else
                {
                    httpContext.Response.StatusCode = 200;
                    await httpContext.Response.WriteAsJsonAsync(orders);
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

                _logger.LogInformation("Описание ресторана {name} изменено на:\n {about}",
                    currentRestaurant.Name, about
                    );

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

                restaurantService.ChangeRestaurantContacts(currentRestaurant.Name, contacts);

                httpContext.Response.StatusCode = 200;

                _logger.LogInformation("Контакты ресторана {name} изменены на:\n {contacts}",
                    currentRestaurant.Name, contacts
                    );

                await httpContext.Response.WriteAsJsonAsync($"Контакты изменены на: {contacts}");
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

                    _logger.LogWarning("Ошибка при создании отзыва к ресторану {name}", currentRestaurant.Name);

                    await httpContext.Response.WriteAsJsonAsync(new {Error = "Ошибка при создании отзыва"});
                }
                else
                {
                    restaurantService.AddReview(currentRestaurant, review);
                    httpContext.Response.StatusCode = 201;

                    _logger.LogInformation("Отзыв к ресторану {name} добавлен: \n {reviewText}", 
                        currentRestaurant.Name, review.Text
                        );

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

                    _logger.LogWarning("Ошибка при добавлении блюда в ресторан {name}", currentRestaurant.Name);

                    await httpContext.Response.WriteAsJsonAsync(new { Error = "Ошибка при добавлении блюда" });
                }
                else
                {
                    restaurantService.AddMenuDish(currentRestaurant, dish);
                    httpContext.Response.StatusCode = 201;

                    _logger.LogInformation("Блюдо {dishName} в ресторан {restName} добавлено",
                        dish.Name, currentRestaurant.Name);

                    await httpContext.Response.WriteAsJsonAsync(new { message = "Блюдо добавлено успешно" });
                }
            }
            else if (path == $"/{currentRestaurant.Name}/basket/add" && method == "POST")
            {
                int dishId = Convert.ToInt32(httpContext.Request.Query["dishId"]);

                List<int> dishesId = httpContext.Session.Get<List<int>>("dishesId") ?? new List<int>();
                dishesId.Add(dishId);

                httpContext.Session.Set("dishesId", dishesId);

                await httpContext.Response.WriteAsJsonAsync(new { message = "Блюдо добавлено в корзину успешно" });
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
