
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

        private async Task<VM_AYARLAR> ModelIlgiliDosyalariDoldur(VM_AYARLAR input, IFormFile file)
        {

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                if (!string.IsNullOrEmpty(input.Profile.ProfilResimBase64) && input.Profile.ProfilResimName == file.FileName)
                {

                    string base64String = Convert.ToBase64String(fileBytes);
                    input.Profile.ProfilResimBase64 = base64String;
                }

            }

            return input;
        }

        [HttpPost]
        [Route("SaveOrUpdateAyarlar")]
        public async Task<JsonResult> SaveOrUpdateAyarlarAsync(VM_AYARLAR ayr)
        {
            ayr.LoginUserId = LOGIN_USER_ID;

            var files = ayr.dosyalar?.FirstOrDefault();
            if ( files != null)
            {
                ayr = await ModelIlgiliDosyalariDoldur(ayr, files);
            }

            var sonuc = new AyarlarService().SaveOrUpdateAyarlar(ayr);

            return Json(sonuc);
        }


    }
}
