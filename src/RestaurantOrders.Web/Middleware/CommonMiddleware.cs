using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrders.Domain.Common;

namespace RestaurantOrders.Web.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string Header = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[Header].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
        context.TraceIdentifier = correlationId;
        context.Response.Headers[Header] = correlationId;
        await next(context);
    }
}

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();
        await next(context);
        logger.LogInformation("{Method} {Path} returned {StatusCode} in {Elapsed}ms ({CorrelationId})",
            context.Request.Method, context.Request.Path, context.Response.StatusCode,
            timer.ElapsedMilliseconds, context.TraceIdentifier);
    }
}

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled request failure ({CorrelationId})", context.TraceIdentifier);
            if (context.Response.HasStarted)
                throw;

            var status = exception is DomainException
                ? StatusCodes.Status422UnprocessableEntity
                : StatusCodes.Status500InternalServerError;
            var problem = new ProblemDetails
            {
                Status = status,
                Title = exception is DomainException ? "Нарушение бизнес-правила" : "Непредвиденная ошибка сервера",
                Detail = exception.Message,
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
