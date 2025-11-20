using System.Net;
using System.Text.Json;

namespace WebApplication1.Middleware
{
    /// <summary>
    /// Middleware for secure error handling - prevents information disclosure
    /// Following OWASP and PCI-DSS guidelines
    /// </summary>
    public class SecureErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecureErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public SecureErrorHandlingMiddleware(
            RequestDelegate next, 
            ILogger<SecureErrorHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the full exception (server-side only)
            _logger.LogError(exception, 
                "Unhandled exception occurred. Request: {Method} {Path}", 
                context.Request.Method, 
                context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Never expose sensitive information to clients
            var response = new
            {
                error = "An error occurred while processing your request.",
                // Only include details in development
                details = _environment.IsDevelopment() ? exception.Message : null,
                requestId = context.TraceIdentifier
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}

