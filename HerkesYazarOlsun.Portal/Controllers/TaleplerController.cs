using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class TaleplerController : Controller
    {
        [HttpPost]
        public ServiceResult TalepKaydet(Talepler talepler)
        {
            ServiceResult result = new TaleplerService().TalepKaydet(talepler);
            return result;
        }

    }
}
