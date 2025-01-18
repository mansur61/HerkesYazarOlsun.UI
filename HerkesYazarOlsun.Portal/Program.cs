using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
using HerkesYazarOlsun.Portal.Helpers.Extensions; 
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMvc();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IClaimsTransformation, UserClaimProvider>();

builder.Services.Configure<VM_Mail_Settings>(builder.Configuration.GetSection("MailSettings"));
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
 
AppSettings.ApiPath = builder.Configuration.GetSection("AppSettings").GetSection("ApiPath").Value;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
 
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseCookiePolicy();
//app.UseSession();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}); 

app.MapRazorPages();
app.MapControllers();
app.Run();
