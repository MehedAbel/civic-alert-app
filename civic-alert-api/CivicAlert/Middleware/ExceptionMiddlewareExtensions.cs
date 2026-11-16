using CivicAlert.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace CivicAlert.Middleware;

public static class ExceptionMiddlewareExtensions
{
    public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerFeature?.Error;

                var (statusCode, message) = exception switch
                {
                    UserAlreadyExistsException ex => (StatusCodes.Status409Conflict, ex.Message),
                    InvalidRegisterCredentialsException ex => (StatusCodes.Status400BadRequest, ex.Message),
                    InvalidLoginCredentialsException ex => (StatusCodes.Status401Unauthorized, ex.Message),
                    ReportNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
                    UnauthorizedAccessException ex => (StatusCodes.Status403Forbidden, ex.Message),
                    _ => (StatusCodes.Status500InternalServerError, exception?.Message ?? "An unexpected server error occurred.")
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode,
                    message,
                    timestamp = DateTime.UtcNow
                });
            });
        });
    }
}
