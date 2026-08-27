using RestaurantOrders.Application;
using RestaurantOrders.Domain.Users;
using RestaurantOrders.Infrastructure;
using RestaurantOrders.Infrastructure.Persistence;
using RestaurantOrders.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("Admin", policy => policy.RequireRole(Roles.Admin));
    foreach (var permission in new[]
    {
        Permissions.RestaurantManage, Permissions.RestaurantPublish, Permissions.RestaurantDelete,
        Permissions.ReservationManage, Permissions.OrderManage, Permissions.ReviewModerate,
        Permissions.AdminUsersManage
    })
        options.AddPolicy(permission, policy => policy.RequireRole(Roles.Admin));
    foreach (var permission in new[]
    {
        Permissions.ReservationCreate, Permissions.OrderCreate, Permissions.ReviewCreate
    })
        options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser());
});
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().InitializeAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ApiRoutingMiddleware>();
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
