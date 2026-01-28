using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
// Configuration ayarları
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

AppSettings.ApiPath = builder.Configuration.GetSection("AppSettings")["ApiPath"];

builder.Services.Configure<HelperSettings>(
    builder.Configuration.GetSection("HelperSettings"));
 
builder.Services.AddRazorPages();

// DI ayarları
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IClaimsTransformation, UserClaimProvider>();
builder.Services.Configure<VM_Mail_Settings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddHttpClient();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "login";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;
    
    options.LoginPath = "/Account/Giris";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
 

var app = builder.Build();

app.UseHttpsRedirection();

// 🔐 Security headers & CSP
app.UseSecurityHeaders(app.Environment, app.Configuration);

// 📦 Static files
app.UseCustomStaticFiles(app.Environment);

// 🔁 Core pipeline
app.UseApplicationPipeline();
 

// 🚦 Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Tanitim}/{id?}");

app.MapRazorPages();

app.Run();
