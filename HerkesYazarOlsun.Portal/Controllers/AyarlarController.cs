
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class AyarlarController : BaseController
    {
        public AyarlarController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) : base(environment, httpContextAccessor)
        {
        }

        public IActionResult Ayarlar()
        {
            long loginId = 6;
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

            var sonuc = new AyarlarService().SaveOrUpdateAyarlar(ayr);

            return Json(sonuc);
        }


    }
}
