
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class SponsorlarService : BaseService
    {


        public ServiceResult PostSponsorlar(VM_SPONSORLAR spns)
        {
            string stringData = JsonConvert.SerializeObject(spns);
            Task<string> jsonContent = PostData("api/Sponsorlar/PostSponsorlar", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }


        public List<VM_SPONSORLAR> GetSponsorlar()
        {
            Task<string> jsonContent = GetData("api/Sponsorlar/GetSponsorlar");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<VM_SPONSORLAR>>(jsonContent.Result);
            return result;
        }

        public VM_SPONSORLAR GetSponsorlarById(long id)
        {
            Task<string> jsonContent = GetData("api/Sponsorlar/GetSponsorlarById?id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_SPONSORLAR>(jsonContent.Result);
            return result;
        }

    }
}
