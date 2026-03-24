using RestaurantOrdersWebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseRoutingMiddleware();

app.Run(async (context) => await context.Response.WriteAsync("Добро пожаловать"));

app.Run();
