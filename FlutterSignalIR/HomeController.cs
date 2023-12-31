using FlutterSignalIR.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace FlutterSignalIR
{
    public class HomeController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            await new TestHub2().SendMessage("mansur ");
            
            return View();
        }
    }
}
