using HerkesYazarOlsun.Model.Entity;
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

        public List<Books>? GetBooksList()
        {
            Task<string> jsonContent = GetData("api/Books/GetBooksList");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<Books>>(jsonContent.Result);
            return result;
        }
        public FAVORILER PostFavoriSaveBook(FAVORILER fav)
        {
            string stringData = JsonConvert.SerializeObject(fav);
            Task<string> jsonContent = PostData("api/Books/PostFavoriSaveBook", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<FAVORILER>(jsonContent.Result);
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
