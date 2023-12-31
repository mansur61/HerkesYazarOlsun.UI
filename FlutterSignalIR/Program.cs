using FlutterSignalIR;
using FlutterSignalIR.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

//builder.Services.AddCors();
string url = "http://localhost:5263";

//"http://localhost:5263";
 //"http://localhost:5263";
builder.Services.AddCors(options => options.AddPolicy("AllowAll", //CorsPolicy
               builder =>
               {
                   builder.WithOrigins(url)
                            .AllowAnyHeader()
                          .AllowAnyMethod()
                          .SetIsOriginAllowed((host) => true)
                          .AllowCredentials();
               }));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   // app.UseHsts();
  
}


app.UseAuthentication();
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");


//app.MapPost("test", async (string message,IHubContext<TestHub2,ITest2Client> context) =>
//{
//    await context.Clients.All.ReeciveMessage(message);

//    return Results.NoContent();
//});

//app.UseHttpsRedirection();


//app.UseCors();

//app.UseCors("CorsPolicy");
app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseRouting();

app.MapHub<TestHub>("/testHub");

/**app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<TestHub>("/testHub");
    endpoints.MapControllers();
});
*/

app.UseAuthorization();

app.MapRazorPages();

app.Run();
