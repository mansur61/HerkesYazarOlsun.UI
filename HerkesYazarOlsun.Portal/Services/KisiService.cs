using HerkesYazarOlsun.Model.Entity;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class KisiService : BaseService
    {
        public KISILER GetKisiByTC(long id)
        {
            Task<string> jsonContent = GetData("api/Kisi/GetKisiByTC?" + "id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<KISILER>(jsonContent.Result);
            return result;
        }
        //Örnek post kullanımı
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
