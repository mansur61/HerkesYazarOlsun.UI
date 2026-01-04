using HerkesYazarOlsun.Portal.Helpers;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using HerkesYazarOlsun.Portal.Helpers.Extensions;

namespace HerkesYazarOlsun.Portal.Services
{
    public class BaseService
    {

        protected string UriService { get; set; }
        protected readonly HttpContext? _httpContextAccessor;

        public BaseService()
        {
            UriService = AppSettings.ApiPath;
            _httpContextAccessor = new HttpContextAccessor().HttpContext;
                //HttpContextHelper.Current;
        }

        protected async Task<string> GetData(string url, long? tckimlikno = null,string? eposta=null)
        {
            var client = new GetHttpClientCustom().GetHttpClient();
            tckimlikno ??= _httpContextAccessor?.User.GetTcKimlikNo();
            var mail = _httpContextAccessor?.User.GetEmail();
            //tckimlikno = tckimlikno.HasValue ? tckimlikno : _httpContextAccessor.User.GetTcKimlikNo();
            //var birim_id = _httpContextAccessor.User.GetUserInfoByKey("birim_id");
            //if (birim_id != 0)
            //{
            //    client.DefaultRequestHeaders.Add("birim_id", birim_id.ToString());
            //}
            client.DefaultRequestHeaders.Add("tckimlikno", tckimlikno.ToString());
            
            if (string.IsNullOrEmpty(mail))
            {
                mail = eposta;
            }

            var ip = _httpContextAccessor?.User.GetIpAddress();
            client.DefaultRequestHeaders.Add("ip", ip);

            client.BaseAddress = new Uri(UriService);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Task<HttpResponseMessage> serviceCallResponse = client.GetAsync(url);

            Task.WaitAll(serviceCallResponse);
            var httpresponse = serviceCallResponse.Result;

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
            //tckimlikno = tckimlikno.HasValue ? tckimlikno : _httpContextAccessor.User.GetTcKimlikNo();
            //var birim_id = _httpContextAccessor.User.GetUserInfoByKey("birim_id");
            //if (birim_id != 0)
            //{
            //    client.DefaultRequestHeaders.Add("birim_id", birim_id.ToString());
            //}
            client.DefaultRequestHeaders.Add("tckimlikno", (tckimlikno != null) ? tckimlikno.ToString() : "0" );
            client.DefaultRequestHeaders.Add("email", (mail != null )? mail.ToString() : "");

            client.BaseAddress = new Uri(UriService);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.Timeout = TimeSpan.FromMinutes(10);

            HttpContent content = new StringContent(stringData);

            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            var contentData = new StringContent(stringData, System.Text.Encoding.UTF8, "application/json");


            Task<HttpResponseMessage> serviceCallResponse = client.PostAsync(url, contentData);
            Task.WaitAll(serviceCallResponse);

            var httpresponse = serviceCallResponse.Result;

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
