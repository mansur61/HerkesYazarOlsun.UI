namespace HerkesYazarOlsun.Portal.Middlewares
{
    public static class ApplicationMiddlewareExtensions
    {
        public static WebApplication UseApplicationPipeline(this WebApplication app)
        {
            app.UseHttpsRedirection();

            if (!app.Environment.IsDevelopment())
            {
                //app.UseExceptionHandler("/Error"); GlobalExceptionMiddleware yazıldı bu yüzden kullanımdan alındı
                app.UseHsts(); // SSL stripping saldırılarını engeller, MITM riskini azaltır, HTTPS zorunlu hale gelir
            }

            app.UseRouting();

            //app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy();

            return app;
        }
    }
}
