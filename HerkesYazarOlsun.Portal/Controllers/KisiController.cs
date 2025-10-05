using AutoMapper;
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class KisiController : Controller 
    {
        private IHttpContextAccessor _contextAccessor;
        private long Lid;
        public KisiController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
             Lid = (long)(_contextAccessor?.HttpContext?.User.GetLoginUserId());
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
            VM_USERS_DETAIL vmUsers = new VM_USERS_DETAIL();

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
                LoginUserId = Convert.ToInt32(Lid),
                YazarId = id,
                tck = tck
            };
            var getFavori_yazar = new KisiService().PostFavoriSaveWriter(fav_yazar);


            return Json(getFavori_yazar);
        }

        [HttpPost]
        [Route("PostWriterFollow")]
        public JsonResult PostWriterFollow(int id, int follow)
        {
            WriterFollow fovllow_yazar = new WriterFollow()
            {
                LoginUserId = Convert.ToInt32(Lid),
                YazarId = id,
                isFollow = follow
                
            };
            var getFavori_yazar = new KisiService().PostWriterFollow(fovllow_yazar);

            return Json(getFavori_yazar);
        }

        [HttpPost]
        [Route("PostWriterStars")]
        public JsonResult PostWriterStars(int id, int puan)
        {

            WriterStars stars_yazar = new WriterStars()
            {
                LoginUserId = Convert.ToInt32(Lid),
                YazarId = id,
                StarPuani = puan

            };
            var getFavori_yazar = new KisiService().PostWriterStars(stars_yazar);

            return Json(getFavori_yazar);
        }

        public IActionResult AraButonFiltrelemeYazarlar(VM_ARAMA_INPUT arama)
        {
            VM_USERS_DETAIL vmUsers = new VM_USERS_DETAIL();

            vmUsers.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            List<VM_USERS> usersList = new KisiService().GetKisiler(arama).ToList();
            vmUsers.VMUsersList = usersList;

            return View(vmUsers);
        }
        // buda reviz edilecek aslında
        public IActionResult Profil(string? pId)
        {
            //var userId = pId != null ? pId : Lid.ToString();
            //var id = Convert.ToInt64(StringCipher.Decrypt(userId.ToString()));
            var id = Lid;

            Users kisi = new KisiService().GetKisiById(id);
            var profile = new ProfilService().GetProfilByLoginId(kisi.ID);

            var vm_kisi = ObjectMapper.Map(kisi, new VM_USERS());

            vm_kisi.Profile = profile;
            vm_kisi.Stars = new KisiService().GetMaxStarWriterById(id);
            ViewBag.vMWriterFollow = new KisiService().GetWriterFollowById(id);

            Users lgn_kisi = new KisiService().GetKisiById(Lid);

            ViewBag.LOGIN_USER_ID = Lid;
            ViewBag.LGN_USER = lgn_kisi;


            var Bildirim = new BildirimlerService().GetBildirimlerByLoginId(Lid);

            ViewBag.isTakip = Bildirim?.IsTakip ?? false; //Birisi beni takip ettiğinde bana e-posta gönder 

            return View(vm_kisi);
            
        }
        

    }
}
