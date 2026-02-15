using Microsoft.AspNetCore.Http;

namespace HerkesYazarOlsun.Portal.Middlewares
{
    public static class SecurityHeadersMiddleware
    {
        public static IApplicationBuilder UseSecurityHeaders(
            this IApplicationBuilder app,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            app.Use(async (context, next) =>
            {
                var cspList = env.IsDevelopment()
                    ? configuration.GetSection("CSP:Development").Get<string[]>()
                    : configuration.GetSection("CSP:Prod").Get<string[]>();

                if (cspList != null && cspList.Length > 0)
                {
                    context.Response.Headers["Content-Security-Policy"]
                        = string.Join("; ", cspList) + ";";
                }

                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";

                await next();
            });

            return app;
        }
    }
}
