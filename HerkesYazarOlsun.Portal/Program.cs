using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configuration ayarları
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

AppSettings.ApiPath = builder.Configuration.GetSection("AppSettings")["ApiPath"];

/*** Cookie ayarları 
 * SameSite=Strict → Cookie hiçbir cross-site istekte gönderilmez (en katı). Kullanıcı başka siteden geldiğinde oturum cookie gönderilmez.
 * 
 * SameSite=Lax → Güvenli GET navigasyonlarında cookie gönderilebilir (ör. linke tıklama). 
 * POST gibi cross-site state‑değiştiren isteklerde gönderilmez.
 * 
 * SameSite=None; Secure → Cookie cross-site isteklerde de gönderilir (üçüncü taraf), ama Secure olmalı (HTTPS).
 * 
 * ***/

// Session desteği
builder.Services.AddDistributedMemoryCache();
// Session (örnek)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;            // JS ile okunamaz
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax; // veya Strict (uygulamanıza göre)
});

// --- Cookie  yapılandırma ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // veya Lax
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // Varsayılan cookie auth davranışı (login path vb)
    options.Cookie.Name = "login";
    options.LoginPath = "/Account/Giris";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
// --- Antiforgery (CSRF) ---
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // AJAX istekleri için header üzerinden gönder
});

// MVC ve Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// DI ayarları
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IClaimsTransformation, UserClaimProvider>();
builder.Services.Configure<VM_Mail_Settings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddHttpClient();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
      .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
      {
          // Yukarıdaki ConfigureApplicationCookie zaten bu cookie'yi yapılandırdı ama
          // burada tekrar ayar yapmak istersen ekleyebilirsin.
          options.Cookie.Name = "login";
          options.LoginPath = "/Account/Giris";
          options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
          options.SlidingExpiration = true;
      });

var app = builder.Build();

// Production ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Güvenli header'lar (CSP, X-Frame-Options, nosniff, vs.)
/**** 
 * default-src 'self':
Tüm kaynaklar (resim, CSS, JS vb.) sadece kendi domain’inden (aynı origin) yüklenebilir.
Yani başka bir siteden script, iframe, resim çekemezsin.

* script-src 'self':
JavaScript dosyaları sadece kendi domain’inden yüklenebilir.
CDN veya üçüncü parti script (ör. Google Analytics, Bootstrap CDN) engellenir.

*object-src 'none':
<object>, <embed>, <applet> gibi eski HTML etiketlerinden hiçbirine izin verilmez.
Bunlar genelde zararlı içerik yüklemek için kullanılır.
 * 
 * 
 * ***/
// CSP ve diğer güvenlik header'ları
app.Use(async (context, next) =>
{
    var env = app.Environment;
    var configuration = app.Configuration;

    // Ortama göre CSP seç
    // Ortama göre CSP listesini al
    var cspList = env.IsDevelopment()
        ? configuration.GetSection("CSP:Development").Get<string[]>()
        : configuration.GetSection("CSP:Default").Get<string[]>();

    // Dizi varsa string'e birleştir
    string cspPolicy = cspList != null ? string.Join("; ", cspList) + ";" : null;

    // CSP header’ı ekle
    if (!string.IsNullOrEmpty(cspPolicy))
    {
        context.Response.Headers["Content-Security-Policy"] = cspPolicy;
    }

    // Diğer güvenlik header'ları
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    await next();
});

app.UseHttpsRedirection();

// ---------- Static Files Ayarı ----------

// MIME tipleri için provider
var provider = new FileExtensionContentTypeProvider();
if (!provider.Mappings.ContainsKey(".mp3"))
    provider.Mappings[".mp3"] = "audio/mpeg";
if (!provider.Mappings.ContainsKey(".ogg"))
    provider.Mappings[".ogg"] = "audio/ogg";
if (!provider.Mappings.ContainsKey(".wav"))
    provider.Mappings[".wav"] = "audio/wav";

// wwwroot içindeki dosyalar (CSS, JS, resim, mp3, ogg vs)
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// Belgeler klasörü (production için özel)
if (!app.Environment.IsDevelopment())
{
    var belgelerPath = Path.Combine(builder.Environment.ContentRootPath, "Belgeler");
    if (Directory.Exists(belgelerPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(belgelerPath),
            RequestPath = "/Belgeler",
            ContentTypeProvider = provider,
            ServeUnknownFileTypes = true // PDF, DOCX vs için
        });
    }
    else
    {
        Console.WriteLine($" Belgeler klasörü bulunamadı: {belgelerPath}");
    }
}

// ---------- Middleware Sırası ----------
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();

// ---------- Routing ----------
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Tanitim}/{id?}");


app.MapRazorPages();
app.Run();
