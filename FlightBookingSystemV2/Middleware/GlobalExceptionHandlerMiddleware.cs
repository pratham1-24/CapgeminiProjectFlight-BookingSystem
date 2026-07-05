using System.Net;
using System.Text.Json;

namespace FlightBookingSystem.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next   = next;
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
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (status, message) = ex switch
            {
                ArgumentException           => (HttpStatusCode.BadRequest,           ex.Message),
                KeyNotFoundException        => (HttpStatusCode.NotFound,             ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,         ex.Message),
                InvalidOperationException   => (HttpStatusCode.Conflict,             ex.Message),
                _                           => (HttpStatusCode.InternalServerError,  "An unexpected error occurred.")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)status;

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message    = message,
                statusCode = (int)status,
                timestamp  = DateTime.UtcNow
            }));
        }
    }

    // Extension method for clean registration in Program.cs
    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
