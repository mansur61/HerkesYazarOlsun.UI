 using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class CarouselDuyuruController : Controller
    {
        public JsonResult GetDuyurular()
        {
            List<VM_CAROUSEL_DUYURU> list = new CarouselDuyuruService().GetDuyurular();
            return Json(list);
        }
    }
}
