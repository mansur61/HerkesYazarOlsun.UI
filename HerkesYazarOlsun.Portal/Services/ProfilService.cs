
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class ProfilService : BaseService
    {


        public Profil GetProfilByLoginId(long loginId)
        {
            Task<string> jsonContent = GetData("api/Profil/GetProfilByLoginId?" + "loginId=" + loginId);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Profil>(jsonContent.Result);
            return result;
        }

    }
}
