
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Xml.Linq;


namespace HerkesYazarOlsun.Portal.Controllers
{
    public class SmsController : Controller
    {

        public IActionResult SmsDogrula()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SmsOtp(VM_OTP otp)
        {
            otp.baslik = "Telefon Doğrulaması";
            otp.alt_baslik = "Telefona gelen kodu giriniz";
            otp.telno = otp.telno;
            otp.tip = "sms";
            return PartialView(otp);
        }


        public ServiceResult Gonder(string telno)
        {
            ServiceResult result = new ServiceResult();

            string sifre = "";
            int rnd = 0;

            Random rnd_kh = new Random();
            rnd = rnd_kh.Next(0, 4);
            sifre = rnd.ToString();

            string testXml = "<request>";
            testXml += "<authentication>";
            testXml += "<username></username>";
            testXml += "<password></password>";
            testXml += "</authentication>";
            testXml += "<order>";
            testXml += "<sender></sender>";
            testXml += "<sendDateTime></sendDateTime>";
            testXml += "<message>";
            testXml += $"<text>Gelen kod : {sifre}</text>";
            testXml += "<receipents>";
            testXml += $"<number>{telno}</number>";
            testXml += "</receipents>";
            testXml += "</message>";
            testXml += "</order>";
            testXml += "</request>";

           

            string sonuc = SmsGonder("http://api.iletimerkezi.com/v1/send-sms", testXml);
           
            
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


        public string SmsGonder(string PostAddress, string xmlData)
        {
            try
            {
                var res = "";
                byte[] bytes = Encoding.UTF8.GetBytes(xmlData);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PostAddress);

                request.Method = "POST";
                request.ContentLength = bytes.Length;
                request.ContentType = "text/xml";
                request.Timeout = 300000000;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }

                // This sample only checks whether we get an "OK" HTTP status code back.
                // If you must process the XML-based response, you need to read that from
                // the response stream.
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        string message = String.Format(
                        "POST failed. Received HTTP {0}",
                        response.StatusCode);
                        throw new ApplicationException(message);
                    }

                    Stream responseStream = response.GetResponseStream();
                    using (StreamReader rdr = new StreamReader(responseStream))
                    {
                        res = rdr.ReadToEnd();
                    }
                    return res;
                }
            }
            catch
            {

                return "-1";
            }
        }
    }
}
