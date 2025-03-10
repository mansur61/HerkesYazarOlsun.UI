
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;


namespace HerkesYazarOlsun.Portal.Controllers
{
    public class AramaController : Controller
    {
        private IHttpContextAccessor _contextAccessor;
        private long Lid; 
        public AramaController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            Lid = (long)(_contextAccessor?.HttpContext?.User.GetLoginUserId());
        }
        public IActionResult Arama(VM_ARAMA_INPUT arama)
        {

            VM_ARAMA_SONUC aramaSonuc = new AramaService().TumAramalar(arama);
            aramaSonuc.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;
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
