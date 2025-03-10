 using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services; 
using Microsoft.AspNetCore.Mvc;   
using Microsoft.AspNetCore.Authorization;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using Microsoft.AspNetCore.Http;

namespace HerkesYazarOlsun.Portal.Controllers
{
   // [Authorize]
    public class HomeController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;

        private IWebHostEnvironment _environment;
        private long Lid;
        public HomeController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            Lid = (long)(_httpContextAccessor?.HttpContext?.User.GetLoginUserId());
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
            string isGozlemciMod = User.GetGozlemciMod(); 
            if (!string.IsNullOrEmpty(isGozlemciMod) && isGozlemciMod == "1") // gözlemci mod ile gelinmiş
            {
                var vM_BOOKS = GetListBooks();
                vM_BOOKS.isAnaSayfa  = true;

                List<VM_CAROUSEL_DUYURU> list = new CarouselDuyuruService().GetDuyurular();
                ViewBag.Duyurular = list;
                List<VM_SPONSORLAR> spnlist = new SponsorlarService().GetSponsorlar();
                ViewBag.Sponsorlar = spnlist;
                return View(vM_BOOKS);
            }

            if (!string.IsNullOrEmpty(User.GetEmail()))
            {
                var vM_BOOKS = GetListBooks();
                vM_BOOKS.isAnaSayfa = true;

                List<VM_CAROUSEL_DUYURU> list = new CarouselDuyuruService().GetDuyurular();
                ViewBag.Duyurular = list;
                List<VM_SPONSORLAR> spnlist = new SponsorlarService().GetSponsorlar();
                ViewBag.Sponsorlar = spnlist;

                //alternaatif çözüm
               // HttpContext.Session.SetInt32("LOGIN_USER_ID", int.Parse(Lid.ToString())); 
                //@Session["LOGIN_USER_ID"] view içinde bu şekilde kullanılır
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
        public ActionResult Iletisim()
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
