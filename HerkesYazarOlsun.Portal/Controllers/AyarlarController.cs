
using DocumentFormat.OpenXml.EMMA;
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

                if (!string.IsNullOrEmpty(input.Profile.ProfilResimName) && input.Profile.ProfilResimName == file.FileName)
                {

                    string base64String = Convert.ToBase64String(fileBytes);
                    input.Profile.ProfilResimBase64 = base64String;
                }

            }

            return input;
        }

        [HttpPost]
        //[Route("SaveOrUpdateAyarlar")]
        public async Task<JsonResult> SaveOrUpdateAyarlar(VM_AYARLAR ayr)
        {
            ayr.LoginUserId = LOGIN_USER_ID;//6;
            //LOGIN_USER_ID;

            // Deserialize JSON strings to their respective objects
            if (Request.Form.ContainsKey("UserDetail"))
            {
                ayr.UserDetail = JsonConvert.DeserializeObject<UsersDetails>(Request.Form["UserDetail"]);
            }
            if (Request.Form.ContainsKey("Profile"))
            {
                ayr.Profile = JsonConvert.DeserializeObject<Profil>(Request.Form["Profile"]);
            }
            if (Request.Form.ContainsKey("User"))
            {
                ayr.User = JsonConvert.DeserializeObject<Users>(Request.Form["User"]);
            }
            if (Request.Form.ContainsKey("Bildirim"))
            {
                ayr.Bildirim = JsonConvert.DeserializeObject<Bildirimler>(Request.Form["Bildirim"]);
            }

            var files = ayr.dosyalar?.FirstOrDefault();
            if (files != null)
            {
                ayr = await ModelIlgiliDosyalariDoldur(ayr, files);                
            }
            
            ayr.dosyalar = null;
            var sonuc = new AyarlarService().SaveOrUpdateAyarlar(ayr);

            return Json(sonuc);
        }


    }
}
