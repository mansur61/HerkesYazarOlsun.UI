using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
namespace HerkesYazarOlsun.Portal.Middleware
{
    public class RequestExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var errors = ex.Value?.ToString();// Select(e => new { e.PropertyName, e.ErrorMessage });
                await context.Response.WriteAsync(JsonConvert.SerializeObject(errors));
            }
            catch (BadHttpRequestException ex) // Özel Bad Request hatasını ele alma
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    ErrorName = ex.Message, // Özel hata adını buraya ekleyin
                    Message = ex.Message
                }));
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    Message = ""
                }));
            }
        }
    }
}
