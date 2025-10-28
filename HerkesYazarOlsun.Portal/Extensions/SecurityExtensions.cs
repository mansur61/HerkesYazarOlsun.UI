
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

public static class SecurityExtensions
{
    public static void AddCustomSecurity(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        // AppSettings.ApiPath örneğin burada set edilebilir (kendi kodundan al)
        // AppSettings.ApiPath = configuration.GetSection("AppSettings")["ApiPath"];

        // --- Session ---
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;            // JS ile okunamaz
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax; // veya Strict/None ihtiyaca göre
        });

        // --- Cookie (form login) ---
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
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
            options.HeaderName = "X-CSRF-TOKEN";
        });

        // --- Authentication: Cookie + JwtBearer ---
        // IMPORTANT: Eğer Default scheme değiştirilirse mevcut cookie tabanlı formlar etkilenebilir.
        // Burada CookieAuthenticationDefaults.AuthenticationScheme'i varsayılan olarak bırakıyoruz.
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
        //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        //{
        //    // JWT seçeneklerini appsettings.json içinde tut
        //    var jwtSection = configuration.GetSection("Jwt");
        //    var key = jwtSection["Key"] ?? "change_this_to_strong_value";
        //    var issuer = jwtSection["Issuer"] ?? "yourIssuer";
        //    var audience = jwtSection["Audience"] ?? "yourAudience";

        //    options.RequireHttpsMetadata = true;
        //    options.SaveToken = false; // token'ı server'da saklamıyoruz
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuer = true,
        //        ValidIssuer = issuer,
        //        ValidateAudience = true,
        //        ValidAudience = audience,
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        //        ValidateLifetime = true,
        //        ClockSkew = TimeSpan.FromMinutes(2)
        //    };
        //
        //});

        // --- Authorization policy: cookie OR jwt (isteğe bağlı) ---
        //builder.Services.AddAuthorization(options =>
        //{
        //    // Bu policy, hem cookie hem de jwt ile authenticate olmuş kullanıcıyı kabul eder.
        //    options.AddPolicy("CookieOrJwt", policy =>
        //    {
        //        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
        //        policy.RequireAuthenticatedUser();
        //    });

        //    // Diğer policy'leri buraya ekleyebilirsiniz.
        //});

        // --- MVC / Razor ---
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // DI örnekleri (senin kodundaki gibi)
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        // builder.Services.AddScoped<IClaimsTransformation, UserClaimProvider>(); // eğer varsa
        builder.Services.AddHttpClient();
    }

    public static void UseCustomSecurity(this WebApplication app)
    {
        var env = app.Environment;

        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // Güvenli header'lar (CSP, X-Frame-Options, nosniff, vs.)
        app.Use(async (context, next) =>
        {
            // Basit CSP; ihtiyaca göre genişlet
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; object-src 'none';";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            await next();
        });

        app.UseHttpsRedirection();

        // static files ayarı örneği (aynı senin örneğin)
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.Mappings.ContainsKey(".mp3")) provider.Mappings[".mp3"] = "audio/mpeg";
        if (!provider.Mappings.ContainsKey(".ogg")) provider.Mappings[".ogg"] = "audio/ogg";
        if (!provider.Mappings.ContainsKey(".wav")) provider.Mappings[".wav"] = "audio/wav";

        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = provider
        });

        // belgeler dizini örneği (production)
        if (!env.IsDevelopment())
        {
            var belgelerPath = Path.Combine(app.Environment.ContentRootPath, "Belgeler");
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

        // Routing + Session + Auth
        app.UseRouting();

        // Session, Cookie vb.
        app.UseSession();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // CookiePolicy (opsiyonel)
        app.UseCookiePolicy();

        // Map endpoints (örnek)
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Tanitim}/{id?}");

        app.MapRazorPages();
    }
}
