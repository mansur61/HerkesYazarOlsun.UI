using HerkesYazarOlsun.Portal.Services;
using System.Net;
using System.Security.Claims;

namespace HerkesYazarOlsun.Portal.Helpers.Extensions
{
    public static class User
    {
        //Ip Adresini Almak İçin
        public static string IP()
        {
            string host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostByName(host);
            return ip.AddressList[0].ToString();
            //birden fazla ip olabilceğinden burada ilk bulunanı alıyoruz.
        }
        public static string GetAdSoyad(this ClaimsPrincipal user)
        {
            var adi = user.Claims.Where(x => x.Type == "adi").FirstOrDefault();
            var soyadi = user.Claims.Where(x => x.Type == "soyadi").FirstOrDefault();
            return adi != null ? adi.Value + " " + soyadi.Value : "";
        }

        public static long GetTcKimlikNo(this ClaimsPrincipal user)
        {
            var tckimlikno = user.Claims.Where(x => x.Type == "tckimlikno").FirstOrDefault();
            return tckimlikno != null ? Convert.ToInt64(tckimlikno.Value) : 0;
        }

        public static string GetGozlemciMod(this ClaimsPrincipal user)
        {
            var mod = user.Claims.Where(x => x.Type == "gozlemci_mod").FirstOrDefault();
            return mod != null ? mod.Value : "0";
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            var email = user.Claims.Where(x => x.Type == "email").FirstOrDefault();
            return email != null ? email.Value : "";
        }
        public static string GetUserName(this ClaimsPrincipal user)
        {
            var usrName = user.Claims.Where(x => x.Type == "username").FirstOrDefault();
            return usrName != null ? usrName.Value : "";
        }

        public static long GetLoginUserId(this ClaimsPrincipal user)
        {
            var email = user.Claims.Where(x => x.Type == "email").FirstOrDefault();
            KisiService kisiService = new KisiService();
            var sonuc = kisiService.GetKisiByMail(email?.Value ?? "");
            return sonuc != null ? sonuc.Result != null ? sonuc.Result.ID  : 0 : 0;
        }

        public static string GetIpAddress(this ClaimsPrincipal user)
        {
            var ip = user.Claims.Where(x => x.Type == "ip").FirstOrDefault();
            return ip != null ? ip.Value : string.Empty;
        }

        public static string GetUserInfo(this ClaimsPrincipal user, string key)
        {
            var data = user.Claims.Where(x => x.Type == key).FirstOrDefault();
            return data != null ? data.Value : "";
        }

        public static long GetUserInfoByKey(this ClaimsPrincipal user, string key)
        {
            var data = user.Claims.Where(x => x.Type == key).FirstOrDefault();
            return data != null && !string.IsNullOrEmpty(data.Value) ? Convert.ToInt64(data.Value) : 0;
        }

        public static bool CheckAuthoryForCbs(this ClaimsPrincipal user, string authorityKey)
        {
            return user.IsInRole(authorityKey);
        }
    }
}
