
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
            if(updateResult.State != MessageResultState.SUCCESS)
            {
                result = updateResult;
                return result;
            }

            var rnd1 = new Random().Next(0, 10);
            var rnd2 = new Random().Next(0, 10);
            var rnd3 = new Random().Next(0, 10);
            var rnd4 = new Random().Next(0, 10);

            string sifre = rnd1.ToString() + ""+ rnd2.ToString()+ "" + rnd3.ToString() +""+ rnd4.ToString();

            string sonuc =  EmailGonder(kime,sifre);
            

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


        public string EmailGonder(string kime, string sifre)
        {
            MailMessage mailMessage = new MailMessage();
            string gonderici_mail = "kayamansur61@gmail.com";
            mailMessage.From = new MailAddress(gonderici_mail);

           
            mailMessage.To.Add(kime);
            mailMessage.Subject = "Herkes Yazar Olsun Mail Doğrulama";
            mailMessage.Body = "Gelen Kod : "+ sifre; // html formatta ta hazırlanabilir

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = "smtp.outlook.com"; //kullanılan servis microsoft 
            //"smtp.gmail.com"; // 3. parti uygulamalara izin verilmiyor

            smtpClient.Port = 587;
            //smtpClient.UseDefaultCredentials = true;
            smtpClient.EnableSsl = true;

            ////gmail için
            //string gonderici_username = "kayamansur61@gmail.com";
            //string gonderici_password = "Google.*?6161";

            //microsoft için
            string gonderici_username = "kayamansur61@gmail.com";
            string gonderici_password = "Google.*?61";
            smtpClient.Credentials = new NetworkCredential(gonderici_username, gonderici_password);
           

            try
            {
                smtpClient.Send(mailMessage);
                return "1";
            }
            catch (Exception ex)
            {
                return "-1";
            }
        }
    }
}
