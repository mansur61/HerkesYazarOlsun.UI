using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers;

public class UstamNeredeController : Controller
{
    [AllowAnonymous]
    public IActionResult GizlilikPolitikasi()
    {
        return View();
    }
}