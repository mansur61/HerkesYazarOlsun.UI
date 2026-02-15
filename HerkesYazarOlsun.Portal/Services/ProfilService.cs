using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{

    public class ProfilService : BaseService
    {


        public VM_PROFILE GetProfilByLoginId(long loginId)
        {
            Task<string> jsonContent = GetData("api/Profil/GetProfilByLoginId?" + "loginId=" + loginId);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_PROFILE>(jsonContent.Result);
            return result;
        }

    }
}
