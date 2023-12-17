using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
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

        public IActionResult GetKisiByTC(long tck)
        {
            Users kisi = new KisiService().GetKisiByTC(tck);
            return View(kisi);
        }

        public IActionResult TumYazarlar(VM_ARAMA_INPUT arama)
        {
            VM_USERS vmUsers = new VM_USERS();

            vmUsers.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            List<VM_USERS> usersList = new KisiService().GetKisiler(arama).ToList();
            vmUsers.VMUsersList = usersList;

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

            List<VM_USERS> usersList = new KisiService().GetKisiler(arama).ToList();
            vmUsers.VMUsersList = usersList;

            return View(vmUsers);
        }

        public IActionResult Profil(long id)
        {
            Users kisi = new KisiService().GetKisiById(id);
            var vm_kisi = ObjectMapper.Map(kisi, new VM_USERS());
            return View(vm_kisi);
            
        }
        

    }
}
