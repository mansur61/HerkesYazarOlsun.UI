using AutoMapper;
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Enums;
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
            if (arama.FavoriYazarlar.HasValue)
            {
                arama.yazarIId = Lid;
            }

            ViewBag.FavoriYazarlar = arama.FavoriYazarlar;

            vmUsers.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            List<VM_USERS> usersList = new KisiService().GetKisiler(arama).ToList();
            vmUsers.VMUsersList = usersList;//.Where(p=>p.ID != Lid).ToList();

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
            var result = new KisiService().PostFavoriSaveWriter(fav_yazar);

            return Json(result);
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
            long id = Lid;

            if (!string.IsNullOrEmpty(pId))
            { 
                id = Convert.ToInt64(StringCipher.Decrypt(pId.ToString()));
            } 
             
             Users kisi = new KisiService().GetKisiById(id); 
            //var profile = new ProfilService().GetProfilByLoginId(kisi.ID);
            var vm_profil = ObjectMapper.Map(kisi.Profil, new VM_PROFILE());
            var vm_kisi = ObjectMapper.Map(kisi, new VM_USERS());

            Users kisiL = new KisiService().GetKisiById(Lid);
            var vm_kisi_L = ObjectMapper.Map(kisiL, new VM_USERS());

            vm_kisi.Profile = vm_profil;
            vm_kisi.Stars = new KisiService().GetMaxStarWriterById(id);
            //ViewBag.vMWriterFollow = new KisiService().GetWriterFollowById(id);

            if (!string.IsNullOrEmpty(pId))
            {
                // başkasının profilini gir tekipte misin  bak
                ViewBag.IsTakip = vm_kisi_L.WriterFollowLoginList?.Where(p => p.LoginUserId == Lid && p.isFollow == (int)Takip.TakipEt
                && p.YazarId == kisi.ID).Any() ?? false;
            }
            else
            {
               ViewBag.IsTakip = vm_kisi.WriterFollowLoginList?.Where(p => p.LoginUserId == id
                && p.isFollow == (int)Takip.TakipEt ).Any() ?? false;//&& p.YazarId != Lid
            }


                Users lgn_kisi = new KisiService().GetKisiById(Lid);

            ViewBag.LOGIN_USER_ID = Lid;
            ViewBag.LGN_USER = lgn_kisi;


            var Bildirim = new BildirimlerService().GetBildirimlerByLoginId(Lid);

            ViewBag.isBildirimTakip = Bildirim?.IsTakip ?? false; //Birisi beni takip ettiğinde bana e-posta gönder 

            return View(vm_kisi);
            
        }
        

    }
}
