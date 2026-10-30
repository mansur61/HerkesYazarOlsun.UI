
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HerkesYazarOlsun.Model.Utils;
using Microsoft.AspNetCore.Authorization;
using HerkesYazarOlsun.Model.Entity;
using Newtonsoft.Json;
using System.Text;

namespace HerkesYazarOlsun.Portal.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;

        private IWebHostEnvironment _environment;

        public string SignInUrl { get { return $"/Home/Index"; } }
        public string Onboarding { get { return $"/Home/Tanitim"; } }
        public string SignUpInUrl { get { return $"/Account/KayitOl"; } }
        public AccountController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        [HttpGet]
        public ActionResult KayitOl()
        {
            VM_USERS vmUsers = new VM_USERS();
            return View(vmUsers);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Giris(string returnUrl)
        {
            if (returnUrl != null && returnUrl.Contains("KayitOl"))
            {
                return RedirectToAction("KayitOl", "Account");
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
            ServiceResult result = new ServiceResult(message: "Gözlemci Modda Girişiniz Algılandı", state: MessageResultState.SUCCESS);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(VM_LOGIN login)
        {
            var logFolder = Path.Combine(_environment.WebRootPath, "herkesyazarolsun_log");
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            try
            {
                if (!ModelState.IsValid)
                    return View();

                // 1. Servisten kullanıcıyı getir (mevcut davranış korunuyor)
                ServiceResult<Users> sonuc = new KisiService().GetKisiByMail(login.email ?? "");

                if (sonuc == null || sonuc.Result == null)
                {
                    return Json(new
                    {
                        Message = "Bir eksiklik var lütfen geliştiricinize başvurunuz veya giriş için kayıt yaptırdığınızdan emin olunuz !",
                        State = MessageResultState.ERROR
                    });
                }

                if (sonuc.State != MessageResultState.SUCCESS)
                    return Json(sonuc);

                // 2. Servis Login endpoint'inden access + refresh token al
                string jwtToken = "";
                string refreshToken = "";
                try
                {
                    using var httpClient = new HttpClient();
                    var loginPayload = JsonConvert.SerializeObject(new { email = login.email, RememberLogin = login.RememberLogin });
                    var content = new StringContent(loginPayload, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"{Helpers.AppSettings.ApiPath}api/Users/Login", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var tokenResult = JsonConvert.DeserializeObject<dynamic>(responseBody);
                        jwtToken = tokenResult?.token ?? tokenResult?.accessToken ?? "";
                        refreshToken = tokenResult?.refreshToken ?? "";
                    }
                }
                catch (Exception ex)
                {
                    // JWT alınamazsa cookie auth ile devam et, logla
                    var logFile = Path.Combine(logFolder, $"jwt_error_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    await System.IO.File.WriteAllTextAsync(logFile, ex.ToString());
                }

                // 3. Cookie claim'leri oluştur
                var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";

                HttpContext.Request.Headers["email"] = sonuc.Result.EMAIL ?? "";

                var claims = new List<Claim>
                {
                    new Claim("telno",        sonuc.Result.TELNO    ?? ""),
                    new Claim("tckimlikno",   "0"),
                    new Claim("uygulama_id",  "1"),
                    new Claim("email",        sonuc.Result.EMAIL    ?? ""),
                    new Claim("ip",           ip),
                    new Claim("adi",          sonuc.Result.NAME     ?? ""),
                    new Claim("soyadi",       sonuc.Result.SURNAME  ?? ""),
                    new Claim("username",     sonuc.Result.USERNAME ?? ""),
                    new Claim("user_id",      sonuc.Result.ID.ToString())
                };

                // Access + Refresh token cookie claim'leri set edilir.
                if (!string.IsNullOrEmpty(jwtToken))
                    claims.Add(new Claim("jwt_token", jwtToken));

                if (!string.IsNullOrEmpty(refreshToken))
                    claims.Add(new Claim("refresh_token", refreshToken));

                var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var props = new AuthenticationProperties
                {
                    IsPersistent = login.RememberLogin,
                    ExpiresUtc   = DateTime.UtcNow.AddDays(30),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

                return Json(sonuc);
            }
            catch (Exception ex)
            {
                var logFile = Path.Combine(logFolder, $"login_error_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                await System.IO.File.WriteAllTextAsync(logFile, ex.ToString());
                return StatusCode(500, "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.");
            }
        }

        [HttpPost]
        [Route("SaveOrUpdateAyarlar")]
        [ValidateAntiForgeryToken]
        public JsonResult SaveOrUpdateAyarlar(VM_AYARLAR ayr)
        {

            var sonuc = new AyarlarService().SaveOrUpdateAyarlar(ayr);

            return Json(sonuc);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CikisYap()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("Cookies");

            return Redirect(Onboarding);
        }
    }
}
