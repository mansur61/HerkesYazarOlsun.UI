using DocumentFormat.OpenXml.Wordprocessing;
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    public class TaleplerService : BaseService
    {

        public ServiceResult TalepKaydet(TALEPLER talepler)
        {
            string stringData = JsonConvert.SerializeObject(talepler);
            Task<string> jsonContent = PostData("api/Talepler/TalepKaydet", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }
         

    }
}
