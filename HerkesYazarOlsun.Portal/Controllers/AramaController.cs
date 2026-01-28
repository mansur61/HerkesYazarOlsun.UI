using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace HerkesYazarOlsun.Portal.Controllers
{
    public class AramaController : Controller
    {
        private IHttpContextAccessor _contextAccessor;
        private long Lid;
         private int SliderdaGosterilecekKayit = 0;
        private int DefaultSliderdaGosterilecekKayit = 0;
        public AramaController(IHttpContextAccessor contextAccessor,IOptions<HelperSettings> helperSettings)
        {
            _contextAccessor = contextAccessor;
            Lid = (long)(_contextAccessor?.HttpContext?.User.GetLoginUserId());


            SliderdaGosterilecekKayit = helperSettings.Value.SliderdaGosterilecekKayit;
            DefaultSliderdaGosterilecekKayit = helperSettings.Value.DefaultSliderdaGosterilecekKayit;

        }
        public IActionResult Arama(VM_ARAMA_INPUT arama)
        {

            VM_ARAMA_SONUC aramaSonuc = new AramaService().TumAramalar(arama);
            aramaSonuc.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            ViewBag.SliderdaGosterilecekKayit = SliderdaGosterilecekKayit;
            ViewBag.DefaultSliderdaGosterilecekKayit = DefaultSliderdaGosterilecekKayit;

            ViewBag.YayinAyar = new AyarlarService().GetYyainAyarlari();
            ViewBag.LOGIN_USER_ID = Lid;
            return View(aramaSonuc.vmBook);

        }

        [HttpGet]
        [Route("OrtakTumAramalar")]
        public IActionResult OrtakTumAramalar(VM_ARAMA_INPUT arama)
        {

            VM_ARAMA_SONUC aramaSonuc = new AramaService().TumAramalar(arama);
            ViewBag.LOGIN_USER_ID = Lid;
            aramaSonuc.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;
            ViewBag.YayinAyar = new AyarlarService().GetYyainAyarlari();
            return View(aramaSonuc);
        }

    }
}
