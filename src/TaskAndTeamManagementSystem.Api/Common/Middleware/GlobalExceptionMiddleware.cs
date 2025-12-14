using System.Net;
using System.Text.Json;
using FluentValidation;

namespace TaskAndTeamManagementSystem.Api.Common.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static System.Threading.Tasks.Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An error occurred while processing your request.";
        var errors = new List<string>();

        if (exception is ValidationException validationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = "Validation failed";
            errors = validationException.Errors.Select(e => e.ErrorMessage).ToList();
        }
        else
        {
            errors.Add(exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = message,
            errors = errors,
            timestamp = DateTime.UtcNow
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(jsonResponse);
    }
}

