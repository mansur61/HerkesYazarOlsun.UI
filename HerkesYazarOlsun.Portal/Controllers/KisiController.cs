using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class KisiController : Controller 
    {
        public KisiController()
        {

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetKisiByTC(long id)
        {
            Users kisi = new KisiService().GetKisiByTC(id);
            return View(kisi);
        }

        public IActionResult TumYazarlar(VM_ARAMA_INPUT arama)
        {
            List<Users> kisiler = new KisiService().GetKisiler(arama);
            return View(kisiler); 
        }
    }
}
