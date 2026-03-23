using RestaurantOrdersWebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Map("/fastfood", appBuilder =>
{
    app.UseFastfoodRestaurantMiddleware();
});

app.Map("/delivery", appBuilder =>
{
    app.UseDeliveryRestaurantMiddleware();
});

app.Map("/premium", appBuilder =>
{
    app.UsePremiumRestaurantMiddleware();
});

app.Run(async (context) => await context.Response.WriteAsync("Добро пожаловать"));

app.Run();
