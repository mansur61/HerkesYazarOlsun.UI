using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;

namespace HerkesYazarOlsun.Portal.Controllers
{

    public class KitaplarController : Controller
    {
        private IHttpContextAccessor _contextAccessor;
        private long Lid;
        private int ParagrafLimiti = 0;
        private int SliderdaGosterilecekKayit = 0;
        private int DefaultSliderdaGosterilecekKayit = 0;
        private int KitapTamamlamaSayisi = 0;
        private readonly string _dosyaBaseUrl;
        // kullanımdan alındı
        private string DosyaYolu = "";
        public KitaplarController(IHttpContextAccessor contextAccessor, IOptions<HelperSettings> helperSettings)
        {
            _contextAccessor = contextAccessor;
            Lid = (long)(_contextAccessor?.HttpContext?.User.GetLoginUserId());
            _dosyaBaseUrl = helperSettings.Value.DosyaBaseUrl;
            ParagrafLimiti = helperSettings.Value.ParagrafLimiti;
            SliderdaGosterilecekKayit = helperSettings.Value.SliderdaGosterilecekKayit;
            DefaultSliderdaGosterilecekKayit = helperSettings.Value.DefaultSliderdaGosterilecekKayit;
            KitapTamamlamaSayisi = helperSettings.Value.KitapTamamlamaSayisi;

            ViewBag.SliderdaGosterilecekKayit = SliderdaGosterilecekKayit;
            ViewBag.DefaultSliderdaGosterilecekKayit = DefaultSliderdaGosterilecekKayit;
            ViewBag.KitapTamamlamaSayisi = KitapTamamlamaSayisi;

            DosyaYolu = _dosyaBaseUrl;
        }
        public IActionResult KitapOlustur(VM_BOOKS_DETAIL input)
        {
            ViewBag.KitapTamamlamaSayisi = KitapTamamlamaSayisi;
            long bookID = Convert.ToInt64(StringCipher.Decrypt(input.kitap_id));
            ViewBag.BOOK_ID = bookID;

            if (input.isYazmayaDevamEt.HasValue && !input.isYazmayaDevamEt.Value) //isYazmayaDevamEt = false ise
            {
                var kategoriler = new BooksService().GetCategories();
                input.Katergoriler = kategoriler!;
                input.IlgiiSayfaSayisi = 1;
            }
             
            

            if (input.isYazmayaDevamEt.HasValue && input.isYazmayaDevamEt.Value)//
            {
                if (input.IlgiiSayfaSayisi.HasValue && input.IlgiiSayfaSayisi.Value == 0)
                {
                    input.IlgiiSayfaSayisi = 2;
                }

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

        /// <summary>
        /// Pdf lerde sayfa sayfa veri tabanına ekleme yapıldı. {ParagrafLimiti} limiti şeklinde ekleme
        /// </summary>
        /// <param name="vM_BOOKS"></param>
        private async Task<ServiceResult> WordDosyasindaAktarmaAsync(VM_BOOKS_DETAIL vM_BOOKS)
        {
            ServiceResult result = new ServiceResult(state: MessageResultState.SUCCESS);

            try
            {
                if (vM_BOOKS.DocMemoryStream == null)
                {
                    result.Message = "DocMemoryStream boş.";
                    result.State = MessageResultState.ERROR;
                    return result;
                }

                // Stream başa alınmalı
                vM_BOOKS.DocMemoryStream.Position = 0;

                using (WordprocessingDocument doc = WordprocessingDocument.Open(vM_BOOKS.DocMemoryStream, false))
                {
                    if (doc.MainDocumentPart == null || doc.MainDocumentPart.Document?.Body == null)
                    {
                        result.Message = "Belge yapısı geçersiz.";
                        result.State = MessageResultState.ERROR;
                        return result;
                    }

                    // Access the main document part
                    var mainPart = doc.MainDocumentPart;

                    // Access the document body
                    var body = mainPart.Document.Body;

                    //WORD İÇİ RESİM TESPİT
                    var firstImagePart = mainPart.ImageParts.FirstOrDefault();
                    if (firstImagePart != null)
                    {
                        byte[] imageBytes;

                        await using (var imgStream = firstImagePart.GetStream())
                        await using (var ms = new MemoryStream())
                        {
                            await imgStream.CopyToAsync(ms);
                            imageBytes = ms.ToArray(); // KOPYA ALINDI
                        }

                        //  Bundan sonrası thread-safe
                        var formFile = imageBytes.ToImageFormFile(
                            fileName: "word_img_1",
                            contentType: firstImagePart.ContentType
                        );

                        if (formFile != null)
                        {
                            vM_BOOKS.BookPagesModel ??= new VM_BOOKS_PAGES();

                            // Her çağrıda YENİ liste → concurrency safe
                            vM_BOOKS.BookPagesModel.PageFotoDosyalar =
                                new List<IFormFile> { formFile };
                        }
                    }


                    var pageBreakCount = mainPart.Document.Descendants<LastRenderedPageBreak>().Count();

                    // Word kaç sayfada oluşur bunu almaya çalıştık fakat tam istenileni vermedi. Bir fazlası veriyor
                    int pageCount = pageBreakCount + 1;

                    // Word kaç sayfada oluşur bunu almaya çalıştık fakat tam istenileni vermedi. Bir eksik veriyor
                    int partCount = doc.Parts.Count();

                    int targetPageNumber = 2;
                    string pageText = GetTextFromPage(body, targetPageNumber);

                    int wordCount = CountWords(pageText);

                    var paragraphs = body.Elements<Paragraph>().ToList();
                    var toplamParagrafUzunlugu = paragraphs.Sum(p => p.InnerText.Length);

                    // RAM tasarrufu için StringBuilder kullanıyoruz (özellikle büyük belgelerde fark yaratır)
                    StringBuilder sayfaYazisiBuilder = new StringBuilder();

                    int paragrafLimiti = ParagrafLimiti;
                    int paragrafU = 0;

                    var toplamSayfaSayisiBelirlenenSayfaBasinaParagrafLimitiOlarak = toplamParagrafUzunlugu / paragrafLimiti;
                    var yuvarlaSayfaIndex = Math.Ceiling(Convert.ToDecimal(toplamSayfaSayisiBelirlenenSayfaBasinaParagrafLimitiOlarak));

                    foreach (var paragraph in paragraphs)
                    {
                        paragrafU += paragraph.InnerText.Length;
                        sayfaYazisiBuilder.Append(paragraph.InnerText);
                    }

                    string sayfaYazisi = sayfaYazisiBuilder.ToString();

                    // paragrafLimiti, ilgili uzunluk: 220-230 cümleye denk gelmektedir.
                    result = await SayfalariVeriTabaninaAktar(vM_BOOKS, sayfaYazisi, paragrafLimiti);
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.State = MessageResultState.ERROR;
                return result;
            }
        }

        /// <summary>
        /// Pdf lerde sayfa sayfa veri tabanına ekleme yapıldı. {ParagrafLimiti} limite şuan uyulmadı.
        /// </summary>
        /// <param name="vM_BOOKS"></param>

        private async Task<ServiceResult> PdfDosyasindanOkumaAsync(VM_BOOKS_DETAIL vM_BOOKS_det)
        {
            ServiceResult sonuc = new ServiceResult(state: MessageResultState.SUCCESS);
            //vM_BOOKS_det.BookModel = new VM_BOOKS();
            try
            {
                if (vM_BOOKS_det.PdfMemoryStream == null)
                {
                    sonuc.Message = "PdfMemoryStream  boş.";
                    sonuc.State = MessageResultState.ERROR;
                    return sonuc;
                }

                // Stream başa alınmalı
                vM_BOOKS_det.PdfMemoryStream.Position = 0;

                // İlk etapta kitabı ekle
                var eklemeDurumu = await KitapEkle(vM_BOOKS_det);

                if (eklemeDurumu.State == MessageResultState.SUCCESS && eklemeDurumu.Result != null)
                {
                    sonuc.Result = StringCipher.Encrypt(eklemeDurumu.Result.BookID.ToString());
                }

                if (eklemeDurumu.State == MessageResultState.SUCCESS)
                {
                    // PdfReader ve PdfDocument iText7 ile açılır
                    using var pdfReader = new PdfReader(vM_BOOKS_det.PdfMemoryStream);
                    using var pdfDoc = new PdfDocument(pdfReader);

                    for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                    {
                        var sayfaText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), new SimpleTextExtractionStrategy());
                        Console.WriteLine($"sayfaYaz: {page} -- {sayfaText}\n");

                        // RESİMLERİ YAKALA
                        var imageListener = new PdfImageListener();
                        var processor = new PdfCanvasProcessor(imageListener);
                        processor.ProcessPageContent(pdfDoc.GetPage(page));

                        // VM_BOOKS güncelle
                        vM_BOOKS_det.BookModel.ID = (int)eklemeDurumu.Result.BookID;
                        vM_BOOKS_det.BookPagesModel = new VM_BOOKS_PAGES();
                        vM_BOOKS_det.BookPagesModel.PageWrite = sayfaText.Trim();
                        vM_BOOKS_det.BookPagesModel.PageWriteBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(sayfaText));

                        var firstImage = imageListener.Images?.FirstOrDefault();

                        var formFile = firstImage?.ToImageFormFile(
                            fileName: $"page_{page}_img_1"
                        );

                        if (formFile != null)
                        {
                            vM_BOOKS_det.BookPagesModel.PageFotoDosyalar = new List<IFormFile> { formFile };
                        }

                        //vM_BOOKS_det.BookPagesModel.PageFotoDosyalar = 
                        //    vM_BOOKS_det.BookPagesModel.PageFotoDosyalar =
                        //    imageListener.Images
                        //        .Select((img, index) =>
                        //            img.ToFormFile(
                        //                fileName: $"page_{page}_img_{index + 1}.png",
                        //                contentType: "image/png"
                        //            )
                        //        ) .ToList();

                        // Her sayfa için kaydet
                        _ = await KitapEkle(vM_BOOKS_det);
                    }
                }
                else
                {
                    sonuc.State = MessageResultState.ERROR;
                    sonuc.Message = eklemeDurumu.Message;
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
            result.Message = eklemeDurumu.Message;
            if (eklemeDurumu.State == MessageResultState.SUCCESS && eklemeDurumu.Result != null)
            {
                result.Result = StringCipher.Encrypt(eklemeDurumu.Result.BookID.ToString());
            }

            if (eklemeDurumu.State == MessageResultState.SUCCESS)
            {
                vM_BOOKS_det.BookModel.ID = (int)eklemeDurumu.Result.BookID;

                result = await BoslugaGoreAyirVeIlgiliUzunlukKadarYaziSayfayaEkle(vM_BOOKS_det, sayfaYazisi, aktarilmakIstenenLimit);

            }

            // ilgili kitabın enc hali
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

            //sonuc.Result = string.Join(" ", parcalar);
            return sonuc;
        }

        private async Task<ServiceResult> AlinanDosyayiBellekteAktarmaAsync(VM_BOOKS_DETAIL vM_BOOKS)
        {
            var sonuc = new ServiceResult(state: MessageResultState.SUCCESS);
            try
            {
                // Kullanıcıdan gelen dosyayı bul
                var file = vM_BOOKS.dosyalar?.FirstOrDefault(p => p.FileName == vM_BOOKS.AktarilanDosya);
                if (file == null)
                {
                    sonuc.State = MessageResultState.ERROR;
                    sonuc.Message = "Dosya Yükleyiniz";
                    return sonuc;
                }

                if (file != null)
                {
                    var uzanti = Path.GetExtension(file.FileName)?.ToLower();
                    if (uzanti != ".pdf" && uzanti != ".docx" && uzanti != ".doc")
                    {
                        sonuc.State = MessageResultState.ERROR;
                        sonuc.Message = "Yalnızca PDF veya Word dosyaları yükleyebilirsiniz.";
                        return sonuc;
                    }
                }

                // Kullanıcı dosyasını belleğe oku
                using (var tempStream = new MemoryStream())
                {
                    await file!.CopyToAsync(tempStream); // asenkron ve thread-safe
                    tempStream.Position = 0;

                    // Bellekteki veriyi vM_BOOKS.DocMemoryStream'e kopyala
                    if (vM_BOOKS.pdfVeyaWord == 1)
                    {
                        vM_BOOKS.DocMemoryStream = new MemoryStream(tempStream.ToArray());
                        vM_BOOKS.DocMemoryStream.Position = 0;
                    }
                    else
                    {
                        vM_BOOKS.PdfMemoryStream = new MemoryStream(tempStream.ToArray());
                        vM_BOOKS.PdfMemoryStream.Position = 0;
                    }

                }

                return sonuc;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");

                sonuc.State = MessageResultState.ERROR;
                sonuc.Message = ex.Message;
                return sonuc;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostKitapEkleWordOrPdf(VM_BOOKS_DETAIL vM_BOOKS_det)
        {
            ServiceResult result = new ServiceResult(state: MessageResultState.SUCCESS);
            //var vM_BOOKS = new VM_BOOKS();
            //vM_BOOKS_det.BookModel = vM_BOOKS;

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

            ServiceResult isAktarma = await AlinanDosyayiBellekteAktarmaAsync(vM_BOOKS_det);

            if (isAktarma.State == MessageResultState.SUCCESS)
            {
                vM_BOOKS_det.BookModel.ID = 0;
                vM_BOOKS_det.isWordPDF = true;
                // result = await DosyadanKitapAktar(vM_BOOKS_det);
                try
                {
                    if (vM_BOOKS_det.pdfVeyaWord == 1)
                    {
                        // word
                        result = await WordDosyasindaAktarmaAsync(vM_BOOKS_det);
                    }
                    else
                    {
                        // pdf okuma işlemini getir.
                        result = await PdfDosyasindanOkumaAsync(vM_BOOKS_det);
                    }

                }
                catch (Exception ex)
                {
                    result.State = MessageResultState.ERROR;
                    Console.WriteLine($"Hata: {ex.Message}");
                }
                var bak = result;
            }
            else
            {
                //result.State = MessageResultState.ERROR;
                // result.Message = "Dosya ekleme  durumunda hata oluştu.";
                return Json(isAktarma);
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public JsonResult KitabiYayinaGonder(string kitapid)
        {
            var kId = StringCipher.Decrypt(kitapid.ToString());
            var KİTAPıD = Convert.ToInt64(kId);
            ServiceResult<Books> checkerBook = new BooksService().CheckBook(KİTAPıD);
            //checkerBook.State = MessageResultState.SUCCESS; //test
            return Json(checkerBook);
        }

        [HttpPost]
        [Route("KitabiSil")]
        [ValidateAntiForgeryToken]
        public JsonResult KitabiSil(string kitapid)
        {
            var kId = StringCipher.Decrypt(kitapid.ToString());
            var KİTAPıD = Convert.ToInt64(kId);
            ServiceResult<bool> result = new BooksService().DeleteBookById(KİTAPıD);
            return Json(result);
        }

        [HttpPost]
        [Route("KitapSayfaSil")]
        [ValidateAntiForgeryToken]
        public JsonResult KitapSayfaSil(long sayfaId)
        {  
             
             ServiceResult<bool> result = new BooksPagesService().DeleteBookPageById(sayfaId);
             return Json(result);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public string DosyaYukle(IFormFile dosya, VM_BOOKS input)
        {
            var ds = dosya;
            return "";
        }
 
        [HttpGet]
        public IActionResult Alert(VM_ALERT alert)
        {
            return PartialView(alert);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ServiceResult<Books> KitabiTamamla(VM_BOOKS_DETAIL input)
        {

            var getBook = new BooksService().GetBooks(input.BookModel?.ID ?? 0);
            var _book = ObjectMapper.Map(getBook, new Books());
            _book.TAMAMLANDIMI = input.isTamalama ?? false;

            ServiceResult<Books> updaterBook = new BooksService().UpdateBook(_book);

            return updaterBook;

        }
        public static string ReplaceImagesWithKeys(string html)
        {
            const string imageKey = "||img_BooksPage||";

            var imgRegex = new Regex(
                "<img[^>]*src=[\"']data:image/.*?;base64,.*?['\"][^>]*>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            bool hasImage = imgRegex.IsMatch(html);

            //   Base64 img varsa → onları key ile değiştir
            if (hasImage)
            {
                html = imgRegex.Replace(html, imageKey);
            }

            //   HER HALÜKARDA key yoksa ekle
            if (!html.Contains(imageKey))
            {
                html = html.TrimEnd() + Environment.NewLine + imageKey;
            }

            return html;
        }


        /// <summary>
        /// Kitap ve kitaba ait sayfalar bu metot ile eklenir.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ServiceResult<VM_KITAP_EKLE>> KitapEkle(VM_BOOKS_DETAIL input)
        {
            int sayfaId = 0;
            int sayfaCount = 0;
            ViewBag.KitapTamamlamaSayisi = KitapTamamlamaSayisi;

            ServiceResult<VM_KITAP_EKLE> result = new ServiceResult<VM_KITAP_EKLE>();

            VM_KITAP_EKLE vmKitap = new VM_KITAP_EKLE();


            if (input.BookModel?.ID != null && input.BookModel.ID != 0)
            {
                var _book = ObjectMapper.Map(input.BookModel, new Books());
                var bookPapers = new VM_BOOKS_PAGES()
                {
                    isWordPDF = input.isWordPDF ?? false,
                    BookId = input.BookModel.ID ?? 0,
                    PageWrite = ReplaceImagesWithKeys(input.BookPagesModel?.PageWrite),
                    // ID = input.BookPagesModel?.ID + 1  ?? 0,
                    PageFoto = input.BookPagesModel?.PageFoto ?? "PageFoto",
                    PageWriteBase64 = input.BookPagesModel?.PageWriteBase64 ?? "PageWriteBase64",
                    PageFotoDosyalar = input.BookPagesModel?.PageFotoDosyalar,
                    PageFotoList = input.BookPagesModel?.PageFotoList,

                };
                var bookPages = new BooksPagesService().PostSaveBooksPages(bookPapers);
                if (bookPages.Result != null && bookPages.Result.State == MessageResultState.SUCCESS)
                {
                    sayfaCount = new BooksPagesService().GetPagesByBooks(input.BookModel.ID ?? 0)!.Count();
                    sayfaId = (int)bookPages.Result.Result.ID;

                    result.State = MessageResultState.SUCCESS;

                }
                else
                {
                    //  new BooksService().DeleteBook(_book);
                    result.State = MessageResultState.ERROR;
                    result.Message = bookPages?.Result?.Message;
                }

                vmKitap.BookID = input.BookModel.ID ?? 0;
                vmKitap.BooksPageCount = sayfaCount + 1;
                vmKitap.BooksPageID = sayfaId == 0 ? 1 : sayfaId;
                input.IlgiiSayfaSayisi = sayfaCount + 1;

            }
            else
            {

                VM_BOOKS vM_BOOKS = input.BookModel;
                vM_BOOKS.YazarId = (int)Lid;
                vM_BOOKS.TAMAMLANDIMI = input.isPdfVeyaWordTamalama ?? false;
                //vM_BOOKS.BookModel = kitap;
                //input.BookPagesModel = new VM_BOOKS_PAGES();

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
                            BookId = book.Result.ID,
                            PageWrite = ReplaceImagesWithKeys(input.BookPagesModel?.PageWrite),
                            PageFoto = input.BookPagesModel?.PageFoto ?? "PageFoto",
                            PageWriteBase64 = input.BookPagesModel?.PageWriteBase64 ?? "PageWriteBase64",
                            PageFotoDosyalar = input.BookPagesModel?.PageFotoDosyalar,
                            PageFotoList = input.BookPagesModel?.PageFotoList,

                        };
                        var bookPages = new BooksPagesService().PostSaveBooksPages(bookPapers);
                        var bookPagesId = bookPages.Result != null ? bookPages.Result.Result.ID : 0;
                        if (bookPages.Result != null && bookPagesId != 0)
                        {
                            input.BookPagesModel.ID = bookPagesId;
                            sayfaId = (int)bookPagesId;
                            sayfaCount =
                            //(int)bookPages.Result.ID + 1;
                            new BooksPagesService().GetPagesByBooks(book.Result.ID)!.Where(p => p.BookId == book.Result.ID).Count();

                            result.State = MessageResultState.SUCCESS;
                        }
                        else
                        {
                            //new BooksService().DeleteBook(book.Result);

                            result.State = MessageResultState.ERROR;
                            result.Message = bookPages.Result.Message;
                        }

                        vmKitap.BookID = book.Result!.ID;
                        vmKitap.BooksPageID = sayfaId == 0 ? 1 : sayfaId;
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

            vmKitap.BookIdENC = StringCipher.Encrypt(vmKitap.BookID.ToString());
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
        // [Route("PostBooksPageUpdate")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostBooksPageUpdate(VM_BOOKS_PAGES sayfa)
        {
            if(sayfa.PageFoto == null || sayfa.PageFoto == "")
            {
                sayfa.PageFoto = "PageFoto";
            }
            ServiceResult sonuc = await new BooksPagesService().PostUpdateBooksPages(sayfa);
            return Json(sonuc);
        }

        public IActionResult KitapDetay(string kitap_id)
        {
            var kitapId = Convert.ToInt64(StringCipher.Decrypt(kitap_id));
            VM_BOOKS_DETAIL kitapDetay = new VM_BOOKS_DETAIL();

            ViewBag.KITAPENC = kitapId;
            var vmBook = new BooksService().GetBooks(kitapId);
            ViewBag.iSTATISTIK = vmBook?.iSTATISTIK;

            ViewBag.YayinAyar = new AyarlarService().GetYayinAyarlariByBookId(kitapId);

            kitapDetay.isTamalama = vmBook?.TAMAMLANDIMI;
            kitapDetay.BookModel = vmBook;

            var Bildirim = new BildirimlerService().GetBildirimlerByLoginId(Lid);
            ViewBag.LOGIN_USER_ID = Lid;
            ViewBag.isTakip = Bildirim?.IsTakip ?? false; //Birisi beni takip ettiğinde bana e-posta gönder
            ViewBag.IsKitapYayin = Bildirim?.IsKitapYayin ?? false;//Birisi kitap yayınladığında bana bildirim yolla
            ViewBag.IsKitapYorum = Bildirim?.IsKitapYorum ?? false;//Birisi kitabıma yorum yaptığında bana e-posta gönder

            return View(kitapDetay);
        }
        public IActionResult YayinEviKitaplar(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS_DETAIL kitapDetay = new VM_BOOKS_DETAIL();
            kitapDetay.BookModel = new VM_BOOKS();

            var getBookList = new BooksService().TumKitaplar(arama);
            kitapDetay.VMBooksList = getBookList!;

            kitapDetay.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            ViewBag.YayinAyar = new AyarlarService().GetYyainAyarlari();
            ViewBag.LOGIN_USER_ID = Lid;

            ViewBag.DevamEdenKitaplar = arama.DevamEdenKitaplar;
            ViewBag.FavoriKitaplar = arama.FavoriKitaplar;
            ViewBag.YayinlananKitaplar = arama.YayinlananKitaplar;
            ViewBag.BitenKitaplar = arama.BitenKitaplar;

            ViewBag.SliderdaGosterilecekKayit = SliderdaGosterilecekKayit;
            ViewBag.DefaultSliderdaGosterilecekKayit = DefaultSliderdaGosterilecekKayit;

            return View("TumKitaplar", kitapDetay);
        }
        public IActionResult TumKitaplar(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS_DETAIL kitapDetay = new VM_BOOKS_DETAIL();
            kitapDetay.BookModel = new VM_BOOKS();

            if (arama.FavoriYazarlar != null || arama.DevamEdenKitaplar != null ||
                arama.FavoriKitaplar != null || arama.YayinlananKitaplar != null || arama.BitenKitaplar != null)
            {
                arama.yazarIId = Lid;
            }

            var getBookList = new BooksService().TumKitaplar(arama);
            kitapDetay.VMBooksList = getBookList!;

            kitapDetay.sliderdaGosterilecekKayit = arama.listelenecek_kayit_sayisi;

            ViewBag.YayinAyar = new AyarlarService().GetYyainAyarlari();
            ViewBag.LOGIN_USER_ID = Lid;

            ViewBag.DevamEdenKitaplar = arama.DevamEdenKitaplar;
            ViewBag.FavoriKitaplar = arama.FavoriKitaplar;
            ViewBag.YayinlananKitaplar = arama.YayinlananKitaplar;
            ViewBag.BitenKitaplar = arama.BitenKitaplar;
            ViewBag.Tip = "tumkitaplar";
            ViewBag.SliderdaGosterilecekKayit = SliderdaGosterilecekKayit;
            ViewBag.DefaultSliderdaGosterilecekKayit = DefaultSliderdaGosterilecekKayit;

            return View(kitapDetay);
        }

        [HttpPost]
        [Route("AraButonFiltrelemeKitaplar")]
        [ValidateAntiForgeryToken]
        public IActionResult AraButonFiltrelemeKitaplar(VM_ARAMA_INPUT arama)
        {
            VM_BOOKS_DETAIL kitapDetay = new VM_BOOKS_DETAIL();
            //kitapDetay.isAnaSayfa = true;
            //kitapDetay.Start = 0;
            //kitapDetay.sliderdaGosterilecekKayit = SliderdaGosterilecekKayit;

            kitapDetay.BookModel = new VM_BOOKS();

            kitapDetay.sliderdaGosterilecekKayit =
                (arama.listelenecek_kayit_sayisi != 0 ? arama.listelenecek_kayit_sayisi : SliderdaGosterilecekKayit);

            kitapDetay.profilKitapTuru = arama.profilKitapTuru ?? "";
            kitapDetay.isAnaSayfa = false;
            kitapDetay.Tip = arama.profilKitapTuru + "-" + arama.Tip;

            arama.yazarIId = Lid;

            List<VM_BOOKS>? bookList = new BooksService().TumKitaplar(arama);
            //vM_BOOKS.Stars = new BooksService().GetMaxStarBooks();
            ViewBag.LOGIN_USER_ID = Lid;
             
            //ViewBag.YayinAyar = new AyarlarService().GetYyainAyarlari();
            kitapDetay.VMBooksList = bookList!;

            return View(kitapDetay);
        }

        [HttpPost]
        [Route("PostBooksStars")]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
                result.State = sonuc.State;
                result.Message = sonuc.Message;
                result.Result = sonuc.Result;
            }

            return Json(result);
        }

        [HttpPost]
        [Route("PostBooksComments")]
        [ValidateAntiForgeryToken]
        public JsonResult PostBooksComments(VM_BOOKS_COMMENT mesajlar)
        {

            ServiceResult result = new ServiceResult();

            mesajlar.LoginUserId = Convert.ToInt32(Lid);
            result = new BooksService().PostBooksComments(mesajlar);

            return Json(result);
        }

    }
}
