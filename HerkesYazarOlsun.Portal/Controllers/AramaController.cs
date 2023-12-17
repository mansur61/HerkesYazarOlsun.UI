
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;


namespace HerkesYazarOlsun.Portal.Controllers
{
    public class AramaController : Controller
    {
     
        public IActionResult Arama(VM_ARAMA_INPUT arama)
        {

            VM_ARAMA_SONUC aramaSonuc = new AramaService().TumAramalar(arama);
            aramaSonuc.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            return View(aramaSonuc.vmBook);
           
        }

        [HttpGet]
        [Route("OrtakTumAramalar")]
        public IActionResult OrtakTumAramalar(VM_ARAMA_INPUT arama)
        {

            VM_ARAMA_SONUC aramaSonuc = new AramaService().TumAramalar(arama);
            aramaSonuc.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            return View(aramaSonuc);
        }

    }
}
