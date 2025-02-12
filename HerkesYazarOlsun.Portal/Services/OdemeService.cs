
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class OdemeService : BaseService
    {
        

        public ServiceResult PostOdeme(VM_KARTLAR kart)
        {
            string stringData = JsonConvert.SerializeObject(kart);
            Task<string> jsonContent = PostData("api/Odeme/PostOdeme", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }

        public ServiceResult SaveSponsorlukBildir(VM_ODEME_SPONSORLARI odemeSponsorlar)
        {
            string stringData = JsonConvert.SerializeObject(odemeSponsorlar);
            Task<string> jsonContent = PostData("api/Odeme/SaveSponsorlukBildir", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }

    }
}
