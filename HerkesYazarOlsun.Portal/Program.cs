using HerkesYazarOlsun.Portal.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMvc();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpClient();
builder.Services.AddAuthentication()
        .AddCookie(options =>
        {
            options.LoginPath = "/Home/Login/";
            options.AccessDeniedPath = "/Home/HataliGiris/";
        })
       ;

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
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
});




app.MapRazorPages();
app.MapControllers();
app.Run();
