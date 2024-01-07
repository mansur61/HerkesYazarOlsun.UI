
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        public long YETKILITCNO { get; set; }
        public IHttpContextAccessor _httpContextAccessor;
        public IWebHostEnvironment _environment;

        public BaseController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            var tckimlikno = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Equals("tckimlikno"));
            if (tckimlikno != null)
            {
                YETKILITCNO = Convert.ToInt64(tckimlikno.Value);
            }
            else
            {
                httpContextAccessor.HttpContext.Response.StatusCode = 403;
                throw new Exception("Giriş Yapınız..");
            }
        }

    }
}
