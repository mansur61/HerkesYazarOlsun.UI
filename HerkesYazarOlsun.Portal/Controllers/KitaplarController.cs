using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class KitaplarController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult KitapOku()
        {
            return View();
        }

        public JsonResult KitapEkle()
        {

            return Json(2);
        }
    }
}
