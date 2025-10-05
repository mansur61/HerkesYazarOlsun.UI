using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
 
    public class HomeController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;

        private IWebHostEnvironment _environment;
        private IConfiguration _configuration;
        private long Lid;
        public HomeController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, IConfiguration configuratioN)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            Lid = (long)(_httpContextAccessor?.HttpContext?.User.GetLoginUserId());
            _configuration = configuratioN;
        }

        public string SignInUrl { get { return $"/Account/Giris"; } }
 
        [AllowAnonymous]
        public ActionResult Index()
        {
            VM_BOOKS_DETAIL vM_BOOKS = new BooksService().GetBooksList();
            vM_BOOKS.isAnaSayfa = true;
            vM_BOOKS.Start = 0;
            vM_BOOKS.sliderdaGosterilecekKayit = 5;

            string isGozlemciMod = User.GetGozlemciMod(); 
            if (!string.IsNullOrEmpty(isGozlemciMod) && isGozlemciMod == "1") // gözlemci mod ile gelinmiş
            { 
                List<VM_CAROUSEL_DUYURU> list = new CarouselDuyuruService().GetDuyurular();
                ViewBag.Duyurular = list;
                List<VM_SPONSORLAR> spnlist = new SponsorlarService().GetSponsorlar();
                ViewBag.Sponsorlar = spnlist;
                return View(vM_BOOKS);
            }

            if (!string.IsNullOrEmpty(User.GetEmail()))
            { 
                List<VM_CAROUSEL_DUYURU> list = new CarouselDuyuruService().GetDuyurular();
                ViewBag.Duyurular = list;
                List<VM_SPONSORLAR> spnlist = new SponsorlarService().GetSponsorlar();
                ViewBag.Sponsorlar = spnlist;
                 
                ViewBag.LOGIN_USER_ID = Lid;
                ViewBag.YayinAyar = new AyarlarService().GetYyainAyarlari();
                return View(vM_BOOKS);

            }
            else
            {
                return Redirect(SignInUrl);
            }

        }

        public ActionResult Hakkimizda()
        {
            return View();
        }
        public ActionResult Sozlesme()
        {
            return View();
        }

        public ActionResult Iletisim()
        {
            var mail = _configuration.GetSection("AppSettings")["AliciMail"];
            ViewBag.MAIL = mail;
            return View();
        }
        public ActionResult Tanitim()
        {
            return View();
        }
        public ActionResult HataliGiris()
        {
            return View();
        }


        public ActionResult Dogrulama()
        {
            return View();
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

            user.PASSWORD = EncryptionHelper.ComputeSHA256Hash(user.PASSWORD);
              
            result = new KisiService().PostKisiSave(user); 
            return Json(result);

        }

        public ActionResult SifremiUnuttum()
        {
            return View();
        }

    }
}
