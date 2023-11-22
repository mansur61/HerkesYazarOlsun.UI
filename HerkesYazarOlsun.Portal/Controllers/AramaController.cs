
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;


namespace HerkesYazarOlsun.Portal.Controllers
{
    public class AramaController : Controller
    {
     
        public IActionResult Arama(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS vM_BOOKS = new VM_BOOKS();

            var getBookList = new BooksService().GetBooksList();
            vM_BOOKS.BooksList = getBookList!;
            return View(vM_BOOKS);
           
        }

    }
}
