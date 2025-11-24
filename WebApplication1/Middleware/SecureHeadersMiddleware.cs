using System.Text;

namespace WebApplication1.Middleware
{
    /// <summary>
    /// </summary>
    public class SecureHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecureHeadersMiddleware> _logger;

        public SecureHeadersMiddleware(RequestDelegate next, ILogger<SecureHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("X-AspNet-Version");

            var csp = new StringBuilder();
            csp.Append("default-src 'self'; ");
            csp.Append("script-src 'self' 'unsafe-inline' https://code.jquery.com https://cdn.jsdelivr.net; ");
            csp.Append("style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; ");
            csp.Append("font-src 'self' https://cdn.jsdelivr.net data:; ");
            csp.Append("img-src 'self' data: https:; ");
            csp.Append("connect-src 'self' https://api.apilayer.com; ");
            csp.Append("frame-ancestors 'none'; ");
            csp.Append("base-uri 'self'; ");
            csp.Append("form-action 'self'; ");
            csp.Append("upgrade-insecure-requests; ");
            csp.Append("block-all-mixed-content; ");
            
            context.Response.Headers.Add("Content-Security-Policy", csp.ToString());
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Add("Permissions-Policy", 
                "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=(), accelerometer=()");

            if (context.Request.IsHttps && !context.Response.Headers.ContainsKey("Strict-Transport-Security"))
            {
                context.Response.Headers.Add("Strict-Transport-Security", 
                    "max-age=31536000; includeSubDomains; preload");
            }

            context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
            context.Response.Headers.Add("Cross-Origin-Embedder-Policy", "require-corp");
            context.Response.Headers.Add("Cross-Origin-Opener-Policy", "same-origin");
            context.Response.Headers.Add("Cross-Origin-Resource-Policy", "same-origin");

            await _next(context);
        }
    }
}

