using System.Net;
using System.Text.Json;

namespace EcommerceAI.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = new
            {
                Success = false,
                Error = ex switch
                {
                    KeyNotFoundException => "Resource not found",
                    UnauthorizedAccessException => "Unauthorized",
                    ArgumentException => ex.Message,
                    InvalidOperationException => ex.Message,
                    _ => "An unexpected error occurred"
                },
                StatusCode = context.Response.StatusCode
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
