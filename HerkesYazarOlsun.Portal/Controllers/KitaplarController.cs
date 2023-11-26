using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using HerkesYazarOlsun.Model.Utils;
using System.Linq;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class KitaplarController : Controller
    {
        // private readonly BooksService booksService;

        //Kitap ekleme ana sayfası açılır
        public IActionResult Index(VM_BOOKS input)
        {

            return View(input);
        }

        public IActionResult KitapOku(long kitapId)
        {
            var getBook = new BooksService().GetBooks(kitapId);
            var vmBook = ObjectMapper.Map(getBook, new VM_BOOKS());
            var getBookPage = new BooksPagesService().GetPagesByBooks(kitapId);
            getBookPage = getBookPage ?? null;
            vmBook.BooksPageList = ObjectMapper.MapList(getBookPage!, new List<VM_BOOKS_PAGES>());
            return View(vmBook);
        }
        [HttpPost]
        [Route("KitabiFavorilereEkle")]
        public JsonResult KitabiFavorilereEkle(long id)
        {
            FAVORILER fAVORILER = new FAVORILER()
            {
                BOOKS_ID = id,
                USER_ID = 1
            };
            var getFavori = new BooksService().PostFavoriSaveBook(fAVORILER);

            
            return Json(getFavori);
        }


        [HttpPost]
        [Route("KitapEkle")]
        public JsonResult KitapEkle(VM_BOOKS input)
        {
            int sayfaId = 0;
            int sayfaCount = 0;
            VM_KITAP_EKLE vmKitap = new VM_KITAP_EKLE();
            //var getBook = new BooksService().GetBooks(input.ID);
            if (input.ID != 0)
            {

                var _book = ObjectMapper.Map(input, new Books());
                var updaterBook = new BooksService().UpdateBook(_book);

                var bookPages = new BooksPagesService().PostSaveBooksPages(new BooksPages()
                {
                    BooksId = input.ID,
                    PageWrite = input.SAYFAYAZI,
                    PageFoto = input.KITAPSAYFAFOTO != null ? input.KITAPSAYFAFOTO : ""

                });
                if (bookPages != null)
                {
                    sayfaCount = new BooksPagesService().GetPagesByBooks(input.ID)!.Count();
                    sayfaId = (int)bookPages.ID;

                }

                vmKitap.BookID = input.ID;
                vmKitap.BooksPageCount = sayfaCount + 1;
                vmKitap.BooksPageID = sayfaId;


            }
            else
            {
                var book = new BooksService().PostSaveBook(new Books()
                {
                    ARKAKAPAKFOTO = input.ARKAKAPAKFOTO != null ? input.ARKAKAPAKFOTO : "",
                    ONSOZ = input.ONSOZ,
                    ONKAPAKFOTO = input.ONKAPAKFOTO != null ? input.ONKAPAKFOTO : "",
                    CategoriId = input.CategoriId,
                    Name = input.Name != null ? input.Name : "",
                    ARKAKAPAKYAZISI = input.ARKAKAPAKYAZISI != null ? input.ARKAKAPAKYAZISI : ""

                });
                if (book?.ID != 0 && book != null)
                {
                    var bookPages = new BooksPagesService().PostSaveBooksPages(new BooksPages()
                    {
                        BooksId = book.ID,
                        PageWrite = input.SAYFAYAZI,
                        PageFoto = input.KITAPSAYFAFOTO != null ? input.KITAPSAYFAFOTO : ""

                    });
                    if (bookPages != null && bookPages.ID != 0)
                    {
                        input.ID = bookPages.ID;
                        sayfaId = (int)bookPages.ID;
                        sayfaCount = new BooksPagesService().GetPagesByBooks(book.ID)!.Where(p => p.BooksId == book.ID).Count();
                    }

                    vmKitap.BookID = book!.ID;
                    vmKitap.BooksPageID = sayfaId;
                    vmKitap.BooksPageCount = sayfaCount + 1;
                }


            }
            return Json(vmKitap);
        }
    

        public IActionResult KitapDetay(long kitapId)
        {
            var getBookDetay = new BooksService().GetBooks(kitapId);
            return View(getBookDetay);
        }

        public IActionResult TumKitaplar(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS vM_BOOKS = new VM_BOOKS();
            vM_BOOKS.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;
            var getBookList = new BooksService().TumKitaplar(arama);
            vM_BOOKS.BooksList = getBookList!; vM_BOOKS.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;
            return View(vM_BOOKS);
        }
        public IActionResult AraButonFiltrelemeKitaplar(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS vM_BOOKS = new VM_BOOKS();
            List<Books>? bookList;
            vM_BOOKS.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;
           
            bookList = new BooksService().TumKitaplar(arama);
            vM_BOOKS.BooksList = bookList!;
            
            return View(vM_BOOKS);
        }

    }
}
