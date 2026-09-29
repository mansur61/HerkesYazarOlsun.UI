using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class EminStokController : Controller
    {
        [AllowAnonymous]
        public ActionResult GizlilikSozlezmesi()
        {
            return View();
        }
    }
}
