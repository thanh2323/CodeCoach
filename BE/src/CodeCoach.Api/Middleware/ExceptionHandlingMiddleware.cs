using Microsoft.AspNetCore.Mvc;

using CodeCoach.Application.Exceptions;

namespace CodeCoach.Api.Middleware;

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
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected Error")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
