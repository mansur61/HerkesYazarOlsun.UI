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

        public string SignInUrl { get { return $"/Account/Login"; } }

        private VM_BOOKS GetListBooks()
        {
            VM_BOOKS vM_BOOKS = new VM_BOOKS();
            var getBookList = new BooksService().GetBooksList();
            vM_BOOKS.VMBooksList = getBookList!;
            // var bb = vM_BOOKS.VMBooksList.Where(p => p.Stars.HangiStar == "yildiz2").ToList();
            //  List<string> kitaplar = new List<string>();

            return vM_BOOKS;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            //var bak = (long)(_httpContextAccessor?.HttpContext?.User.GetLoginUserId());
            string isGozlemciMod = User.GetGozlemciMod();
            //var bb = User.GetEmail();
            if (!string.IsNullOrEmpty(isGozlemciMod) && isGozlemciMod == "1") // gözlemci mod ile gelinmiş
            {
                var vM_BOOKS = GetListBooks();
                vM_BOOKS.isAnaSayfa  = true;
                return View(vM_BOOKS);
            }

            if (!string.IsNullOrEmpty(User.GetEmail()))
            {
                var vM_BOOKS = GetListBooks();
                vM_BOOKS.isAnaSayfa = true;
                return View(vM_BOOKS);

            }
            else
            {
                return Redirect(SignInUrl);
            }

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
