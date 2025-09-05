using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using System.Text;

namespace RemoteBackupsApp.Infrastructure.Middlewares
{
    public class CspNonceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public CspNonceMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            context.Items["ScriptNonce"] = nonce;

            var csp = new StringBuilder();
            csp.Append("default-src 'self'; ");
            csp.Append($"script-src 'self' 'nonce-{nonce}' https://unpkg.com https://cdnjs.cloudflare.com https://code.jquery.com https://cdn.jsdelivr.net;");
            csp.Append("style-src 'self' https://cdn.jsdelivr.net https://unpkg.com https://cdnjs.cloudflare.com;");
            csp.Append("font-src 'self' https://cdn.jsdelivr.net https://unpkg.com; ");
            csp.Append("img-src 'self' https://ui-avatars.com data:;");
            csp.Append("base-uri 'self'; ");
            csp.Append("form-action 'self'; ");
            csp.Append("frame-ancestors 'none'; ");
            csp.Append("report-uri /csp-report-endpoint; ");

            if (_env.IsDevelopment())
            {
                csp.Append("connect-src 'self' http://localhost:* https://localhost:* ws://localhost:* wss://localhost:* https://cdnjs.cloudflare.com;");
            }
            else
            {
                csp.Append("connect-src 'self'; ");
            }

            context.Response.Headers["Content-Security-Policy"] = csp.ToString();

            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("Referrer-Policy", "no-referrer");

            await _next(context);
        }
    }
}
