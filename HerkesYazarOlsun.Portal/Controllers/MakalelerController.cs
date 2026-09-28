using HerkesYazarOlsun.Portal.Helpers.Extensions;
using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HerkesYazarOlsun.Portal.Controllers;
public class MakalelerController(MakaleApiService api, ILogger<MakalelerController> logger) : Controller
{
    public async Task<IActionResult> Index(long? author, int page = 1, bool drafts = false)
    {
        if (page < 1 || page > 100000) return BadRequest();
        if (drafts && (User.Identity?.IsAuthenticated != true || author != User.GetLoginUserId())) return Forbid();
        ViewBag.Drafts = drafts;
        ViewBag.Author = author;
        return View(await api.List(author, drafts, page));
    }
    [Authorize]
    public IActionResult Benim() => RedirectToAction(nameof(Index), new { author = User.GetLoginUserId(), drafts = true });
    public async Task<IActionResult> Belge(Guid id)
    {
        var document = await api.Document(id);
        if (document == null) return NotFound();
        Response.Headers.CacheControl = "private, no-store";
        return File(document.Value.Bytes, document.Value.ContentType, enableRangeProcessing: true);
    }
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(Guid id)
    {
        if (User.GetGozlemciMod() != "0") return Forbid();
        try { await api.Delete(id); }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Makale silinemedi. Makale kimliği: {ArticleId}, HTTP durumu: {StatusCode}", id, ex.StatusCode);
            TempData["MakaleHata"] = ex.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Oturumunuz sona ermiş. Yeniden giriş yapıp tekrar deneyin.",
                System.Net.HttpStatusCode.Forbidden => "Bu makaleyi silme yetkiniz yok. Makalenin yazarı olan hesapla giriş yapın.",
                System.Net.HttpStatusCode.NotFound => "Makale bulunamadı; listeyi yenileyin.",
                _ => $"Makale silinemedi. Servis yanıtı: HTTP {(int?)ex.StatusCode ?? 0}. Lütfen tekrar deneyin."
            };
            return RedirectToAction(nameof(Benim));
        }
        TempData["MakaleBilgi"] = "Makale silindi.";
        return RedirectToAction(nameof(Benim));
    }
    [Authorize]
    public IActionResult Ekle() => User.GetGozlemciMod() == "0" ? View() : Forbid();
    [Authorize, HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(22 * 1024 * 1024)]
    public async Task<IActionResult> Ekle(string baslik, IFormFile? dosya)
    {
        if (User.GetGozlemciMod() != "0") return Forbid();
        ViewBag.Baslik = baslik;
        if (string.IsNullOrWhiteSpace(baslik) || baslik.Length > 200 || dosya == null || dosya.Length == 0 || dosya.Length > 20 * 1024 * 1024)
        { ModelState.AddModelError("", "Başlık ve en fazla 20 MB Word/PDF dosyası seçin."); return View(); }
        try { return RedirectToAction(nameof(Oku), new { id = await api.Create(baslik, dosya) }); }
        catch (InvalidDataException ex) { ModelState.AddModelError("", ex.Message); }
        catch (HttpRequestException) { ModelState.AddModelError("", "Makale servisine ulaşılamadı veya oturumunuz sona erdi. Tekrar giriş yaparak deneyin."); }
        return View();
    }
    public async Task<IActionResult> Oku(Guid id)
    {
        var article = await api.Get(id);
        if (article == null) return NotFound();
        ViewBag.IsOwner = User.Identity?.IsAuthenticated == true && User.GetLoginUserId() == article.YazarId;
        return View(article);
    }
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Yayinla(Guid id)
    {
        if (User.GetGozlemciMod() != "0") return Forbid();
        try { await api.Publish(id); }
        catch (HttpRequestException) { TempData["MakaleHata"] = "Makale yayınlanamadı. Oturumunuzu kontrol edip tekrar deneyin."; }
        return RedirectToAction(nameof(Oku), new { id });
    }
}
