using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;


namespace HerkesYazarOlsun.Portal.Services
{
    
    public class BooksPagesService : BaseService
    {
        public BooksPages? GetBooksPages(long id)
        {
            Task<string> jsonContent = GetData("api/BooksPages/GetBooksPages?" + "id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<BooksPages>(jsonContent.Result);
            return result;
        }

        public ServiceResult PostUpdateBooksPages(VM_BOOKS_PAGES sayfa)
        {
            string stringData = JsonConvert.SerializeObject(sayfa);
            Task<string> jsonContent = PostData("api/BooksPages/PostUpdateBooksPages", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }

        public List<BooksPages>? GetBooksPagesList()
        {
            Task<string> jsonContent = GetData("api/BooksPages/GetBooksPagesList");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<BooksPages>>(jsonContent.Result);
            return result;
        }
        public List<BooksPages>? GetPagesByBooks(long bookID)
        {
            Task<string> jsonContent = GetData("api/BooksPages/GetPagesByBooks?" + "bookID=" + bookID);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<BooksPages>?>(jsonContent.Result);
            return result;
        }

       
        public ServiceResult<BooksPages> PostSaveBooksPages(VM_BOOKS_PAGES bookPages)
        {
            string stringData = JsonConvert.SerializeObject(bookPages);
            Task<string> jsonContent = PostData("api/BooksPages/PostSaveBooksPages", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<BooksPages>>(jsonContent.Result);
            return result;
        }

    }
}
