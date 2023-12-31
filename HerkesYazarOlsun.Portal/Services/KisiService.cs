
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class KisiService : BaseService
    {
        public Users GetKisiByTC(long tck)
        {
            Task<string> jsonContent = GetData("api/Users/GetKisiByTC?" + "tck=" + tck);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Users>(jsonContent.Result);
            return result;
        }

        public VM_WriterFollow GetWriterFollowById(long yazar_id)
        {
            Task<string> jsonContent = GetData("api/Users/GetWriterFollowById?" + "yazar_id=" + yazar_id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_WriterFollow>(jsonContent.Result);
            return result;
        }

        
        public Users GetKisiByUsername(string username )
        {
            Task<string> jsonContent = GetData("api/Users/GetKisiByUsername?" + "username=" + username);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Users>(jsonContent.Result);
            return result;
        }

        public Users GetKisiById(long id)
        {
            Task<string> jsonContent = GetData("api/Users/GetKisiById?" + "id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Users>(jsonContent.Result);
            return result;
        }

        public VM_Stars GetMaxStarWriterById(long id)
        {
            Task<string> jsonContent = GetData("api/Users/GetMaxStarWriterById?" + "id=" + id); ;
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_Stars>(jsonContent.Result);
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

        public ServiceResult<WriterStars> PostWriterStars(WriterStars star)
        {
            string stringData = JsonConvert.SerializeObject(star);
            Task<string> jsonContent = PostData("api/Users/PostWriterStars", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<WriterStars>>(jsonContent.Result);
            return result;
        }

        public ServiceResult<WriterFollow> PostWriterFollow(WriterFollow follow)
        {
            string stringData = JsonConvert.SerializeObject(follow);
            Task<string> jsonContent = PostData("api/Users/PostWriterFollow", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<WriterFollow>>(jsonContent.Result);
            return result;
        }

        public ServiceResult PostKisiUpdate(VM_USERS vm_kisi)
        {
            string stringData = JsonConvert.SerializeObject(vm_kisi);
            Task<string> jsonContent = PostData("api/Users/PostKisiUpdate", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }

        public ServiceResult PostKisiSave(VM_USERS vm_kisi)
        {
            string stringData = JsonConvert.SerializeObject(vm_kisi);
            Task<string> jsonContent = PostData("api/Users/PostKisiSave", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }


        public List<VM_USERS> GetKisiler(VM_ARAMA_INPUT arama)
        {
            string stringData = JsonConvert.SerializeObject(arama);
            Task<string> jsonContent = PostData("api/Users/GetKisiler", stringData);

           //Task<string> jsonContent = GetData("api/Users/GetKisiler");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<VM_USERS>>(jsonContent.Result);
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
