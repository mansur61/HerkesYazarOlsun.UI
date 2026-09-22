using HerkesYazarOlsun.Portal.Helpers;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using HerkesYazarOlsun.Portal.Helpers.Extensions;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace HerkesYazarOlsun.Portal.Services
{
    public class BaseService
    {

        protected string UriService { get; set; }
        protected readonly HttpContext? _httpContextAccessor;

        public BaseService()
        {
            UriService = AppSettings.ApiPath;
            _httpContextAccessor = HttpContextHelper.Current;
        }

        private async Task UpdateAuthenticationTokensAsync(string? accessToken, string? refreshToken)
        {
            var httpContext = _httpContextAccessor ?? HttpContextHelper.Current;
            if (httpContext == null || string.IsNullOrWhiteSpace(accessToken))
                return;

            var existingIdentity = httpContext.User.Identity as ClaimsIdentity;
            if (existingIdentity == null)
                return;

            var claims = existingIdentity.Claims
                .Where(c => c.Type != "jwt_token" && c.Type != "refresh_token")
                .ToList();

            claims.Add(new Claim("jwt_token", accessToken));
            if (!string.IsNullOrWhiteSpace(refreshToken))
                claims.Add(new Claim("refresh_token", refreshToken));

            var newPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                claims,
                existingIdentity.AuthenticationType ?? CookieAuthenticationDefaults.AuthenticationScheme,
                existingIdentity.NameClaimType,
                existingIdentity.RoleClaimType));

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                newPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(30),
                    AllowRefresh = true
                });
        }

        private async Task<(string? AccessToken, string? RefreshToken)> TryRefreshAccessTokenAsync()
        {
            var refreshToken = _httpContextAccessor?.User.GetRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return (null, null);

            using var client = new HttpClient();
            var payload = JsonConvert.SerializeObject(new { refreshToken });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{UriService.TrimEnd('/')}api/Users/RefreshToken", content);

            if (!response.IsSuccessStatusCode)
                return (null, null);

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResult = JsonConvert.DeserializeObject<dynamic>(responseBody);
            var accessToken = tokenResult?.accessToken ?? tokenResult?.token ?? "";
            var nextRefreshToken = tokenResult?.refreshToken ?? refreshToken;

            if (!string.IsNullOrWhiteSpace(accessToken))
                await UpdateAuthenticationTokensAsync(accessToken, nextRefreshToken);

            return (accessToken, nextRefreshToken);
        }

        private async Task<HttpResponseMessage> SendWithRefreshRetryAsync(Func<HttpClient, Task<HttpResponseMessage>> requestFactory, HttpClient client)
        {
            var response = await requestFactory(client);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshedToken = await TryRefreshAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(refreshedToken.AccessToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshedToken.AccessToken);
                    response = await requestFactory(client);
                }
            }

            return response;
        }

        protected async Task<string> GetData(string url, long? tckimlikno = null, string? eposta = null)
        {
            var client = new GetHttpClientCustom().GetHttpClient();
            tckimlikno ??= _httpContextAccessor?.User.GetTcKimlikNo();
            var mail = _httpContextAccessor?.User.GetEmail();
            var jwt  = _httpContextAccessor?.User.GetJwtToken();

            client.DefaultRequestHeaders.Add("tckimlikno", tckimlikno.ToString());

            if (string.IsNullOrEmpty(mail))
                mail = eposta;

            var ip = _httpContextAccessor?.User.GetIpAddress();
            client.DefaultRequestHeaders.Add("ip", ip);

            // JWT token varsa Bearer olarak gönder
            if (!string.IsNullOrEmpty(jwt))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", jwt);

            client.BaseAddress = new Uri(UriService);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpresponse = await SendWithRefreshRetryAsync(async httpClient =>
                await httpClient.GetAsync(url), client);

            string jsonContent = await httpresponse.Content.ReadAsStringAsync();

            if (httpresponse.StatusCode == System.Net.HttpStatusCode.NotFound) { jsonContent = ""; }

            return jsonContent;
        }

        protected async Task<string> DeleteData(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UriService);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Task<HttpResponseMessage> serviceCallResponse = client.DeleteAsync(url);
                Task.WaitAll(serviceCallResponse);

                string jsonContent = await serviceCallResponse.Result.Content.ReadAsStringAsync();
                return jsonContent;
            }
        }

        protected async Task<string> PostData(string url, string stringData, long? tckimlikno = null)
        {
            var client = new GetHttpClientCustom().GetHttpClient();
            tckimlikno ??= _httpContextAccessor?.User.GetTcKimlikNo();
            var mail = _httpContextAccessor?.User.GetEmail();
            var jwt  = _httpContextAccessor?.User.GetJwtToken();

            client.DefaultRequestHeaders.Add("tckimlikno", (tckimlikno != null) ? tckimlikno.ToString() : "0");
            client.DefaultRequestHeaders.Add("email", (mail != null) ? mail.ToString() : "");

            // JWT token varsa Bearer olarak gönder
            if (!string.IsNullOrEmpty(jwt))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", jwt);

            client.BaseAddress = new Uri(UriService);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.Timeout = TimeSpan.FromMinutes(10);

            HttpContent content = new StringContent(stringData);

            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            var contentData = new StringContent(stringData, System.Text.Encoding.UTF8, "application/json");

            var httpresponse = await SendWithRefreshRetryAsync(async httpClient =>
                await httpClient.PostAsync(url, contentData), client);

            var jsonContent = await httpresponse.Content.ReadAsStringAsync();

            if (httpresponse.StatusCode == System.Net.HttpStatusCode.NotFound) { jsonContent = ""; }

            return jsonContent;

            //HttpResponseMessage response = await client.PostAsync(url, content);

            //string responseContent = await response.Content.ReadAsStringAsync();


            //if (response.StatusCode == System.Net.HttpStatusCode.NotFound) { responseContent = ""; }

            //return responseContent;

        }

    }
}
