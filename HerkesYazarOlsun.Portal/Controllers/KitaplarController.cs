using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerkesYazarOlsun.Portal.Controllers
{
    public class KitaplarController : Controller
    {
        // private readonly BooksService booksService;


        public IActionResult Index(VM_BOOKS input)
        {

            return View(input);
        }

        public IActionResult KitapOku()
        {
            return View();
        }


        [HttpPost]
        [Route("KitapEkle")]
        public JsonResult KitapEkle(VM_BOOKS input)
        {
            int kitapId = 0;
            var getBook = new BooksService().GetBooks(input.ID);
            if (getBook != null)
            {
                var bookPages = new BooksPagesService().PostSaveBooksPages(new BooksPages()
                {
                    BooksId = getBook.ID,
                    PageWrite = input.SAYFAYAZI,
                    PageFoto = input.KITAPSAYFAFOTO

                });
            }
            else
            {
                var book = new BooksService().PostSaveBook(new Books()
                {
                    ARKAKAPAKFOTO = input.ARKAKAPAKFOTO,
                    ONSOZ = input.ONSOZ,
                    ONKAPAKFOTO = input.ONKAPAKFOTO,
                    CategoriId = input.CategoriId,

                });

                var bookPages = new BooksPagesService().PostSaveBooksPages(new BooksPages()
                {
                    BooksId = book.ID,
                    PageWrite = input.SAYFAYAZI,
                    PageFoto = input.KITAPSAYFAFOTO

                });

                input.ID = bookPages.ID;
                kitapId = (int)bookPages.ID;
            }
            return Json(kitapId);
        }

        public IActionResult KitapArama()
        {
            return View();
        }

        public IActionResult KitapDetay()
        {
            return View();
        }

        public IActionResult KitapDetaySayfasi()
        {
            return View();
        }
    }
}
