using System.Net;
using System.Net.Http.Headers;
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
namespace HerkesYazarOlsun.Portal.Services;
public class MakaleApiService(IHttpClientFactory factory) : BaseService
{
    private async Task<HttpResponseMessage> Send(Func<HttpRequestMessage> create)
    {
        using var client = factory.CreateClient();
        client.BaseAddress = new Uri(UriService.TrimEnd('/') + "/");
        var token = _httpContextAccessor?.User.GetJwtToken();
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await SendWithRefreshRetryAsync(async c => { using var request = create(); return await c.SendAsync(request); }, client);
    }
    public async Task<MakaleSayfa> List(long? author = null, bool drafts = false, int page = 1)
    {
        using var response = await Send(() => new(HttpMethod.Get, $"api/makaleler?author={author}&drafts={drafts}&page={page}"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MakaleSayfa>())!;
    }
    public async Task<Makale?> Get(Guid id)
    {
        using var response = await Send(() => new(HttpMethod.Get, $"api/makaleler/{id}"));
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Makale>();
    }
    public async Task<Guid> Create(string title, IFormFile file)
    {
        using var response = await Send(() => {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(title), "baslik");
            content.Add(new StreamContent(file.OpenReadStream()), "dosya", Path.GetFileName(file.FileName));
            return new HttpRequestMessage(HttpMethod.Post, "api/makaleler") { Content = content };
        });
        if (response.StatusCode == HttpStatusCode.BadRequest) throw new InvalidDataException("Dosya okunamadı. Geçerli, şifresiz .docx veya PDF yükleyin (en fazla 20 MB ve 200 PDF sayfası).");
        if ((int)response.StatusCode == 429) throw new InvalidDataException("Yükleme işlemleri şu an yoğun. Lütfen biraz sonra tekrar deneyin.");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }
    public async Task<(byte[] Bytes, string ContentType)?> Document(Guid id)
    {
        using var response = await Send(() => new(HttpMethod.Get, $"api/makaleler/{id}/belge"));
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsByteArrayAsync(), response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream");
    }
    public async Task Delete(Guid id)
    {
        using var response = await Send(() => new(HttpMethod.Post, $"api/makaleler/{id}/sil"));
        response.EnsureSuccessStatusCode();
    }
    public async Task Publish(Guid id)
    {
        using var response = await Send(() => new(HttpMethod.Post, $"api/makaleler/{id}/yayinla"));
        response.EnsureSuccessStatusCode();
    }
}
