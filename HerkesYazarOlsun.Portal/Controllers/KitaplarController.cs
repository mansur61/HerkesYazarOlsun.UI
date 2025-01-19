using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.Security.Cryptography;

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
        public IActionResult KitapOlustur(VM_BOOKS input)
        {
            if (input.ID != 0)
            {
                int sayfaCount = new BooksPagesService().GetPagesByBooks(input.ID)!.Count();
                input.IlgiiSayfaSayisi = sayfaCount;
            }

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

        public IActionResult KitapEkleWordOrPdf()
        {
            VM_BOOKS vM_BOOKS = new VM_BOOKS();
            vM_BOOKS.LoginUserId = 6;
            return View(vM_BOOKS);
        }

        private async Task<ServiceResult> WordDosyasindaAktarma(VM_BOOKS vM_BOOKS)
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
        private async Task<ServiceResult> PdfDosyasindanOkumaAsync(VM_BOOKS vM_BOOKS)
        {
            ServiceResult sonuc = new ServiceResult(state: MessageResultState.SUCCESS);

            string pdfFilePath = DosyaYolu + vM_BOOKS.FDileName;
           // int paragrafLimiti = 1800;

            PdfReader reader = new PdfReader(pdfFilePath);
            string text = string.Empty;

            try
            {
                var eklemeDurumu = await KitapEkle(vM_BOOKS);//ilk etap kitabı ekle

                if (eklemeDurumu.State == MessageResultState.SUCCESS)
                {
                    for (int page = 1; page <= reader.NumberOfPages; page++)
                    {
                        text += PdfTextExtractor.GetTextFromPage(reader, page);
                        string sayfaYaz = PdfTextExtractor.GetTextFromPage(reader, page);
                        Console.WriteLine("sayfaYaz: " + page + " -- " + sayfaYaz + "\n");

                        if (eklemeDurumu.State == MessageResultState.SUCCESS)
                        {
                            vM_BOOKS.ID = eklemeDurumu.Result.BookID;
                            vM_BOOKS.SAYFAYAZI = sayfaYaz.Trim();
                            byte[] result = Encoding.UTF8.GetBytes(sayfaYaz);
                            vM_BOOKS.SAYFAYAZIBASE64 = Convert.ToBase64String(result);
                            _ = KitapEkle(vM_BOOKS);
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception)
            {

                sonuc.State = MessageResultState.ERROR;
            }

            return sonuc;

        }
        private async Task<ServiceResult> SayfalariVeriTabaninaAktar(VM_BOOKS vM_BOOKS, string sayfaYazisi, int aktarilmakIstenenLimit)
        {
            ServiceResult result = new ServiceResult(state: MessageResultState.SUCCESS);
            var eklemeDurumu = await KitapEkle(vM_BOOKS);//ilk etap kitabı ekle
            result.State = eklemeDurumu.State;

            if (eklemeDurumu.State == MessageResultState.SUCCESS)
            {
                vM_BOOKS.ID = eklemeDurumu.Result.BookID;
                result = await BoslugaGoreAyirVeIlgiliUzunlukKadarYaziSayfayaEkle(vM_BOOKS, sayfaYazisi, aktarilmakIstenenLimit);

            }
            return result;
        }
        private async Task<ServiceResult> DosyadanKitapAktar(VM_BOOKS vM_BOOKS)
        {
            //string docxFilePath = DosyaYolu + vM_BOOKS.FDileName;
            ServiceResult result = new ServiceResult(state: MessageResultState.SUCCESS);
            try
            {
                if (vM_BOOKS.pdfVeyaWord == 1)
                {
                    // word
                    result =  await WordDosyasindaAktarma(vM_BOOKS);
                }
                else
                {
                    // pdf okuma işlemini getir.
                    result = await PdfDosyasindanOkumaAsync(vM_BOOKS);
                }

            }
            catch (Exception ex)
            {
                result.State = MessageResultState.ERROR;
                Console.WriteLine($"Hata: {ex.Message}");
            }
            return result;
        }

        private async Task<ServiceResult> BoslugaGoreAyirVeIlgiliUzunlukKadarYaziSayfayaEkle(VM_BOOKS vM_BOOKS, string input, int uzunluk)
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
                    vM_BOOKS.SAYFAYAZI = parcalar[i].Trim();
                    byte[] result = Encoding.UTF8.GetBytes(parcalar[i]);
                    byte[] array = Encoding.ASCII.GetBytes(parcalar[i]);
                    vM_BOOKS.SAYFAYAZIBASE64 = Convert.ToBase64String(result);

                    var kitapResult = await KitapEkle(vM_BOOKS);//kitabın sayfalarını ekle
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

        private byte GetByte(byte[] bytes)
        {
            byte currentByte = 0;
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                // BinaryReader kullanarak MemoryStream'den okuma yap
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    // MemoryStream'in sonuna kadar byte'ları oku
                    while (memoryStream.Position < memoryStream.Length)
                    {
                        // Bir byte oku
                        currentByte = reader.ReadByte();

                        // Okunan byte'ı kullan veya işle
                        Console.Write($"{currentByte} ");


                    }
                }
            }
            return currentByte;
        }
        private byte[]? GetByteler()
        {
            byte[]? bytes;
            var item = Request.Form.Files[0];
            using (var ms = new MemoryStream())
            {
                item.CopyTo(ms);

                bytes = ms.ToArray();
                string resultString = Encoding.UTF8.GetString(bytes);

                Console.Write($"{resultString} ");

            }
            return bytes;
        }

        private bool AlinanDosyayiSablonDosyayaAktarma(string hedefDosyaYolu, VM_BOOKS vM_BOOKS)
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


                Console.WriteLine("Dosya aktarma işlemi tamamlandı.");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata :" + ex.Message);
                return false;
            }



        }

        [HttpPost]
        //[Route("PostKitapEkleWordOrPdf")]
        public async Task<JsonResult> PostKitapEkleWordOrPdf(VM_BOOKS vM_BOOKS) // JsonResult oalrakta çalıştır
        {
            ServiceResult result = new ServiceResult(state: MessageResultState.SUCCESS);

            string dosya = "";

            if (vM_BOOKS.pdfVeyaWord == 1)
            {
                dosya = "SablonWordBelge.docx";
            }
            else
            {
                dosya = "SablonPdfBelge.pdf";
            }

            string hedefDosyaYolu = DosyaYolu + dosya;

            vM_BOOKS.FDileName = dosya;

            bool isAktarma = AlinanDosyayiSablonDosyayaAktarma(hedefDosyaYolu, vM_BOOKS);
            //false;
           
            if (isAktarma)
            {
                vM_BOOKS.ID = 0;
                vM_BOOKS.isWordPDF = true;
                result = await DosyadanKitapAktar(vM_BOOKS);
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
                BOOKS_ID = KİTAPıD,
                USER_ID = Lid
            };
            var getFavori = new BooksService().PostFavoriBookSave(fAVORILER);


            return Json(getFavori);
        }

        [HttpPost]
        [Route("KitabiYayinaGonder")]
        public JsonResult KitabiYayinaGonder(long kitapid)
        {
            var getBook = new BooksService().GetBooks(kitapid);
            ServiceResult<Books> checkerBook = new BooksService().CheckBook(getBook!);
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
                    }
                    if (!string.IsNullOrEmpty(input.ARKAKAPAKFOTO) && input.ARKAKAPAKFOTO == item.FileName)
                    {

                        string base64String = Convert.ToBase64String(fileBytes);
                        input.ARKAKAPAKFOTO = base64String;
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

            var getBook = new BooksService().GetBooks(input.ID);
            ServiceResult<Books> updaterBook = new BooksService().UpdateBook(getBook!);

            return updaterBook;

        }


        [HttpPost]
        //[Route("KitapEkle")]
        public async Task<ServiceResult<VM_KITAP_EKLE>> KitapEkle(VM_BOOKS input)
        {
            int sayfaId = 0;
            int sayfaCount = 0;

            // Request.Form.Files;
            var files = input.dosyalar;
            if (files?.Count != 0 && files != null)
            {
                input = await ModelIlgiliDosyalariDoldur(input, files);
            }

            ServiceResult<VM_KITAP_EKLE> result = new ServiceResult<VM_KITAP_EKLE>();

            VM_KITAP_EKLE vmKitap = new VM_KITAP_EKLE();
            //var getBook = new BooksService().GetBooks(input.ID);
            if (input.ID != 0)
            {
                var _book = ObjectMapper.Map(input, new Books());
                var bookPages = new BooksPagesService().PostSaveBooksPages(new VM_BOOKS_PAGES()
                {
                    isWordPDF = input.isWordPDF,
                    BooksId = input.ID,
                    PageWrite = input.SAYFAYAZI,
                    PageWriteBase64 = input.SAYFAYAZIBASE64 == null ? "" : input.SAYFAYAZIBASE64,
                    PageFoto = input.KITAPSAYFAFOTO != null ? input.KITAPSAYFAFOTO : ""

                });
                if (bookPages.Result != null)
                {
                    sayfaCount = new BooksPagesService().GetPagesByBooks(input.ID)!.Count();
                    sayfaId = (int)bookPages.Result.ID;

                    result.State = MessageResultState.SUCCESS;

                }
                else
                {
                    new BooksService().DeleteBook(_book);
                    result.State = MessageResultState.ERROR;
                    result.Message = result.Message;
                }

                vmKitap.BookID = input.ID;
                vmKitap.BooksPageCount = sayfaCount + 1;
                vmKitap.BooksPageID = sayfaId;
                input.IlgiiSayfaSayisi = sayfaId;

            }
            else
            {
                var book = new BooksService().PostSaveBook(new Books()
                {
                    ARKAKAPAKFOTO = input.ARKAKAPAKFOTO != null ? input.ARKAKAPAKFOTO : "",
                    ONSOZ = input.ONSOZ,
                    YazarId = Lid,
                    ONKAPAKFOTO = input.ONKAPAKFOTO != null ? input.ONKAPAKFOTO : "",
                    CategoriId = input.CategoriId,
                    TAMAMLANDIMI = input.isPdfVeyaWordTamalama ?? false,
                    Name = input.Name != null ? input.Name : "",
                    ARKAKAPAKYAZISI = input.ARKAKAPAKYAZISI != null ? input.ARKAKAPAKYAZISI : ""

                });
                if (!input.isWordPDF)
                {
                    if (book?.Result.ID != 0 && book?.Result != null)
                    {
                        var bookPages = new BooksPagesService().PostSaveBooksPages(new VM_BOOKS_PAGES()
                        {
                            isWordPDF = input.isWordPDF,
                            BooksId = book.Result.ID,
                            PageWrite = input.SAYFAYAZI,
                            PageWriteBase64 = input.SAYFAYAZIBASE64 == null ? "" : input.SAYFAYAZIBASE64,
                            PageFoto = input.KITAPSAYFAFOTO != null ? input.KITAPSAYFAFOTO : ""

                        });
                        if (bookPages.Result != null && bookPages.Result.ID != 0)
                        {
                            input.ID = bookPages.Result.ID;
                            sayfaId = (int)bookPages.Result.ID;
                            sayfaCount = new BooksPagesService().GetPagesByBooks(book.Result.ID)!.Where(p => p.BooksId == book.Result.ID).Count();

                            result.State = MessageResultState.SUCCESS;
                        }
                        else
                        {
                            new BooksService().DeleteBook(book.Result);

                            result.State = MessageResultState.ERROR;
                            result.Message = result.Message;
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

        public IActionResult KitapGuncelleme(long kitapId)
        {
            var getBook = new BooksService().GetBooks(kitapId);
            var vmBook = ObjectMapper.Map(getBook, new VM_BOOKS());
            var getBookPage = new BooksPagesService().GetPagesByBooks(kitapId);
            getBookPage = getBookPage ?? null;
            vmBook.BooksPageList = ObjectMapper.MapList(getBookPage!, new List<VM_BOOKS_PAGES>());
            return View(vmBook);
        }

        public IActionResult GetIlgiliKitapSayfasi(long kitapSayfaId, long? kitapId = 0)
        {
            var getBookPage = new BooksPagesService().GetBooksPages(kitapSayfaId);
            getBookPage = getBookPage ?? null;
            var kitapSayfa = ObjectMapper.Map(getBookPage!, new VM_BOOKS_PAGES());
            return PartialView(kitapSayfa);
        }

        [HttpPost]
        [Route("PostBooksPageUpdate")]
        public JsonResult PostBooksPageUpdate(VM_BOOKS_PAGES sayfa)
        {
            sayfa.PageFoto = "";
            ServiceResult sonuc = new BooksPagesService().PostUpdateBooksPages(sayfa);
            return Json(sonuc);
        }

        public IActionResult KitapDetay(long kitapId)
        {
            VM_BOOKS_DETAY kitapDetay = new VM_BOOKS_DETAY();

            var getBookDetay = new BooksService().GetBooks(kitapId);
            var vmBook = ObjectMapper.Map(getBookDetay, new VM_BOOKS());

            vmBook.isTamalama = getBookDetay.TAMAMLANDIMI;
            kitapDetay.Vm_Book = vmBook;
            kitapDetay.Vm_Book_Degerlendirme_List = new BooksService().GetDegerlendirmelerBooksById(kitapId);
            kitapDetay.Vm_Book_Comments = new BooksService().GetCommenstBooksById(kitapId);

            kitapDetay.Stars = new BooksService().GetMaxStarBooksById(kitapId);

            return View(kitapDetay);
        }

        public IActionResult TumKitaplar(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS vM_BOOKS = new VM_BOOKS();
            // vM_BOOKS.Stars = new BooksService().GetMaxStarBooks();
            //vM_BOOKS.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;
            var getBookList = new BooksService().TumKitaplar(arama);
            vM_BOOKS.VMBooksList = getBookList!;
            vM_BOOKS.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;
            return View(vM_BOOKS);
        }
        public IActionResult AraButonFiltrelemeKitaplar(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS vM_BOOKS = new VM_BOOKS();
            vM_BOOKS.sliderdaGosterilecekKayit = (arama.listelenecek_kayit_sayisi != 0 ? arama.listelenecek_kayit_sayisi : 0);
            vM_BOOKS.profilKitapTuru = arama.profilKitapTuru ?? "";
            vM_BOOKS.isAnaSayfa = false;
            vM_BOOKS.Tip = arama.profilKitapTuru + "-" + arama.Tip;
            List<VM_BOOKS>? bookList = new BooksService().TumKitaplar(arama);
            //vM_BOOKS.Stars = new BooksService().GetMaxStarBooks();
            vM_BOOKS.VMBooksList = bookList!;

            return View(vM_BOOKS);
        }

        [HttpPost]
        [Route("PostBooksStars")]
        public ServiceResult<BooksStars> PostBooksStars(long kitapId, int yildizPuani)
        {
            ServiceResult<BooksStars> result = new ServiceResult<BooksStars>();

            BooksStars bookStar = new BooksStars()
            {
                BookaId = Convert.ToInt32(kitapId),
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
