using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Security.Cryptography;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class OdemeSayfasiController : Controller
    {
        private readonly VM_Mail_Settings _mailSettings;
        private readonly HelperSettings helperSettings;
        private EmailService emailService;

       
        private IHttpContextAccessor _contextAccessor;
        private long Lid;
        public OdemeSayfasiController(IHttpContextAccessor contextAccessor, 
            IOptions<VM_Mail_Settings> mailSettings, 
            IOptions<HelperSettings> _helperSettings)
        {
            _contextAccessor = contextAccessor;
            Lid = (long)(_contextAccessor?.HttpContext?.User.GetLoginUserId());
            this.emailService = new EmailService();
            _mailSettings = mailSettings.Value;
            helperSettings = _helperSettings.Value;
        }
        public IActionResult Odeme(string kitapId)
        {

            ViewBag.LoginUserId = StringCipher.Encrypt(Lid.ToString());
            ViewBag.KitapId = kitapId;
            ViewBag.DefaultYayinUcreti = helperSettings.DefaultYayinUcreti;
            return View();
        }


        /**
         * //Ödeme alt yapısına gider. (iyizico vs.) Başarılı ise Ödeme tablosuna kayıt atar. isOdeme durumu belilerlenir.
         *  // buradan ilgili ödeme entegrasyonu sayfasına yönlendir. callback url de bu endpoinmti kullanırsın
         * */
        [HttpPost]
        public JsonResult SaveOdeme(VM_KARTLAR kart)
        {
            ServiceResult result = new ServiceResult();
            kart.LoginUserId = Lid;
            result = new OdemeService().PostOdeme(kart);


            return Json(result);

        }
        public IActionResult SponsorlukBildirimi(string kitapId, string yazarId)
        {
            //var kitap_id = Convert.ToInt64(StringCipher.Decrypt(kitapId));
            //var yazar_id = Convert.ToInt64(StringCipher.Decrypt(yazarId));
            //var bb = Lid;
            ViewBag.LoginUserId = yazarId;
            ViewBag.KitapId = kitapId;
            //kitap ve yazarId şifrelerini çöz
            VM_SPONSORLAR vM_SPONSORLAR = new VM_SPONSORLAR();
            vM_SPONSORLAR.SponsorlarList = new SponsorlarService().GetSponsorlar();
            return View(vM_SPONSORLAR);

        }
     
        [HttpPost]
        public async Task<JsonResult> SaveSponsorlukBildirAsync(VM_ODEME_SPONSORLARI odemeSponsorlar)
        {
            ServiceResult result = new ServiceResult();
            odemeSponsorlar.LoginUserId = Lid; 

            result = new ServiceResult(state: MessageResultState.SUCCESS, message: "Olmadi");
            //new OdemeService().SaveSponsorlukBildir(odemeSponsorlar);

            var spnsModel = new SponsorlarService().GetSponsorlarById(odemeSponsorlar.SponsorId);
            var kisiModel = new KisiService().GetKisiById(Lid);

            if (result.State == MessageResultState.SUCCESS)
            {
                var mesaj = result.Message; 
                 //alinan mail
                 var icerik = new VM_MAIL_ICERIK()
                {
                    username = _mailSettings.Username,
                    password = _mailSettings.Password,
                    Host = _mailSettings.Host,

                    konu = "Herkes Yazar Olsun Sponsorluk Seçim Bildirimi",
                    icerik = $@"
                            <html>
                                <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                                    <h2 style='color: #2c3e50;'>Sayın Herkes Yazar Olsun Ekibi,</h2>
                                    <p>
                                        {odemeSponsorlar.KitapId.ToString()} nolu Kitap bilgisi ile Sponsor seçimi için size ulaşıyorum.
                                        Sponsor Adı olarak  {spnsModel.SponsorAdi} seçtiğimi belirtmek istiyorum.
                                    </p>
                                    <p>
                                        <strong>İletişim Bilgilerim:</strong><br>
                                        <strong>Ad Soyad:</strong> {kisiModel.NAME} {kisiModel.SURNAME}<br>
                                        <strong>Telefon:</strong> {odemeSponsorlar.Tel}<br>
                                        <strong>Mail:</strong> {odemeSponsorlar.Mail}<br>
                                    </p>
                                    <p>
                                        Ek Açıklamalarım;                                       
                                    </p>
                                    <p>
                                        {odemeSponsorlar.Mesaj}
                                    </p>
                                    <hr style='border: 1px solid #ccc;' />
                                    
                                </body>
                            </html>",
                     
                    // mail kimden geliyor
                    gondericii_mail = "kayamansur61@gmail.com", //odemeSponsorlar.Mail,
                    // mail kime gidiyor
                    kime = _mailSettings.FromEmail,
                    //dosyaYolu = tempFilePath,
                    dosyalar = odemeSponsorlar.dosyalar != null &&  odemeSponsorlar.dosyalar.Any() ? odemeSponsorlar.dosyalar : null
                 };

                string sonuc = await emailService.EmailGonder(icerik);


                if (sonuc == "-1")
                {
                    result.State = MessageResultState.ERROR;
                    result.Message = "Sponsorluk bildirme de hata meydana geldi. Tekrar deneyiniz.";
                }
                else
                {
                    //gonderilen nmail
                    icerik = new VM_MAIL_ICERIK()
                    {

                        username = _mailSettings.Username,
                        password = _mailSettings.Password,
                        Host = _mailSettings.Host,

                        konu = "Herkes Yazar Olsun Sponsorluk Seçim Bildirimi Yanıtınız",
                        icerik = $@"
                            <html>
                                <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                                    <h2 style='color: #2c3e50;'>Sayın {odemeSponsorlar.NameSurname}  ,</h2>
                                    <p>
                                        Sponsor seçiminiz başarılı şekilde yapılmıştır.
                                        Paylaştığınız mail veya SMS bilgileri ile sizlere en kısa sürede iletişim sağlanacaktır.
                                    </p>
                                    <p>
                                        {mesaj}
                                    </p>
                                    <p>
                                        Sponsorluk süreçleri hakkında daha fazla bilgi almak için bizimle iletişime geçebilirsiniz.
                                    </p>
                                    <p>
                                        Gelen Maillerde spama düşme ihtimali olmaktadır. Spam klasörünüzü kontrol eylemizide rica ederiz.
                                    </p>
                                    <hr style='border: 1px solid #ccc;' />
                                    <p style='font-size: 0.9em; color: #7f8c8d;'>
                                        Bu mesaj otomatik olarak oluşturulmuştur. Lütfen cevaplamayınız.
                                    </p>
                                </body>
                            </html>",

                        // mail kimden geliyor
                        gondericii_mail = _mailSettings.FromEmail,
                        // mail kime gidiyor
                        kime = odemeSponsorlar.Mail,
                    };

                     sonuc = await emailService.EmailGonder(icerik);


                    if (sonuc == "-1")
                    {
                        result.State = MessageResultState.ERROR;
                        result.Message = "Sponsorluk bildirme de hata meydana geldi. Tekrar deneyiniz.";
                    }
                    else
                    {
                        result.State = MessageResultState.SUCCESS;
                        result.Message = mesaj;
                    }
                    
                }
            }

            return Json(result);

        }

    }
}
