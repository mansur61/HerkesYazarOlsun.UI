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

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) : base(environment, httpContextAccessor)
        {
        }

        public ActionResult Index()
        {
            VM_BOOKS vM_BOOKS = new VM_BOOKS();

            var getBookList = new BooksService().GetBooksList();
            vM_BOOKS.VMBooksList = getBookList!;
            

            List<string> kitaplar = new List<string>();
            kitaplar.Add("Yayına En Yakın Olan Kitaplar");
            kitaplar.Add("En Çok Okunan  Kitaplar");
            kitaplar.Add("En Çok Beğenilen Kitaplar");
            return View(vM_BOOKS);
        }

        //[HttpPost]
        [AllowAnonymous]
        public ActionResult Login()
        {
            VM_LOGIN vM_LOGIN = new VM_LOGIN();
            vM_LOGIN.RememberLogin = false;
           // vM_LOGIN.benihatirla = "1";
            return View(vM_LOGIN);
        }

        public ActionResult HataliGiris()
        {            
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
