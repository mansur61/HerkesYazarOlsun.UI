using HerkesYazarOlsun.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HerkesYazarOlsun.Portal.Controllers;

[Route("YaziArkadasi")]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class YaziArkadasiController(WritingCompanionService service) : Controller
{
    private bool IsMember => User.Identity?.IsAuthenticated == true
        && !User.HasClaim("gozlemci_mod", "1") && !string.IsNullOrWhiteSpace(User.FindFirst("user_id")?.Value);

    [HttpGet("Durum")]
    public IActionResult Status() => Json(new { available = IsMember && service.IsConfigured, signedIn = IsMember });

    [HttpPost("Oneri")]
    [RequestSizeLimit(64000)]
    [EnableRateLimiting("writing-companion")]
    public async Task<IActionResult> Suggest([FromBody] WritingRequest? request, CancellationToken cancellationToken)
    {
        if (!IsMember) return Unauthorized(new { message = "Yapay zekâ desteği için hesabına giriş yapmalısın." });
        if (!ModelState.IsValid || !WritingCompanionService.IsValid(request))
            return BadRequest(new { message = "Metin boş veya çok uzun. Taslağını kısaltıp tekrar dene." });
        if (!service.IsConfigured) return StatusCode(503, new { message = "Yapay zekâ henüz etkin değil. Sesli veya elle yazmaya devam edebilirsin." });
        try { return Json(new { text = await service.GenerateAsync(request!, cancellationToken) }); }
        catch (OperationCanceledException) { return StatusCode(504, new { message = "Yanıt zamanında alınamadı. Taslağın korunuyor; tekrar deneyebilirsin." }); }
        catch (HttpRequestException exception) when (exception.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
        {
            return StatusCode(503, new { message = "Yapay zekâ anahtarı geçersiz, süresi dolmuş veya yetkisiz olabilir. Yönetici sunucu ayarlarını kontrol edene kadar mikrofonla ya da yazarak devam edebilirsin." });
        }
        catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            return StatusCode(503, new { message = "Yapay zekâ kullanım kotası dolmuş olabilir. Daha sonra tekrar dene; mikrofonla ya da yazarak devam edebilirsin." });
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidDataException or System.Text.Json.JsonException)
        {
            // Provider bodies can contain sensitive data; do not return or log them.
            return StatusCode(502, new { message = "Yapay zekâ şu anda yanıt veremiyor. Taslağın korunuyor; daha sonra tekrar dene. Mikrofonla veya yazarak devam edebilirsin." });
        }
    }
}
