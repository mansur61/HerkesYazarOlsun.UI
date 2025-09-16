using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configuration ayarlarý
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

AppSettings.ApiPath = builder.Configuration.GetSection("AppSettings")["ApiPath"];

// Session desteði
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC ve Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// DI ayarlarý
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IClaimsTransformation, UserClaimProvider>();
builder.Services.Configure<VM_Mail_Settings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddHttpClient();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "login";
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Production ayarlarý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// wwwroot içindeki statik dosyalar
app.UseStaticFiles();

// **Production’da Belgeler klasörünü servis et**
if (!app.Environment.IsDevelopment())
{
    var belgelerPath = Path.Combine(builder.Environment.ContentRootPath, "Belgeler");
    if (Directory.Exists(belgelerPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(belgelerPath),
            RequestPath = "/Belgeler",
            ServeUnknownFileTypes = true // PDF, DOCX vs için
        });
    }
    else
    {
        Console.WriteLine($" Belgeler klasörü bulunamadý: {belgelerPath}");
    }
}

app.UseRouting();

// Middleware sýrasý kritik
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseCookiePolicy();

// Route ayarlarý
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
