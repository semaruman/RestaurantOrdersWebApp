using RestaurantOrdersWebApp.Data;
using RestaurantOrdersWebApp.Infrastructure;
using RestaurantOrdersWebApp.Middleware;
using RestaurantOrdersWebApp.Models;
using RestaurantOrdersWebApp.Services;
using RestaurantOrdersWebApp.Services.EF_services;
using RestaurantOrdersWebApp.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IOrderService , OrderService>();
builder.Services.AddScoped<IRestaurantContext, RestaurantContext>();

//добавляю сервис для обработки всех исключений
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

//сервисы для сессий
builder.Services.AddDistributedMemoryCache(); // Для хранения сессий в памяти
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); //время жизни сессии
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; //важно для GDPR
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

//подключаю сессии
app.UseSession();

//подключаю статические файлы
app.UseDefaultFiles();
app.UseStaticFiles();
//подключаю отлов всех исключений
app.UseExceptionHandler();

//подключаю логгирование всех запросов
app.UseLoggingMiddleware();

app.UseRoutingMiddleware();
app.UseRestaurantMiddleware();

app.Run(async (context) => await context.Response.WriteAsync("Добро пожаловать"));

app.Run();
