
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class EmailController : Controller
    {
        EmailService _emailService;
        private readonly VM_Mail_Settings _mailSettings;
        private IHttpContextAccessor _httpContextAccessor;

        private IWebHostEnvironment _environment;
        public EmailController(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, IOptions<VM_Mail_Settings> mailSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _emailService = new EmailService();
            _mailSettings = mailSettings.Value;
        }

        public IActionResult EmailDogrula()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EmailOtp(VM_OTP otp)
        {
            otp.baslik = "Email Doğrulaması";
            otp.alt_baslik = "Mailinize gelen kodu giriniz";
            otp.tip = "email";
            return PartialView(otp);
        }


        [HttpPost]
        public async Task<ServiceResult> Gonder(string kime)
        {
            ServiceResult result = new ServiceResult();

            VM_USERS vM_USERS = new VM_USERS();
            vM_USERS.EMAIL = kime;

            var rnd1 = new Random().Next(0, 10);
            var rnd2 = new Random().Next(0, 10);
            var rnd3 = new Random().Next(0, 10);
            var rnd4 = new Random().Next(0, 10);

            string kod = rnd1.ToString() + "" + rnd2.ToString() + "" + rnd3.ToString() + "" + rnd4.ToString();

            var icerik = new VM_MAIL_ICERIK()
            {

                username = _mailSettings.Username,
                password = _mailSettings.Password,
                Host = _mailSettings.Host,
                Port = _mailSettings.Port,
                EnableSSL = _mailSettings.EnableSSL,
                sifre = kod,
                kime = kime,
                konu = _mailSettings.Subject,
                gondericii_mail = _mailSettings.FromEmail
            };

            string sonuc = await _emailService.EmailGonder(icerik);

            if (sonuc == "-1")
            {
                result.State = MessageResultState.ERROR;
                result.Message = "Mail Doğrulama Başarısız";
            }
            else
            {
                result.State = MessageResultState.SUCCESS;
                result.Message = kod;
            }


            return result;
        }

        [HttpPost]
        public ServiceResult PostEmailDogrula(string email)
        {
            VM_USERS vM_USERS = new VM_USERS();
            vM_USERS.EMAIL = email;

            ServiceResult result = new KisiService().PostKisiUpdate(vM_USERS);
            if (result.State != MessageResultState.SUCCESS)
            {
                return result;
            }
            return result;
        }


        [HttpPost]
        public async Task<ServiceResult> MailBilgilendirme(string kime, string konu, string mesaj, int? tip = 0)
        {
            ServiceResult result = new ServiceResult();

            VM_USERS vM_USERS = new VM_USERS();
            vM_USERS.EMAIL = kime;

            if (tip == 0) // mesaj atan kişi bilgilendirmesi
            {

            }
            else // takip eden kişi bilgilendirmesi
            {

            }

            var mail_icerik = new VM_MAIL_ICERIK()
            {

                username = _mailSettings.Username,
                password = _mailSettings.Password,
                Host = _mailSettings.Host,

                icerik = mesaj,
                kime = kime,
                konu = konu,
                gondericii_mail = _mailSettings.FromEmail
            };

            string sonuc = await _emailService.EmailGonder(mail_icerik);

            if (sonuc == "-1")
            {
                result.State = MessageResultState.ERROR;
                result.Message = sonuc;
            }
            else
            {
                result.State = MessageResultState.SUCCESS;
                result.Message = "Bilgilendime İletildi.";
            }


            return result;
        }

    }
}
