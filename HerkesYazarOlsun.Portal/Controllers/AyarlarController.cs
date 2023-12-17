
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;


namespace HerkesYazarOlsun.Portal.Controllers
{
    public class AyarlarController : Controller
    {
     
        public IActionResult Ayarlar()
        {
            return View();
           
        }

       

    }
}
