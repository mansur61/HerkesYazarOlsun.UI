using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using HerkesYazarOlsun.Model.Utils;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class KitaplarController : Controller
    {
        // private readonly BooksService booksService;


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
        [Route("KitapEkle")]
        public JsonResult KitapEkle(VM_BOOKS input)
        {
            int kitapId = 0;

            //var getBook = new BooksService().GetBooks(input.ID);
            if (input.ID != 0)
            {
                var getBook = new BooksService().GetBooks(input.ID);
                var _booksPage = new BooksPagesService().GetPagesByBooks(getBook != null ? getBook.ID : 0);
                if (_booksPage != null && _booksPage.Count() != 0)
                {
                    var enSonKitapKaydi = _booksPage.OrderByDescending(p => p.ID).SingleOrDefault();
                    if (enSonKitapKaydi != null)
                    {
                        kitapId = (int)enSonKitapKaydi.ID;
                    }
                }
                else
                {
                    var bookPages = new BooksPagesService().PostSaveBooksPages(new BooksPages()
                    {
                        BooksId = getBook.ID,
                        PageWrite = input.SAYFAYAZI,
                        PageFoto = input.KITAPSAYFAFOTO != null ? input.KITAPSAYFAFOTO : ""

                    });
                    if (bookPages != null)
                    {
                        kitapId = (int)bookPages.ID;
                    }
                }


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
                        kitapId = (int)bookPages.ID;
                    }

                }

            }
            return Json(kitapId + 1);
        }

        public IActionResult KitapArama()
        {
            return View();
        }

        public IActionResult KitapDetay(long kitapId)
        {
            var getBookDetay = new BooksService().GetBooks(kitapId);
            return View(getBookDetay);
        }


    }
}
