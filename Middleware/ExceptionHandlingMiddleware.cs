using System.Text.Json;
using AspNetMinimalCart.Dtos;

namespace AspNetMinimalCart.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "JSON deserialization error: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest,
                "Session Data was invalid or corrupted", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred", ex.Message);
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string message,
        string? details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var error = new ErrorResponse(statusCode, message, details);
        await context.Response.WriteAsJsonAsync(error);
    }
}