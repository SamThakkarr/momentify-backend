using System.Text.Json;
using momentify_backend.DTOs;

namespace momentify_backend.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ArgumentException =>
                StatusCodes.Status400BadRequest,

            KeyNotFoundException =>
                StatusCodes.Status404NotFound,

            UnauthorizedAccessException =>
                StatusCodes.Status403Forbidden,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var response = new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = exception.Message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}
