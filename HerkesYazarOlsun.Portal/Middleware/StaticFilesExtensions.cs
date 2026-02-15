    using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace HerkesYazarOlsun.Portal.Middlewares
{
    public static class StaticFilesExtensions
    {
        public static IApplicationBuilder UseCustomStaticFiles(
            this IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            var provider = new FileExtensionContentTypeProvider();

            provider.Mappings.TryAdd(".mp3", "audio/mpeg");
            provider.Mappings.TryAdd(".ogg", "audio/ogg");
            provider.Mappings.TryAdd(".wav", "audio/wav");

            // wwwroot
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });

            // Belgeler klasörü (prod)
            if (!env.IsDevelopment())
            {
                var belgelerPath = Path.Combine(env.ContentRootPath, "Belgeler");

                if (Directory.Exists(belgelerPath))
                {
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(belgelerPath),
                        RequestPath = "/Belgeler",
                        ContentTypeProvider = provider,
                        ServeUnknownFileTypes = true
                    });
                }
            }

            return app;
        }
    }
}
