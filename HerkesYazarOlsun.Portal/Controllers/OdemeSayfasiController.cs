
 using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
 

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class OdemeSayfasiController : Controller
    {

        private EmailService emailService;
       
        private IHttpContextAccessor _contextAccessor;
        private long Lid;
        public OdemeSayfasiController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            Lid = (long)(_contextAccessor?.HttpContext?.User.GetLoginUserId());
            this.emailService = new EmailService();
        }
        public IActionResult Odeme()
        {
            return View();
           
        }

        [HttpPost]
        public JsonResult SaveOdeme(VM_KARTLAR kart)
        {
            ServiceResult result = new ServiceResult();
            kart.LoginUserId = Lid;
            result = new OdemeService().PostOdeme(kart);
            

            return Json(result);

        }

        public IActionResult SponsorlukBildirimi(string kitapId,string yazarId)
        {
            //kitap ve yazarId şifrelerini çöz
            VM_SPONSORLAR vM_SPONSORLAR = new VM_SPONSORLAR();
            vM_SPONSORLAR.SponsorlarList = new SponsorlarService().GetSponsorlar();
            return View(vM_SPONSORLAR);

        }

        [HttpPost]
        public JsonResult SaveSponsorlukBildir(VM_ODEME_SPONSORLARI odemeSponsorlar)
        {
            ServiceResult result = new ServiceResult();
            odemeSponsorlar.LoginUserId = Lid;
            result = new OdemeService().SaveSponsorlukBildir(odemeSponsorlar);

            if(result.State == MessageResultState.SUCCESS)
            {
                var mesaj = result.Message;
                var icerik = new VM_MAIL_ICERIK()
                {
                    kime = "kayamansur61@gmail.com",
                    Host = "smtp.outlook.com",
                    konu = "Herkes Yazar Olsun Sponsorluk Seçim Bildirimi",
                    icerik = mesaj,//hazırlanmış pdf dosyasıda iletilebilir. şuan bu şekilde ilerle
                    gondericii_mail = "kayamansur61@gmail.com",
                    gondericii_sifre = "Google.*?61",
                };

                string sonuc = emailService.EmailGonder(icerik);


                if (sonuc == "-1")
                {
                    result.State = MessageResultState.ERROR;
                    result.Message = "Sponsorluk bildirme de hata meydana geldi. Tekrar deneyiniz.";
                }
                else
                {
                    result.State = MessageResultState.SUCCESS;
                    result.Message = mesaj;
                }
            }

            return Json(result);

        }

    }
}
