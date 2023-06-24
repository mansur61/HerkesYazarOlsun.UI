using HerkesYazarOlsun.Model.Entity;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class BooksPagesService : BaseService
    {
        public BooksPages? GetBooksPages(long id)
        {
            Task<string> jsonContent = GetData("api/Books/GetBooksPages?" + "id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<BooksPages>(jsonContent.Result);
            return result;
        }

        public List<BooksPages>? GetBooksPagesList()
        {
            Task<string> jsonContent = GetData("api/Books/GetBooksPagesList");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<BooksPages>>(jsonContent.Result);
            return result;
        }

        public BooksPages PostSaveBooksPages(BooksPages book)
        {
            string stringData = JsonConvert.SerializeObject(book);
            Task<string> jsonContent = PostData("api/BooksPages/PostSaveBooksPages", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<BooksPages>(jsonContent.Result);
            return result;
        }

    }
}
