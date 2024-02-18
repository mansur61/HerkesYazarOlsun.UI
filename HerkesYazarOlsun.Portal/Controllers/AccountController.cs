
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
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace HerkesYazarOlsun.Portal.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        
        private IWebHostEnvironment _environment;
        public AccountController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) 
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(VM_LOGIN login)
        {
            //if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            //{
            //    RedirectToActionResult redirectResult = new RedirectToActionResult("Index", "Home", new object { });
            //    return redirectResult;
            //}

            ServiceResult<Users> sonuc = new KisiService().GetKisiByMail(login.email ?? "");
           
            if (sonuc.State == MessageResultState.SUCCESS)
            {

                login.ExpiresUtc = login.RememberLogin == true ?  DateTime.UtcNow.AddDays(3) : DateTime.UtcNow; 
                login.AllowRefresh = login.RememberLogin != true ? false : true;
                login.IsPersistent = login.RememberLogin != true ? false : true;
                login.LoginUserId = sonuc.Result.ID;

                ServiceResult sonuc2 = new KisiService().SaveOrUpdateAccountLogin(login);
                sonuc.State = sonuc2.State;

                var ip = (HttpContext.Request.Headers["X-Forwarded-For"].ToString() != null
                         && HttpContext.Request.Headers["X-Forwarded-For"].ToString() != "")
                         ? HttpContext.Request.Headers["X-Forwarded-For"].ToString()
                         : HttpContext?.Connection?.RemoteIpAddress?.ToString();

                HttpContext!.Request.Headers["email"] = sonuc.Result.EMAIL ?? "";

                // yapı tckimlik no üzerinden değil bunun yerine tekil olan email üzerinden ilerlemektedir. Zamanla belki tckimlikno üzerinden ilerleyebilir

                List<Claim> claims = new List<Claim>();
                
                claims.Add(new Claim("telno", sonuc.Result.TELNO ?? ""));
                claims.Add(new Claim("tckimlikno", "0"));
                claims.Add(new Claim("uygulama_id", "1")); //WEB
                claims.Add(new Claim("email", sonuc.Result.EMAIL ?? ""));
                claims.Add(new Claim("ip", ip ?? ""));
               
                
                var identity =  new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                UserClaimProvider userClaimProvider = new UserClaimProvider();
                await userClaimProvider.TransformAsync(principal);

                var props = new AuthenticationProperties();
                props.IsPersistent = login.RememberLogin;
                props.ExpiresUtc = login.ExpiresUtc;
                props.AllowRefresh = login.AllowRefresh;                
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
