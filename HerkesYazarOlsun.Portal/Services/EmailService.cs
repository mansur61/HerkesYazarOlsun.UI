using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using System.Net;
using System.Net.Mail;

namespace HerkesYazarOlsun.Portal.Services
{
    public class EmailService
    {
        public async Task<string> EmailGonder(VM_MAIL_ICERIK icerik)
        {
            using (var mailMessage = new MailMessage())
            {
                mailMessage.IsBodyHtml = true;

                // Gönderen
                string gonderici_mail = icerik.gondericii_mail;
                mailMessage.From = new MailAddress(gonderici_mail);

                // Alıcı
                mailMessage.To.Add(icerik.kime);

                // Konu ve içerik
                if (!string.IsNullOrEmpty(icerik.icerik))
                {
                    mailMessage.Subject = icerik.konu;
                    mailMessage.Body = icerik.icerik;
                }
                else
                {
                    mailMessage.Subject = "Herkes Yazar Olsun Mail Doğrulama";
                    mailMessage.Body = "Gelen Kod : " + icerik.sifre;
                }

                // Dosya ekleri (birden fazla olabilir)
                if (icerik.dosyalar != null && icerik.dosyalar.Any())
                {
                    foreach (var dosya in icerik.dosyalar)
                    {
                        if (dosya != null && dosya.Length > 0)
                        {
                            var memoryStream = new MemoryStream();
                            await dosya.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;

                            var attachment = new Attachment(memoryStream, dosya.FileName, dosya.ContentType);
                            mailMessage.Attachments.Add(attachment);
                        }
                    }
                }

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Host = icerik.Host;
                    smtpClient.Port = icerik.Port ?? 587;
                    smtpClient.EnableSsl = icerik.EnableSSL;

                    smtpClient.Credentials = new NetworkCredential(icerik.username, icerik.password);

                    try
                    {
                        await smtpClient.SendMailAsync(mailMessage);
                        return "1";
                    }
                    catch (Exception)
                    {
                        return "-1";
                    }
                }
            }
        }
    }
}
