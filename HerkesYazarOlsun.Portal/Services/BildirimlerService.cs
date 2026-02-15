
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{

    public class BildirimlerService : BaseService
    {


        public Bildirimler GetBildirimlerByLoginId(long loginId)
        {
            Task<string> jsonContent = GetData("api/Bildirimler/GetBildirimlerByLoginId?" + "loginId=" + loginId);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Bildirimler>(jsonContent.Result);
            return result;
        }

    }
}
