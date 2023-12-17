using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class BooksService : BaseService
    {
        public Books? GetBooks(long id)
        {
            Task<string> jsonContent = GetData("api/Books/GetBooks?" + "id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Books>(jsonContent.Result);
            return result;
        }

        public List<VM_BOOKS>? GetBooksList()
        {
            Task<string> jsonContent = GetData("api/Books/GetBooksList");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<VM_BOOKS>>(jsonContent.Result);
            return result;
        }

        public VM_Stars GetMaxStarBooks()
        {
            Task<string> jsonContent = GetData("api/Books/GetMaxStarBooks");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_Stars>(jsonContent.Result);
            return result;
        }

        public VM_Stars GetMaxStarBooksById(long id)
        {
            Task<string> jsonContent = GetData("api/Books/GetMaxStarBooksById?id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_Stars>(jsonContent.Result);
            return result;
        }


        public ServiceResult<FavoriBooks> PostFavoriBookSave(FavoriBooks fav)
        {
            string stringData = JsonConvert.SerializeObject(fav);
            Task<string> jsonContent = PostData("api/Books/PostFavoriBookSave", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<FavoriBooks>>(jsonContent.Result);
            return result;
        }

        public ServiceResult<BooksStars> PostBooksStars(BooksStars star)
        {
            string stringData = JsonConvert.SerializeObject(star);
            Task<string> jsonContent = PostData("api/Books/PostBooksStars", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<BooksStars>>(jsonContent.Result);
            return result;
        }


        public List<VM_BOOKS>? TumKitaplar(VM_ARAMA_INPUT arama)
        {
            string stringData = JsonConvert.SerializeObject(arama);
            Task<string> jsonContent = PostData("api/Books/TumKitaplar", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<VM_BOOKS>?>(jsonContent.Result);
            return result;
        }

        public Books? PostSaveBook(Books book)
        {
            string stringData = JsonConvert.SerializeObject(book);
            Task<string> jsonContent = PostData("api/Books/PostSaveBook", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Books>(jsonContent.Result);
            return result;
        }

        public Books? UpdateBook(Books book)
        {
            string stringData = JsonConvert.SerializeObject(book);
            Task<string> jsonContent = PostData("api/Books/UpdateBook", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Books>(jsonContent.Result);
            return result;
        }

    }
}
