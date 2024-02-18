using HerkesYazarOlsun.Portal.Helpers;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMvc();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IClaimsTransformation, UserClaimProvider>();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    x.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(x =>
{
    x.Cookie.Name = "login";
    x.LoginPath = "/Account/Login";
    x.ExpireTimeSpan = TimeSpan.FromMinutes(5);
});

var app = builder.Build();


//IHostingEnvironment env = new HostingEnvironment();
//RotativaConfiguration.Setup(env);

AppSettings.ApiPath = builder.Configuration.GetSection("AppSettings").GetSection("ApiPath").Value;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//app.UseMiddleware<RequestExceptionMiddleware>();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseCookiePolicy();
//app.UseSession();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
});




app.MapRazorPages();
app.MapControllers();
app.Run();
