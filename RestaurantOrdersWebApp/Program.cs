using RestaurantOrdersWebApp.Middleware;
using RestaurantOrdersWebApp.Services.EF_services;
using RestaurantOrdersWebApp.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IOrderService , OrderService>();

var app = builder.Build();

app.UseRoutingMiddleware();
app.UseRestaurantMiddleware();

app.Run(async (context) => await context.Response.WriteAsync("Добро пожаловать"));

app.Run();
