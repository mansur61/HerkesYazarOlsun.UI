
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class EmailService 
    {


        public string EmailGonder(VM_MAIL_ICERIK icerik) //string kime, string? sifre
        {
            MailMessage mailMessage = new MailMessage();
            // mail kimden geliyor
            string gonderici_mail = icerik.gondericii_mail; 
            mailMessage.From = new MailAddress(gonderici_mail);


            mailMessage.To.Add(icerik.kime);

            if (string.IsNullOrEmpty(icerik.sifre))
            {
                mailMessage.Subject = icerik.konu;
                mailMessage.Body =  icerik.icerik; // html formatta ta hazırlanabilir
            }
            else
            {
                mailMessage.Subject = "Herkes Yazar Olsun Mail Doğrulama";
                mailMessage.Body = "Gelen Kod : " + icerik.sifre; // html formatta ta hazırlanabilir
            }

            if (!string.IsNullOrEmpty(icerik.dosyaYolu))
            {
                string dosyaYolu = icerik.dosyaYolu;
                Attachment dosyaEki = new Attachment(dosyaYolu);
                mailMessage.Attachments.Add(dosyaEki);
            }               

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = icerik.Host; 

            smtpClient.Port = 587;
            //smtpClient.UseDefaultCredentials = true;
            smtpClient.EnableSsl = true;

           
            string username = icerik.username; 
            string password = icerik.sifre;
            smtpClient.Credentials = new NetworkCredential(username, password);
            


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
