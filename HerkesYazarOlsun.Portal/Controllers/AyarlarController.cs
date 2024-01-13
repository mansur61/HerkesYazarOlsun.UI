
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class AyarlarController : BaseController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AyarlarController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) : base(environment, httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Ayarlar()
        {
            long loginId = LOGIN_USER_ID;
            VM_AYARLAR vmAyr = new VM_AYARLAR();

            vmAyr.LoginUserId = loginId;
            vmAyr.Profile = new ProfilService().GetProfilByLoginId(loginId);
            vmAyr.UserDetail = new UsersDetailsService().GetUsersDetailsByLoginId(loginId);
            vmAyr.Bildirim = new BildirimlerService().GetBildirimlerByLoginId(loginId);
            vmAyr.User = new KisiService().GetUsersByLoginId(loginId);

            return View(vmAyr);
           
        }

        [HttpPost]
        [Route("SaveOrUpdateAyarlar")]
        public JsonResult SaveOrUpdateAyarlar(VM_AYARLAR ayr)
        {
            ayr.LoginUserId = LOGIN_USER_ID;
            var sonuc = new AyarlarService().SaveOrUpdateAyarlar(ayr);

            return Json(sonuc);
        }


    }
}
