using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{
    
    public class BooksService : BaseService
    {
        public VM_BOOKS? GetBooks(long id)
        {
            Task<string> jsonContent = GetData("api/Books/GetBooks?" + "id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_BOOKS>(jsonContent.Result);
            return result;
        }
         
        public VM_BOOK_ISTATISTIKLER GetISTATISTIKLERBooksById(long id)
        {
            Task<string> jsonContent = GetData("api/Books/GetISTATISTIKLERBooksById?" + "id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_BOOK_ISTATISTIKLER>(jsonContent.Result);
            return result;
        }

        public List<VM_BOOKS_DEGERLENDIRME> GetDegerlendirmelerBooksById(long kitapId)
        {
            Task<string> jsonContent = GetData("api/Books/GetDegerlendirmelerBooksById?" + "kitapId=" + kitapId);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<VM_BOOKS_DEGERLENDIRME>>(jsonContent.Result);
            return result;
        }

        public List<VM_BOOKS_COMMENT> GetCommenstBooksById(long kitapId)
        {
            Task<string> jsonContent = GetData("api/Books/GetCommenstBooksById?" + "kitapId=" + kitapId);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<VM_BOOKS_COMMENT>>(jsonContent.Result);
            return result;
        }
        public VM_BOOKS_DETAIL GetBooksList()
        {
            Task<string> jsonContent = GetData("api/Books/GetBooksList");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_BOOKS_DETAIL>(jsonContent.Result);
            return result;
        }

        public List<VM_BOOKS>? GetBooksList2()
        {
            Task<string> jsonContent = GetData("api/Books/GetBooksList");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<VM_BOOKS>>(jsonContent.Result);
            return result;
        }

        public VM_CATEGORI? GetCategory()
        {
            Task<string> jsonContent = GetData("api/Books/GetCategory");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<VM_CATEGORI>(jsonContent.Result);
            return result;
        }

        public List<VM_CATEGORI>? GetCategories()
        {
            Task<string> jsonContent = GetData("api/Books/GetCategories");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<VM_CATEGORI>>(jsonContent.Result);
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

        public ServiceResult PostBooksDegerlendirme(VM_BOOKS_DEGERLENDIRME degerlendirme)
        {
            string stringData = JsonConvert.SerializeObject(degerlendirme);
            Task<string> jsonContent = PostData("api/Books/PostBooksDegerlendirme", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }

        public ServiceResult PostBooksComments(VM_BOOKS_COMMENT mesajlar)
        {
            string stringData = JsonConvert.SerializeObject(mesajlar);
            Task<string> jsonContent = PostData("api/Books/PostBooksComments", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
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

        public async Task<ServiceResult<Books>> PostSaveBook(VM_BOOKS_DETAIL VMbook)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.ApiPath)
            };

            using var form = new MultipartFormDataContent();

            // VM_BOOKS içindeki normal alanları ekle
            foreach (var prop in typeof(VM_BOOKS).GetProperties())
            {
                try
                {
                    if (prop.Name == "dosyalar" || prop.Name == "BookModel")
                        continue; // dosyaları ayrıca ekleyeceğiz

                    var value = prop.GetValue(VMbook.BookModel);
                    if (value != null)
                    {
                        //form.Add(new StringContent(value.ToString()!), prop.Name);
                        form.Add(new StringContent(value.ToString()!), $"BookModel.{prop.Name}");
                    } 
                }
                catch(Exception e)
                {
                    var _ = e.Message;
                }
            }

            // VM_BOOKS_PAGES içindeki normal alanları ekle
            //if (VMbook.BookPagesModel != null)
            //{
            //    foreach (var prop in typeof(VM_BOOKS_PAGES).GetProperties())
            //    {
            //        // Dosya listelerini burada atla
            //        if (prop.PropertyType == typeof(List<IFormFile>) ||
            //            prop.PropertyType == typeof(List<string>))
            //            continue;

            //        var value = prop.GetValue(VMbook.BookPagesModel);
            //        if (value != null)
            //        {
            //            form.Add(
            //                new StringContent(value.ToString()!),
            //                $"BookPagesModel.{prop.Name}"
            //            );
            //        }
            //    }
            //}


            // Dosyaları ekle
            if (VMbook.dosyalar != null)
            {
                foreach (var file in VMbook.dosyalar)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    form.Add(streamContent, "dosyalar", file.FileName);
                }
            }

            //if (VMbook.BookPagesModel?.PageFotoDosyalar != null)
            //{
            //    foreach (var file in VMbook.BookPagesModel.PageFotoDosyalar)
            //    {
            //        var streamContent = new StreamContent(file.OpenReadStream());
            //        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            //        form.Add(streamContent, "BookPagesModel.PageFotoDosyalar", file.FileName);

            //    }
            //}

            // API'ye gönder
            var response = await client.PostAsync("api/Books/PostSaveBook", form);
            var resultJson = await response.Content.ReadAsStringAsync();

            // Doğrudan ServiceResult<Books> deserialize et
            var sonuc = JsonConvert.DeserializeObject<ServiceResult<Books>>(resultJson);

            return sonuc!;
        }

         
        public ServiceResult<Books> PostSaveBook2(VM_BOOKS VMbook)
        {
            string stringData = JsonConvert.SerializeObject(VMbook);
            Task<string> jsonContent = PostData("api/Books/PostSaveBook", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<Books>>(jsonContent.Result);
            return result;
        }

        public void DeleteBook(Books book)
        {
            string stringData = JsonConvert.SerializeObject(book);
            Task<string> jsonContent = PostData("api/Books/DeleteBook", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<Books>(jsonContent.Result);
            //return result;
        }

        public ServiceResult<bool> DeleteBookById(long kId)
        {
            Task<string> jsonContent = GetData("api/Books/DeleteBookById?" + "kId=" + kId);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<bool>>(jsonContent.Result);
            return result;
        }

        public ServiceResult<Books> UpdateBook(Books book)
        {
            string stringData = JsonConvert.SerializeObject(book);
            Task<string> jsonContent = PostData("api/Books/UpdateBook", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<Books>>(jsonContent.Result);
            return result;
        }

        public ServiceResult<Books> CheckBook(long id)
        {
            //string stringData = JsonConvert.SerializeObject(id);
            Task<string> jsonContent = GetData("api/Books/CheckBook?" + "id=" + id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult<Books>>(jsonContent.Result);
            return result;
        }

    }
}
