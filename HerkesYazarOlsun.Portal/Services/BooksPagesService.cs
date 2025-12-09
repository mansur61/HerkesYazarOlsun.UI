using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
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

        public ServiceResult PostUpdateBooksPages2(VM_BOOKS_PAGES sayfa)
        {
            string stringData = JsonConvert.SerializeObject(sayfa);
            Task<string> jsonContent = PostData("api/BooksPages/PostUpdateBooksPages", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }
        public async Task<ServiceResult> PostUpdateBooksPages(VM_BOOKS_PAGES bookPages)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.ApiPath)
            };

            using var form = new MultipartFormDataContent();

            // VM_BOOKS içindeki normal alanları ekle
            foreach (var prop in typeof(VM_BOOKS_PAGES).GetProperties())
            {
                try
                {
                    if (prop.Name == "PageFotoDosyalar")
                        continue; // dosyaları ayrıca ekleyeceğiz

                    var value = prop.GetValue(bookPages);
                    if (value != null)
                    {
                        form.Add(new StringContent(value.ToString()!), $"{prop.Name}");
                    }
                }
                catch (Exception e)
                {
                    var _ = e.Message;
                }
            }


            // Dosyaları ekle
            if (bookPages.PageFotoDosyalar != null)
            {
                foreach (var file in bookPages.PageFotoDosyalar)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    form.Add(streamContent, "PageFotoDosyalar", file.FileName);

                }
            }

            // API'ye gönder 
            var response = await client.PostAsync("api/BooksPages/PostUpdateBooksPages", form);
            var resultJson = await response.Content.ReadAsStringAsync();

            // Doğrudan ServiceResult<Books> deserialize et
            var sonuc = JsonConvert.DeserializeObject<ServiceResult>(resultJson);

            return sonuc!;
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

       
        public ServiceResult<BooksPages> PostSaveBooksPages2(VM_BOOKS_PAGES bookPages)
        {
            string stringData = JsonConvert.SerializeObject(bookPages);
            Task<string> jsonContent = PostData("api/BooksPages/PostSaveBooksPages", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<BooksPages>>(jsonContent.Result);
            return result;
        }

        public async Task<ServiceResult<BooksPages>> PostSaveBooksPages(VM_BOOKS_PAGES bookPages)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.ApiPath)
            };

            using var form = new MultipartFormDataContent();

            // VM_BOOKS içindeki normal alanları ekle
            foreach (var prop in typeof(VM_BOOKS_PAGES).GetProperties())
            {
                try
                {
                    if (prop.Name == "PageFotoDosyalar" )
                        continue; // dosyaları ayrıca ekleyeceğiz

                    var value = prop.GetValue(bookPages);
                    if (value != null)
                    { 
                        form.Add(new StringContent(value.ToString()!), $"{prop.Name}");
                    }
                }
                catch (Exception e)
                {
                    var _ = e.Message;
                }
            }


            // Dosyaları ekle
            if (bookPages.PageFotoDosyalar != null)
            {
                foreach (var file in bookPages.PageFotoDosyalar)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType); 
                    form.Add(streamContent, "PageFotoDosyalar", file.FileName);
                    
                }
            }

            // API'ye gönder 
            var response = await client.PostAsync("api/BooksPages/PostSaveBooksPages", form);
            var resultJson = await response.Content.ReadAsStringAsync();

            // Doğrudan ServiceResult<Books> deserialize et
            var sonuc = JsonConvert.DeserializeObject<ServiceResult<BooksPages>>(resultJson);

            return sonuc!;
        }
    }
}
