
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 
using HerkesYazarOlsun.Model.Utils;
using Microsoft.AspNetCore.Authorization;
using HerkesYazarOlsun.Model.Entity; 

namespace HerkesYazarOlsun.Portal.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;

        private IWebHostEnvironment _environment;

        public string SignInUrl { get { return $"/Home/Index"; } }
        public string SignUpInUrl { get { return $"/Account/Register"; } }
        public AccountController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        [HttpGet]
        public ActionResult Register()
        {
            VM_USERS vmUsers = new VM_USERS();
            return View(vmUsers);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            if (returnUrl != null && returnUrl.Contains("Register"))
            {
                return RedirectToAction("Register", "Account");
            }

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                RedirectToActionResult redirectResult = new RedirectToActionResult("Index", "Home", new object { });
                return redirectResult;
            }

            return View(new VM_LOGIN() { RememberLogin = true }); 

        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GozlemciModLogin(string email)
        {
            ServiceResult result = new ServiceResult(message:"Gözlemci Modda Girişiniz Algılandı",state:MessageResultState.SUCCESS);
            try
            {
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim("gozlemci_mod", "1"));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
            catch (Exception ex)
            {
                result.State = MessageResultState.ERROR;
            }

            //RedirectToActionResult redirectResult = new RedirectToActionResult("Index", "Home", new object { });
            //return redirectResult;
            return Json(result);

        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(VM_LOGIN login)
        {
           
            if (ModelState.IsValid)
            {
                ServiceResult<Users> sonuc = new KisiService().GetKisiByMail(login.email ?? "");
                List<Claim> claims = new List<Claim>();
                if (sonuc.State == MessageResultState.SUCCESS)
                {

                    //login.ExpiresUtc = login.RememberLogin == true ? DateTime.UtcNow.AddDays(3) : DateTime.UtcNow;
                    //login.AllowRefresh = login.RememberLogin != true ? false : true;
                    //login.IsPersistent = login.RememberLogin != true ? false : true;
                    //login.LoginUserId = sonuc.Result.ID;

                    //ServiceResult sonuc2 = new KisiService().SaveOrUpdateAccountLogin(login);
                    //sonuc.State = sonuc2.State;

                    var ip = (HttpContext.Request.Headers["X-Forwarded-For"].ToString() != null
                             && HttpContext.Request.Headers["X-Forwarded-For"].ToString() != "")
                             ? HttpContext.Request.Headers["X-Forwarded-For"].ToString()
                             : HttpContext?.Connection?.RemoteIpAddress?.ToString();

                    HttpContext!.Request.Headers["email"] = sonuc.Result.EMAIL ?? "";

                    // yapı tckimlik no üzerinden değil bunun yerine tekil olan email üzerinden ilerlemektedir. Zamanla belki tckimlikno üzerinden ilerleyebilir

                    claims.Add(new Claim("telno", sonuc.Result.TELNO ?? ""));
                    claims.Add(new Claim("tckimlikno", "0"));
                    claims.Add(new Claim("uygulama_id", "1")); //WEB
                    claims.Add(new Claim("email", sonuc.Result.EMAIL ?? ""));
                    claims.Add(new Claim("ip", ip ?? ""));
                    claims.Add(new Claim("adi", sonuc.Result.NAME ?? ""));
                    claims.Add(new Claim("soyadi", sonuc.Result.SURNAME ?? ""));


                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    var b = login.RememberLogin; 
                    var props = new AuthenticationProperties();
                    props.IsPersistent = true; 
                        //login.RememberLogin;
                    props.ExpiresUtc = DateTime.UtcNow.AddDays(3);
                    props.AllowRefresh = true;
                    
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

                }
                return Json(sonuc);
            }
            return View();
        }

        [HttpPost]
        [Route("SaveOrUpdateAyarlar")]
        public JsonResult SaveOrUpdateAyarlar(VM_AYARLAR ayr)
        {

            var sonuc = new AyarlarService().SaveOrUpdateAyarlar(ayr);

            return Json(sonuc);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("Cookies");

            return Redirect(SignInUrl);
        }
    }
}
