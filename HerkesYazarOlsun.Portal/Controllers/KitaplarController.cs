using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace HerkesYazarOlsun.Portal.Controllers
{

    public class KitaplarController : Controller
    {
        private IHttpContextAccessor _contextAccessor;
        private long Lid;
        private string DosyaYolu = "C:/Users/umpg0020097/Desktop/HerkesYazarOlsun.UI/HerkesYazarOlsun.Portal/Helpers/";
        public KitaplarController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            Lid = (long)(_contextAccessor?.HttpContext?.User.GetLoginUserId());
        }
        public IActionResult KitapOlustur(VM_BOOKS_DETAIL input)
        {
            long bookID = Convert.ToInt64(StringCipher.Decrypt(input.kitap_id));
            ViewBag.BOOK_ID = bookID;
            var kategoriler = new BooksService().GetCategories();
            input.Katergoriler = kategoriler!;
            if (bookID != -1 && bookID != 0)
            {
                int sayfaCount = new BooksPagesService().GetPagesByBooks(bookID)!.Count();
                input.IlgiiSayfaSayisi = sayfaCount;

            }
            else
            {
                input.IlgiiSayfaSayisi = 0;
            }
            
            return View(input);
        }

        public IActionResult KitapOku(string kitap_id)
        {
            ViewBag.KITAPENC = kitap_id;
            var kitapId = Convert.ToInt64(StringCipher.Decrypt(kitap_id));
            var getBook = new BooksService().GetBooks(kitapId);
            var vmBook = ObjectMapper.Map(getBook, new VM_BOOKS());
            ViewBag.VM_KITAP = vmBook;
            var getBookPage = new BooksPagesService().GetPagesByBooks(kitapId);
            getBookPage = getBookPage ?? null;
            vmBook.BooksPageList = ObjectMapper.MapList(getBookPage!, new List<VM_BOOKS_PAGES>());

            VM_BOOKS_DETAIL dt = new VM_BOOKS_DETAIL();
            dt.BookModel = vmBook;
            return View(dt);
        }

        public IActionResult KitapEkleWordOrPdf()
        {
            VM_BOOKS_DETAIL vM_BOOKS_det = new VM_BOOKS_DETAIL();
            vM_BOOKS_det.BookModel = new VM_BOOKS();
            vM_BOOKS_det.BookModel.LoginUserId = (int)Convert.ToDecimal(Lid);

            var kategoriler = new BooksService().GetCategories();
            vM_BOOKS_det.Katergoriler = kategoriler!;
            return View(vM_BOOKS_det);
        }

        private async Task<ServiceResult> WordDosyasindaAktarma(VM_BOOKS_DETAIL vM_BOOKS)
        {
            string docxFilePath = DosyaYolu + vM_BOOKS.FDileName;
            ServiceResult result = new ServiceResult(state: MessageResultState.SUCCESS);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(docxFilePath, false))
            {
                // Access the main document part
                var mainPart = doc.MainDocumentPart;

                // Access the document body
                var body = mainPart!.Document.Body;

                var pageBreakCount = mainPart.Document.Descendants<LastRenderedPageBreak>().Count();

                int pageCount = pageBreakCount + 1;// word kaç sayfada oluşur bunu almaya çalıştık fakat tam istenileni vermedi. Bir fazlası veriyor

                int partCount = doc.Parts.Count(); // word kaç sayfada oluşur bunu almaya çalıştık fakat tam istenileni vermedi. Bİr eksik veriyor

                // Console.WriteLine($"Total Parts: {partCount}");

                int targetPageNumber = 2;
                string pageText = GetTextFromPage(body!, targetPageNumber);

                int wordCount = CountWords(pageText);

                var toplamParagrafUzunlugu = body.Elements<Paragraph>().ToList().Sum(p => p.InnerText.Length);

                string sayfaYazisi = "";

                int paragrafLimiti = 1800, paragrafU = 0;



                var toplamSayfaSayisiBelirlenenSayfaBasinaParagrafLimitiOlarak = toplamParagrafUzunlugu / paragrafLimiti;
                var yuvarlaSayfaIndex = Math.Ceiling(Convert.ToDecimal(toplamSayfaSayisiBelirlenenSayfaBasinaParagrafLimitiOlarak));

                foreach (var paragraph in body!.Elements<Paragraph>())
                {
                    paragrafU = paragrafU + paragraph.InnerText.Length;
                    sayfaYazisi += paragraph.InnerText;

                }
                //paragrafLimiti, iligli uzunluk: 220-230 cümleye denk gelmektedir.
                result = await SayfalariVeriTabaninaAktar(vM_BOOKS, sayfaYazisi, paragrafLimiti);

            }
            return result;

        }

        /// <summary>
        /// Pdf lerde sayfa sayfa veri tabanına ekleme yapıldı. 1800 limite şuan uyulmadı.
        /// </summary>
        /// <param name="vM_BOOKS"></param>

        private async Task<ServiceResult> PdfDosyasindanOkumaAsync(VM_BOOKS_DETAIL vM_BOOKS_det)
        {
            ServiceResult sonuc = new ServiceResult(state: MessageResultState.SUCCESS);

            string pdfFilePath = DosyaYolu + vM_BOOKS_det.FDileName;
            vM_BOOKS_det.BookModel = new VM_BOOKS();
            try
            {
                // İlk etapta kitabı ekle
                var eklemeDurumu = await KitapEkle(vM_BOOKS_det);

                if (eklemeDurumu.State == MessageResultState.SUCCESS)
                {
                    // PdfReader ve PdfDocument iText7 ile açılır
                    using var pdfReader = new PdfReader(pdfFilePath);
                    using var pdfDoc = new PdfDocument(pdfReader);

                    for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                    {
                        var sayfaText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), new SimpleTextExtractionStrategy());
                        Console.WriteLine($"sayfaYaz: {page} -- {sayfaText}\n");

                        // VM_BOOKS güncelle
                        vM_BOOKS_det.BookModel.ID = (int)eklemeDurumu.Result.BookID;
                        vM_BOOKS_det.BookPagesModel = new VM_BOOKS_PAGES();
                        vM_BOOKS_det.BookPagesModel.PageWrite = sayfaText.Trim();
                        vM_BOOKS_det.BookPagesModel.PageWriteBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(sayfaText));

                        // Her sayfa için kaydet
                        _ = await KitapEkle(vM_BOOKS_det);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
                sonuc.State = MessageResultState.ERROR;
            }

            return sonuc;
        }

        private async Task<ServiceResult> SayfalariVeriTabaninaAktar(VM_BOOKS_DETAIL vM_BOOKS_det, string sayfaYazisi, int aktarilmakIstenenLimit)
        {
            ServiceResult result = new ServiceResult(state: MessageResultState.SUCCESS);
            var eklemeDurumu = await KitapEkle(vM_BOOKS_det);//ilk etap kitabı ekle
            vM_BOOKS_det.BookModel = new VM_BOOKS();
            result.State = eklemeDurumu.State;

            if (eklemeDurumu.State == MessageResultState.SUCCESS)
            {
                vM_BOOKS_det.BookModel.ID = (int)eklemeDurumu.Result.BookID;
                result = await BoslugaGoreAyirVeIlgiliUzunlukKadarYaziSayfayaEkle(vM_BOOKS_det, sayfaYazisi, aktarilmakIstenenLimit);

            }
            return result;
        }
        private async Task<ServiceResult> DosyadanKitapAktar(VM_BOOKS_DETAIL vM_BOOKS_Det)
        {
            //string docxFilePath = DosyaYolu + vM_BOOKS.FDileName;
            ServiceResult result = new ServiceResult(state: MessageResultState.SUCCESS);
            try
            {
                if (vM_BOOKS_Det.pdfVeyaWord == 1)
                {
                    // word
                    result = await WordDosyasindaAktarma(vM_BOOKS_Det);
                }
                else
                {
                    // pdf okuma işlemini getir.
                    result = await PdfDosyasindanOkumaAsync(vM_BOOKS_Det);
                }

            }
            catch (Exception ex)
            {
                result.State = MessageResultState.ERROR;
                Console.WriteLine($"Hata: {ex.Message}");
            }
            return result;
        }

        private async Task<ServiceResult> BoslugaGoreAyirVeIlgiliUzunlukKadarYaziSayfayaEkle(VM_BOOKS_DETAIL vM_BOOKS_det, string input, int uzunluk)
        {
            ServiceResult sonuc = new ServiceResult(state: MessageResultState.SUCCESS);
            int length = input.Length;
            int numberOfChunks = (int)Math.Ceiling((double)length / uzunluk);

            string[] parcalar = new string[numberOfChunks];

            try
            {
                for (int i = 0; i < numberOfChunks; i++)
                {
                    int start = i * uzunluk;
                    int chunkLength = Math.Min(uzunluk, length - start);
                    // parcalar[i] = input.Substring(start, chunkLength);
                    var eklenilecekYazi = input.Substring(start, chunkLength);

                    if (!string.IsNullOrEmpty(eklenilecekYazi) && !char.IsWhiteSpace(eklenilecekYazi[eklenilecekYazi.Length - 1]))
                    {
                        // Son karakterde boşluk olmayan durumu kontrol et
                        int sonBoşlukIndex =
                        //input.LastIndexOf(' ');
                        eklenilecekYazi.LastIndexOf(' ');
                        uzunluk = sonBoşlukIndex;
                        if (sonBoşlukIndex != -1)
                        {
                            // Bir önceki boşluğu bul
                            parcalar[i] = input.Substring(start, sonBoşlukIndex);
                        }
                    }
                    else
                    {
                        parcalar[i] = input.Substring(start, chunkLength);
                    }

                    Console.WriteLine($"parcalar[" + i + "]: " + parcalar[i] + " start:" + start.ToString() + "\n");
                    vM_BOOKS_det.BookPagesModel = new VM_BOOKS_PAGES();
                    vM_BOOKS_det.BookPagesModel.PageWrite = parcalar[i].Trim();

                    byte[] result = Encoding.UTF8.GetBytes(parcalar[i]);
                    byte[] array = Encoding.ASCII.GetBytes(parcalar[i]);

                    vM_BOOKS_det.BookPagesModel.PageWriteBase64 = Convert.ToBase64String(result);

                    var kitapResult = await KitapEkle(vM_BOOKS_det);//kitabın sayfalarını ekle
                    sonuc.State = kitapResult.State;
                }
            }
            catch (Exception ex)
            {
                var _ = ex.Message;
                sonuc.State = MessageResultState.ERROR;
            }

            sonuc.Result = string.Join(" ", parcalar);
            return sonuc;
        }

        private bool AlinanDosyayiSablonDosyayaAktarma(string hedefDosyaYolu, VM_BOOKS_DETAIL vM_BOOKS)
        {
            var file = vM_BOOKS.dosyalar?.Where(p => p.FileName == vM_BOOKS.AktarilanDosya).FirstOrDefault();
            //Request.Form.Files;
            try
            {
                if (file != null)
                {
                    if (file.Length > 0)
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            file.CopyTo(memoryStream);

                            using (FileStream hedefDosya = new FileStream(hedefDosyaYolu, FileMode.Create))
                            {
                                // MemoryStream'deki veriyi hedef dosyaya kopyala
                                memoryStream.WriteTo(hedefDosya);
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata :" + ex.Message);
                return false;
            }



        }

        [HttpPost]
        public async Task<JsonResult> PostKitapEkleWordOrPdf(VM_BOOKS_DETAIL vM_BOOKS_det)
        {
            ServiceResult result = new ServiceResult(state: MessageResultState.SUCCESS);
            var vM_BOOKS = new VM_BOOKS();
            vM_BOOKS_det.BookModel = vM_BOOKS;

            string dosya = "";

            if (vM_BOOKS_det.pdfVeyaWord == 1)
            {
                dosya = "SablonWordBelge.docx";
            }
            else
            {
                dosya = "SablonPdfBelge.pdf";
            }

            string hedefDosyaYolu = DosyaYolu + dosya;

            vM_BOOKS_det.FDileName = dosya;

            bool isAktarma = AlinanDosyayiSablonDosyayaAktarma(hedefDosyaYolu, vM_BOOKS_det);

            if (isAktarma)
            {
                vM_BOOKS_det.BookModel.ID = 0;
                vM_BOOKS_det.isWordPDF = true;
                result = await DosyadanKitapAktar(vM_BOOKS_det);
                var bak = result;
            }
            else
            {
                result.State = MessageResultState.ERROR;
                result.Message = "Dosya eklemede durumunda hata oluştu.";
            }


            return Json(result);
        }

        static string GetTextFromPage(Body body, int pageNumber)
        {
            // Assuming each page is separated by a section break
            var paragraphs = body.Elements<Paragraph>().SkipWhile(p => p.GetFirstChild<ParagraphProperties>()?.
            GetFirstChild<SectionProperties>()?.GetFirstChild<NameIndex>()?.InnerText != pageNumber.ToString());

            // Concatenate the text of paragraphs on the specified page
            string pageText = string.Join(" ", paragraphs.Select(p => p.InnerText));

            return pageText;
        }

        static int CountWords(string text)
        {
            // Split the text into words using space as the delimiter
            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Return the count of words
            return words.Length;
        }

        [HttpPost]
        [Route("KitabiFavorilereEkle")]
        public JsonResult KitabiFavorilereEkle(string id)
        {
            var kId = StringCipher.Decrypt(id.ToString());
            var KİTAPıD = Convert.ToInt64(kId);
            FavoriBooks fAVORILER = new FavoriBooks()
            {
                BookId = KİTAPıD,
                UserId = Lid
            };
            var getFavori = new BooksService().PostFavoriBookSave(fAVORILER);


            return Json(getFavori);
        }

        [HttpPost]
        [Route("KitabiYayinaGonder")]
        public JsonResult KitabiYayinaGonder(long kitapid)
        {
            var getBook = new BooksService().GetBooks(kitapid);
            var _book = ObjectMapper.Map(getBook, new Books());
            ServiceResult<Books> checkerBook = new BooksService().CheckBook(_book);
            return Json(checkerBook);
        }


        [HttpPost]
        public string DosyaYukle(IFormFile dosya, VM_BOOKS input)
        {
            var ds = dosya;
            return "";
        }

        private async Task<VM_BOOKS> ModelIlgiliDosyalariDoldur(VM_BOOKS input, List<IFormFile> files)
        {
            foreach (var item in files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await item.CopyToAsync(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();

                    if (!string.IsNullOrEmpty(input.ONKAPAKFOTO) && input.ONKAPAKFOTO == item.FileName)
                    {

                        string base64String = Convert.ToBase64String(fileBytes);
                        input.ONKAPAKFOTO = base64String;
                        // input.ONKAPAKFOTOPATH = item.FileName;
                    }
                    if (!string.IsNullOrEmpty(input.ARKAKAPAKFOTO) && input.ARKAKAPAKFOTO == item.FileName)
                    {

                        string base64String = Convert.ToBase64String(fileBytes);
                        input.ARKAKAPAKFOTO = base64String;
                        // input.ARKAKAPAKFOTOPATH = item.FileName;
                    }
                    // KITAPSAYFAFOTO yayınlanacak versiyonda şuan düşümülmemiştir.
                    //if (!string.IsNullOrEmpty(input.KITAPSAYFAFOTO) && input.KITAPSAYFAFOTO == item.FileName)
                    //{

                    //    string base64String = Convert.ToBase64String(fileBytes);
                    //    input.KITAPSAYFAFOTO = base64String;
                    //}
                }
            }
            return input;
        }

        [HttpGet]
        public IActionResult Alert(VM_ALERT alert)
        {
            return PartialView(alert);
        }

        [HttpPost]
        public ServiceResult<Books> KitabiTamamla(VM_BOOKS input)
        {

            var getBook = new BooksService().GetBooks(input.ID ?? 0);
            var _book = ObjectMapper.Map(getBook, new Books());

            ServiceResult<Books> updaterBook = new BooksService().UpdateBook(_book);

            return updaterBook;

        }


        [HttpPost]
        public async Task<ServiceResult<VM_KITAP_EKLE>> KitapEkle(VM_BOOKS_DETAIL input)
        {
            int sayfaId = 0;
            int sayfaCount = 0;

            ServiceResult<VM_KITAP_EKLE> result = new ServiceResult<VM_KITAP_EKLE>();

            VM_KITAP_EKLE vmKitap = new VM_KITAP_EKLE();


            if (input.BookModel?.ID != null && input.BookModel.ID != 0)
            {
                var _book = ObjectMapper.Map(input.BookModel, new Books());
                var bookPapers = new VM_BOOKS_PAGES()
                {
                    isWordPDF = input.isWordPDF ?? false,
                    BooksId = input.BookModel.ID ?? 0,
                    PageWrite = input.BookPagesModel.PageWrite,
                    ID = input.BookPagesModel.ID,
                    PageFoto = input.BookPagesModel.PageFoto ?? "",
                    PageWriteBase64 = input.BookPagesModel.PageWriteBase64 ?? ""

                };
                var bookPages = new BooksPagesService().PostSaveBooksPages(bookPapers);
                if (bookPages.Result != null)
                {
                    sayfaCount = new BooksPagesService().GetPagesByBooks(input.BookModel.ID ?? 0)!.Count();
                    sayfaId = (int)bookPages.Result.ID;

                    result.State = MessageResultState.SUCCESS;

                }
                else
                {
                    new BooksService().DeleteBook(_book);
                    result.State = MessageResultState.ERROR;
                    result.Message = result.Message;
                }

                vmKitap.BookID = input.BookModel.ID ?? 0;
                vmKitap.BooksPageCount = sayfaCount + 1;
                vmKitap.BooksPageID = sayfaId;
                input.IlgiiSayfaSayisi = sayfaId;

            }
            else
            {

                VM_BOOKS vM_BOOKS = input.BookModel;
                vM_BOOKS.YazarId = (int)Lid;
                vM_BOOKS.TAMAMLANDIMI = input.isPdfVeyaWordTamalama ?? false;
                //vM_BOOKS.BookModel = kitap;

                var book = await new BooksService().PostSaveBook(input);

                if (book.Result == null)
                {
                    result.State = MessageResultState.ERROR;
                    result.Message = "Kitap Ekleme Başarısız";

                    return result;
                }
                if (!(input.isWordPDF ?? false))
                {
                    if (book?.Result.ID != 0 && book?.Result != null)
                    {
                        var bookPapers = new VM_BOOKS_PAGES()
                        {
                            isWordPDF = input.isWordPDF ?? false,
                            BooksId = book.Result.ID,
                            PageWrite = input.BookPagesModel?.PageWrite,
                            PageFoto = input.BookPagesModel?.PageFoto ?? "",
                            PageWriteBase64 = input.BookPagesModel?.PageWriteBase64 ?? ""

                        };
                        var bookPages = new BooksPagesService().PostSaveBooksPages(bookPapers);

                        if (bookPages.Result != null && bookPages.Result.ID != 0)
                        {
                            input.BookPagesModel.ID = bookPages.Result.ID;
                            sayfaId = (int)bookPages.Result.ID;
                            sayfaCount = (int)bookPages.Result.ID + 1;
                            //new BooksPagesService().GetPagesByBooks(book.Result.ID)!.Where(p => p.BookId == book.Result.ID).Count();

                            result.State = MessageResultState.SUCCESS;
                        }
                        else
                        {
                            new BooksService().DeleteBook(book.Result);

                            result.State = MessageResultState.ERROR;
                            result.Message = bookPages.Message;
                        }

                        vmKitap.BookID = book.Result!.ID;
                        vmKitap.BooksPageID = sayfaId;
                        vmKitap.BooksPageCount = sayfaCount + 1;
                        input.IlgiiSayfaSayisi = sayfaCount + 1;
                    }
                    else
                    {
                        result.State = MessageResultState.ERROR;
                        result.Message = result.Message;
                    }

                }
                else
                {
                    if (book.Result != null)
                    {
                        result.State = MessageResultState.SUCCESS;
                    }
                    else
                    {
                        result.State = MessageResultState.ERROR;
                    }

                    vmKitap.BookID = book.Result != null ? book.Result.ID : 0;

                }


            }

            result.Result = vmKitap;
            //result.State = MessageResultState.SUCCESS;
            return result;

        }

        public IActionResult KitapGuncelleme(string kitap_id)
        {
            var kitapId = Convert.ToInt64(StringCipher.Decrypt(kitap_id));
            ViewBag.KITAPENC = kitap_id;
            var getBook = new BooksService().GetBooks(kitapId);
            ViewBag.VM_KITAP = getBook;
            var vmBook = ObjectMapper.Map(getBook, new VM_BOOKS());
            var getBookPage = new BooksPagesService().GetPagesByBooks(kitapId);
            getBookPage = getBookPage ?? null;
            vmBook.BooksPageList = ObjectMapper.MapList(getBookPage!, new List<VM_BOOKS_PAGES>());

            VM_BOOKS_DETAIL dt = new VM_BOOKS_DETAIL();
            dt.BookModel = vmBook;
            return View(dt);
        } 

        [HttpPost]
        [Route("PostBooksPageUpdate")]
        public JsonResult PostBooksPageUpdate(VM_BOOKS_PAGES sayfa)
        { 
            ServiceResult sonuc = new BooksPagesService().PostUpdateBooksPages(sayfa);
            return Json(sonuc);
        }

        public IActionResult KitapDetay(string kitap_id)
        {
            var kitapId = Convert.ToInt64(StringCipher.Decrypt(kitap_id));
            VM_BOOKS_DETAIL kitapDetay = new VM_BOOKS_DETAIL();

            var vmBook = new BooksService().GetBooks(kitapId);
            ViewBag.iSTATISTIK = vmBook?.iSTATISTIK;

            ViewBag.YayinAyar = new AyarlarService().GetYayinAyarlariByBookId(kitapId);

            kitapDetay.isTamalama = vmBook?.TAMAMLANDIMI;
            kitapDetay.BookModel = vmBook;

            var Bildirim = new BildirimlerService().GetBildirimlerByLoginId(Lid);

            ViewBag.isTakip = Bildirim?.IsTakip ?? false; //Birisi beni takip ettiğinde bana e-posta gönder
            ViewBag.IsKitapYayin = Bildirim?.IsKitapYayin ?? false;//Birisi kitap yayınladığında bana bildirim yolla
            ViewBag.IsKitapYorum = Bildirim?.IsKitapYorum ?? false;//Birisi kitabıma yorum yaptığında bana e-posta gönder

            return View(kitapDetay);
        }

        public IActionResult TumKitaplar(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS_DETAIL kitapDetay = new VM_BOOKS_DETAIL();
            kitapDetay.BookModel = new VM_BOOKS();

            var getBookList = new BooksService().TumKitaplar(arama);
            kitapDetay.VMBooksList = getBookList!;

            kitapDetay.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            ViewBag.YayinAyar = new AyarlarService().GetYyainAyarlari();
            ViewBag.LOGIN_USER_ID = Lid;

            return View(kitapDetay);
        }
        public IActionResult AraButonFiltrelemeKitaplar(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS_DETAIL kitapDetay = new VM_BOOKS_DETAIL();
            kitapDetay.BookModel = new VM_BOOKS();

            kitapDetay.sliderdaGosterilecekKayit = (arama.listelenecek_kayit_sayisi != 0 ? arama.listelenecek_kayit_sayisi : 0);
            kitapDetay.profilKitapTuru = arama.profilKitapTuru ?? "";
            kitapDetay.isAnaSayfa = false;
            kitapDetay.Tip = arama.profilKitapTuru + "-" + arama.Tip;
            List<VM_BOOKS>? bookList = new BooksService().TumKitaplar(arama);
            //vM_BOOKS.Stars = new BooksService().GetMaxStarBooks();
            ViewBag.LOGIN_USER_ID = Lid;
            ViewBag.YayinAyar = new AyarlarService().GetYyainAyarlari();
            kitapDetay.VMBooksList = bookList!;

            return View(kitapDetay);
        }

        [HttpPost]
        [Route("PostBooksStars")]
        public ServiceResult<BooksStars> PostBooksStars(long kitapId, int yildizPuani)
        {
            ServiceResult<BooksStars> result = new ServiceResult<BooksStars>();

            BooksStars bookStar = new BooksStars()
            {
                BookId = Convert.ToInt32(kitapId),
                LoginUserId = Convert.ToInt32(Lid),
                StarPuani = yildizPuani
            };
            result = new BooksService().PostBooksStars(bookStar);

            return result;
        }


        [HttpPost]
        [Route("PostBooksDegerlendirme")]
        public JsonResult PostBooksDegerlendirme(VM_BOOKS_DEGERLENDIRME degerlendirme)
        {

            ServiceResult<BooksStars> sonuc = PostBooksStars(degerlendirme.BookId ?? 0, degerlendirme.StarPuani);
            ServiceResult result = new ServiceResult();

            if (sonuc.State == MessageResultState.SUCCESS)
            {
                degerlendirme.LoginUserId = Convert.ToInt32(Lid);
                result = new BooksService().PostBooksDegerlendirme(degerlendirme);
            }
            else
            {
                result.State = MessageResultState.ERROR;
            }

            return Json(result);
        }

        [HttpPost]
        [Route("PostBooksComments")]
        public JsonResult PostBooksComments(VM_BOOKS_COMMENT mesajlar)
        {

            ServiceResult result = new ServiceResult();

            mesajlar.LoginUserId = Convert.ToInt32(Lid);
            result = new BooksService().PostBooksComments(mesajlar);

            return Json(result);
        }

    }
}
