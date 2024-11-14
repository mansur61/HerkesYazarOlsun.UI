
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class EmailController : Controller
    {
        EmailService emailService;
        EmailController(EmailService emailService)
        {
            this.emailService = emailService;
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
        public ServiceResult Gonder(string kime)
        {
            ServiceResult result = new ServiceResult();
            //kime = "emreyunus616195@gmail.com";
            VM_USERS vM_USERS = new VM_USERS();
            vM_USERS.EMAIL = kime;

            // bu kısmı yarın test et
            ServiceResult updateResult = new KisiService().PostKisiUpdate(vM_USERS);
            if (updateResult.State != MessageResultState.SUCCESS)
            {
                result = updateResult;
                return result;
            }

            var rnd1 = new Random().Next(0, 10);
            var rnd2 = new Random().Next(0, 10);
            var rnd3 = new Random().Next(0, 10);
            var rnd4 = new Random().Next(0, 10);

            string sifre = rnd1.ToString() + "" + rnd2.ToString() + "" + rnd3.ToString() + "" + rnd4.ToString();

            var icerik = new VM_MAIL_ICERIK()
            {
                kime = kime,
                sifre = sifre,
                Host = "smtp.outlook.com",  //microsoft servislerini kullan
                gondericii_mail = "kayamansur61@gmail.com",
                gondericii_sifre = "Google.*?61",
            };

            string sonuc = emailService.EmailGonder(icerik);


            if (sonuc == "-1")
            {
                result.State = MessageResultState.ERROR;
                result.Message = sonuc;
            }
            else
            {
                result.State = MessageResultState.SUCCESS;
                result.Message = sifre;
            }

            return result;
        }



    }
}
