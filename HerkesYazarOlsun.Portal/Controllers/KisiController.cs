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
            VM_USERS vmUsers = new VM_USERS();

            vmUsers.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            List<Users> usersList = new KisiService().GetKisiler(arama).ToList();
            vmUsers.UsersList = usersList;

            return View(vmUsers); 
        }


        [HttpPost]
        [Route("YazariFavorilereEkle")]
        public JsonResult YazariFavorilereEkle(int id,long tck)
        {
            VM_FAVORI_YAZARLAR fav_yazar = new VM_FAVORI_YAZARLAR()
            {
                LoginUserId = 1,
                YazarId = id,
                tck = tck
            };
            var getFavori_yazar = new KisiService().PostFavoriSaveWriter(fav_yazar);


            return Json(getFavori_yazar);
        }


        public IActionResult AraButonFiltrelemeYazarlar(VM_ARAMA_INPUT arama)
        {
            VM_USERS vmUsers = new VM_USERS();

            vmUsers.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            List<Users>  usersList = new KisiService().GetKisiler(arama).ToList();
            vmUsers.UsersList = usersList;

            return View(vmUsers);
        }

    }
}
