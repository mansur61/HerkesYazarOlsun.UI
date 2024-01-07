
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Model.Utils;
using Microsoft.AspNetCore.Authorization;
using HerkesYazarOlsun.Model.Entity;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) : base(environment, httpContextAccessor)
        {
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Login(VM_LOGIN login)
        {


            ServiceResult<Users> sonuc = new KisiService().GetKisiByMail(login.email ?? "");
           
            if (sonuc.State == MessageResultState.SUCCESS)
            {

                var ip = (HttpContext.Request.Headers["X-Forwarded-For"].ToString() != null
                         && HttpContext.Request.Headers["X-Forwarded-For"].ToString() != "")
                         ? HttpContext.Request.Headers["X-Forwarded-For"].ToString()
                         : HttpContext?.Connection?.RemoteIpAddress?.ToString();



                List<Claim> claims = new List<Claim>
                {
                    new Claim("telno", sonuc.Result.TELNO ?? ""),                  
                    new Claim("email", sonuc.Result.EMAIL ?? ""),
                    new Claim("ip", ip ?? ""),
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var props = new AuthenticationProperties();
                props.IsPersistent = login.RememberLogin;
                // props.IsPersistent = false;
                props.ExpiresUtc = DateTime.UtcNow.AddDays(3);
                props.AllowRefresh = true;
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

               
            }

            return Json(sonuc);




        }

        [HttpPost]
        [Route("SaveOrUpdateAyarlar")]
        public JsonResult SaveOrUpdateAyarlar(VM_AYARLAR ayr)
        {

            var sonuc = new AyarlarService().SaveOrUpdateAyarlar(ayr);

            return Json(sonuc);
        }


    }
}
