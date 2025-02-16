using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    public class CarouselDuyuruService : BaseService
    {

        public List<VM_CAROUSEL_DUYURU> GetDuyurular()
        {
            Task<string> jsonContent = GetData("api/CarouselDuyuru/GetDuyurular");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<VM_CAROUSEL_DUYURU>>(jsonContent.Result);
            return result;
        }

    }
}
