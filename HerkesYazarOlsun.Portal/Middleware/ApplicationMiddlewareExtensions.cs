namespace HerkesYazarOlsun.Portal.Middlewares
{
    public static class ApplicationMiddlewareExtensions
    {
        public static WebApplication UseApplicationPipeline(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy();

            return app;
        }
    }
}
