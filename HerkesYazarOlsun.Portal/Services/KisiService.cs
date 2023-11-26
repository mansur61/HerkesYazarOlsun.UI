
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class KisiService : BaseService
    {
        public Users GetKisiByTC(long id)
        {
            Task<string> jsonContent = GetData("api/Users/GetKisiByTC?" + "id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Users>(jsonContent.Result);
            return result;
        }

        public FAVORI_YAZARLAR PostFavoriSaveWriter(VM_FAVORI_YAZARLAR fav_yazar)
        {
            string stringData = JsonConvert.SerializeObject(fav_yazar);
            Task<string> jsonContent = PostData("api/Users/PostFavoriSaveWriter", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<FAVORI_YAZARLAR > (jsonContent.Result);
            return result;
        }

        public List<Users> GetKisiler(VM_ARAMA_INPUT arama)
        {
            string stringData = JsonConvert.SerializeObject(arama);
            Task<string> jsonContent = PostData("api/Users/GetKisiler", stringData);

           //Task<string> jsonContent = GetData("api/Users/GetKisiler");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<Users>>(jsonContent.Result);
            return result;
        }

        //public string PostTelefonDogru(VM_TELEFON telefon)
        //{
        //    string stringData = JsonConvert.SerializeObject(telefon);
        //    Task<string> jsonContent = PostData("api/eMaden/PostTelefonDogru", stringData);
        //    Task.WaitAll(jsonContent);

        //    string result = JsonConvert.DeserializeObject<string>(jsonContent.Result);
        //    return result;
        //}
    }
}
