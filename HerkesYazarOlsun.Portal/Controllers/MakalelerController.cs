using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace HerkesYazarOlsun.Portal.Controllers;

public class MakalelerController(MakaleStore store, ILogger<MakalelerController> logger) : Controller
{
    [Authorize]
    public IActionResult Ekle() => View();

    [Authorize, HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(22 * 1024 * 1024)]
    public async Task<IActionResult> Ekle(string baslik, IFormFile? dosya)
    {
        var author = User.GetLoginUserId();
        if (author <= 0 || User.GetGozlemciMod() != "0") return Forbid();
        ViewBag.Baslik = baslik;
        var extension = Path.GetExtension(dosya?.FileName ?? "").ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(baslik) || baslik.Length > 200 || dosya == null || dosya.Length == 0 ||
            dosya.Length > 20 * 1024 * 1024 || (extension != ".docx" && extension != ".pdf"))
        {
            ModelState.AddModelError("", "Başlık (en fazla 200 karakter) ve en fazla 20 MB Word (.docx) veya PDF dosyası seçin.");
            return View();
        }
        using var stream = new MemoryStream();
        await dosya.CopyToAsync(stream);
        stream.Position = 0;
        string text;
        try
        {
            if (extension == ".docx")
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, true))
                    if (zip.Entries.Sum(e => e.Length) > 50 * 1024 * 1024)
                        throw new InvalidDataException("Expanded document is too large.");
                stream.Position = 0;
                using var document = WordprocessingDocument.Open(stream, false);
                text = string.Join("\n\n", document.MainDocumentPart!.Document.Body!
                    .Descendants<Paragraph>().Select(p => p.InnerText));
            }
            else
            {
                using var reader = new PdfReader(stream);
                reader.SetCloseStream(false);
                using var document = new PdfDocument(reader);
                if (document.GetNumberOfPages() > 200) throw new InvalidDataException("Too many pages.");
                text = string.Join("\n\n", Enumerable.Range(1, document.GetNumberOfPages())
                    .Select(i => PdfTextExtractor.GetTextFromPage(document.GetPage(i))));
            }
            if (string.IsNullOrWhiteSpace(text) || text.Length > 1000000)
                throw new InvalidDataException("No readable text or text too long.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Article document could not be read");
            ModelState.AddModelError("", "Dosya okunamadı. Metin içeren, şifresiz bir Word (.docx) veya PDF yükleyin (en fazla 200 PDF sayfası). Taranmış PDF için metin tanıma gerekir.");
            return View();
        }
        var article = new Makale { YazarId = author, Yazar = User.GetUserName(), Baslik = baslik.Trim(), Metin = text, Uzanti = extension };
        await System.IO.File.WriteAllBytesAsync(store.DocumentPath(article), stream.ToArray());
        store.Save(article);
        return RedirectToAction(nameof(Oku), new { id = article.Id });
    }

    public IActionResult Oku(Guid id)
    {
        var article = store.Get(id);
        if (article == null || (!article.YayinTarihi.HasValue &&
            (User.Identity?.IsAuthenticated != true || User.GetLoginUserId() != article.YazarId))) return NotFound();
        ViewBag.IsOwner = User.Identity?.IsAuthenticated == true && User.GetLoginUserId() == article.YazarId;
        return View(article);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public IActionResult Yayinla(Guid id)
    {
        var article = store.Get(id);
        if (article == null) return NotFound();
        if (article.YazarId != User.GetLoginUserId() || User.GetGozlemciMod() != "0") return Forbid();
        article.YayinTarihi ??= DateTimeOffset.UtcNow;
        store.Save(article);
        return RedirectToAction(nameof(Oku), new { id });
    }
}
