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

var app = builder.Build();
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
