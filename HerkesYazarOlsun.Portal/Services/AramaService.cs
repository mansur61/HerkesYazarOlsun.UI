
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{

    public class AramaService : BaseService
    {


        public VM_ARAMA_SONUC TumAramalar(VM_ARAMA_INPUT arama)
        {
            string stringData = JsonConvert.SerializeObject(arama);
            Task<string> jsonContent = PostData("api/Arama/TumAramalar", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_ARAMA_SONUC>(jsonContent.Result);
            return result;
        }

    }
}
