using RestaurantOrdersWebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Map("/fastfood", appBuilder =>
{
    app.UseMiddleware<FastfoodRestaurantMiddleware>();
});

app.Map("/delivery", appBuilder =>
{
    app.UseMiddleware<DeliveryRestaurantMiddleware>();
});

app.Map("/premium", appBuilder =>
{
    app.UseMiddleware<PremiumRestaurantMiddleware>();
});

app.Run(async (context) => await context.Response.WriteAsync("Добро пожаловать"));

app.Run();
