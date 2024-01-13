
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    //[Authorize]
    public class BaseController : Controller
    {
        public long YETKILITCNO { get; set; }
        public string EMAIL { get; set; }
        public long LOGIN_USER_ID { get; set; }
        public bool isEmail { get; set; }
        public IHttpContextAccessor _httpContextAccessor;
        public IWebHostEnvironment _environment;
        private KisiService _kisiService;
        public BaseController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _kisiService = new KisiService();
            //var tckimlikno = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Equals("tckimlikno"));
            //if (tckimlikno != null)
            //{
            //    YETKILITCNO = Convert.ToInt64(tckimlikno.Value);
            //}
            //else
            //{
            //    httpContextAccessor.HttpContext.Response.StatusCode = 403;
            //    throw new Exception("Giriş Yapınız..");
            //}

            var mail = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Equals("email"));
            if (mail != null)
            {
                EMAIL = mail.Value.ToString();
                isEmail = true;
                var ilgiliKisi = _kisiService.GetKisiByMail(mail.Value);
                if (ilgiliKisi.State == Model.Utils.MessageResultState.SUCCESS)
                {
                    LOGIN_USER_ID = ilgiliKisi.Result.ID;

                }
            }
            else
            {

                httpContextAccessor.HttpContext.Response.StatusCode = 403;
                isEmail = false;
                //httpContextAccessor.HttpContext.Request.Path = "/Home/Login";
                //System.Diagnostics.Process.Start("/Home/Login/");
                //throw new Exception("Giriş Yapınız..");
            }
        }

    }
}
