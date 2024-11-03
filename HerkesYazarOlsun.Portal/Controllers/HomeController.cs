using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using HerkesYazarOlsun.Portal.Helpers.Extensions;

namespace HerkesYazarOlsun.Portal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;

        private IWebHostEnvironment _environment;
        public HomeController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public string SignInUrl { get { return $"/Home/Login"; } }
        [AllowAnonymous]
        public ActionResult Index()
        {            

            VM_BOOKS vM_BOOKS = new VM_BOOKS();
            var getBookList = new BooksService().GetBooksList();
            vM_BOOKS.VMBooksList = getBookList!;
            var bb = vM_BOOKS.VMBooksList.Where(p => p.Stars.HangiStar == "yildiz2").ToList();

            List<string> kitaplar = new List<string>();
           
            return View(vM_BOOKS);

            //var mail = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Equals("email"));
            //if (mail != null)
            //{

            //    VM_BOOKS vM_BOOKS = new VM_BOOKS();
            //    var getBookList = new BooksService().GetBooksList();
            //    vM_BOOKS.VMBooksList = getBookList!;


            //    List<string> kitaplar = new List<string>();
            //    kitaplar.Add("Yayına En Yakın Olan Kitaplar");
            //    kitaplar.Add("En Çok Okunan  Kitaplar");
            //    kitaplar.Add("En Çok Beğenilen Kitaplar");
            //    return View(vM_BOOKS);

            //}
            //else
            //{

            //    return Redirect(SignInUrl);
            //}

        }

        
        [AllowAnonymous]
        public ActionResult Login()
        {
            VM_LOGIN vM_LOGIN = new VM_LOGIN();
            vM_LOGIN.RememberLogin = false;
            return View(vM_LOGIN);
        }

        public ActionResult HataliGiris()
        {
            //Localde çalıştırmak için kullanılır... Sonraya doğru işe yarar
            //if (Request.Host.Host.Contains("localhost") || (Request.Host.Host.Contains("emadentest.mapeg.gov.tr") && tck.HasValue))
            //{
            //    await loginProcess(tck.Value);
            //}
            //return Redirect("/Home/Index");
            return View();
        }


        public ActionResult Dogrulama()
        {
            return View();
        }
        public ActionResult Register()
        {
            VM_USERS vmUsers = new VM_USERS();
            return View(vmUsers);
        }

        [HttpPost]
        public JsonResult SaveRegister(VM_USERS user)
        {
            ServiceResult result = new ServiceResult();
            if (user.USERNAME == null)
            {
                result.State = MessageResultState.WARNING;
                result.Message = "Kullanıcı Adı Boş GEÇİLEMEZ";
                return Json(result);
            }
            if (user.PASSWORD == null)
            {
                result.State = MessageResultState.WARNING;
                result.Message = "Şifre Belirleyiniz";
                return Json(result);

            }
            if (user.EMAIL == null)
            {
                result.State = MessageResultState.WARNING;
                result.Message = "Email Boş GEÇİLEMEZ";
                return Json(result);

            }

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            byte[] dizi = Encoding.UTF8.GetBytes(user.PASSWORD);

            dizi = md5.ComputeHash(dizi);

            StringBuilder sb = new StringBuilder();

            foreach (byte ba in dizi)
            {
                sb.Append(ba.ToString("x2").ToLower());
            }

            user.PASSWORD = sb.ToString();

            result = new KisiService().PostKisiSave(user);

            return Json(result);

        }

        public ActionResult SifremiUnuttum()
        {
            return View();
        }

    }
}
