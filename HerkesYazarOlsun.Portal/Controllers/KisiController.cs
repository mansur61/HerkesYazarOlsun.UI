using HerkesYazarOlsun.Model.Entity;
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
            KISILER kisi = new KisiService().GetKisiByTC(id);
            return View(kisi);
        }
    }
}
