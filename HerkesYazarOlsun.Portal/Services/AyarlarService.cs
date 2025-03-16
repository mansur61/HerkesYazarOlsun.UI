
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class AyarlarService : BaseService
    {
        

        public ServiceResult SaveOrUpdateAyarlar(VM_AYARLAR ayarlar)
        {
            string stringData = JsonConvert.SerializeObject(ayarlar);
            Task<string> jsonContent = PostData("api/Settings/SaveOrUpdateAyarlar", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }

        public YayinAyarlari? GetYyainAyarlari()
        { 
            Task<string> jsonContent = GetData("api/Settings/GetYyainAyarlari");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<YayinAyarlari>(jsonContent.Result);
            return result;
        }

    }
}
