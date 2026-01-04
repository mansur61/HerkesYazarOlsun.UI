using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.Utils;
using HerkesYazarOlsun.Model.ViewModel;
using HerkesYazarOlsun.Portal.Helpers;
using Newtonsoft.Json; 

namespace HerkesYazarOlsun.Portal.Services
{

    public class AyarlarService : BaseService
    {
        public async Task<ServiceResult> SaveOrUpdateAyarlar(VM_AYARLAR ayarlar)
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = new Uri(AppSettings.ApiPath)
                };

                using var form = new MultipartFormDataContent();

                // VM_AYARLAR içindeki basit alanları ekle (dosyalar ve complex objeler hariç)
                foreach (var prop in typeof(VM_AYARLAR).GetProperties())
                {
                    if (prop.Name == "dosyalar" || prop.Name == "UserDetail" || prop.Name == "Profile" || prop.Name == "User" || prop.Name == "Bildirim")
                        continue;

                    var value = prop.GetValue(ayarlar);
                    if (value != null)
                        form.Add(new StringContent(value.ToString()!), prop.Name);
                }

                // Complex objeleri ekle
                void AddComplexObject<T>(T obj, string prefix)
                {
                    if (obj == null) return;

                    foreach (var prop in typeof(T).GetProperties())
                    {
                        var value = prop.GetValue(obj);
                        if (value != null)
                            form.Add(new StringContent(value.ToString()!), $"{prefix}.{prop.Name}");
                    }
                }

                AddComplexObject(ayarlar.UserDetail, nameof(ayarlar.UserDetail));
                AddComplexObject(ayarlar.Profile, nameof(ayarlar.Profile));
                AddComplexObject(ayarlar.User, nameof(ayarlar.User));
                AddComplexObject(ayarlar.Bildirim, nameof(ayarlar.Bildirim));

                // Dosyaları ekle
                if (ayarlar.dosyalar != null)
                {
                    foreach (var file in ayarlar.dosyalar)
                    {
                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                        form.Add(streamContent, "dosyalar", file.FileName);
                    }
                }

                // API'ye gönder
                var response = await client.PostAsync("api/Settings/SaveOrUpdateAyarlar", form); 

                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ServiceResult>(resultJson);
                 
                //if (result != null)
                //{
                //    result.State = MessageResultState.SUCCESS;
                //    result.Message = "Değişiklikler Kyadedildi.";
                //}
                //else
                //{
                //    // Deserialize null ise default bir hata State'i ver
                //    result = new ServiceResult
                //    {
                //        State = MessageResultState.ERROR,
                //        Message = "API yanıtı boş döndü."
                //    };
                //}

                return result;

            }
            catch (HttpRequestException ex)
            {
                return new ServiceResult { State = MessageResultState.ERROR, Message = $"HTTP Hatası: {ex.Message}" };
            }
            catch (JsonException ex)
            {
                return new ServiceResult { State = MessageResultState.ERROR, Message = $"JSON Hatası: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ServiceResult { State = MessageResultState.ERROR, Message = $"Beklenmeyen Hata: {ex.Message}" };
            }
        }

        public ServiceResult SaveOrUpdateAyarlar2(VM_AYARLAR ayarlar)
        {
            string stringData = JsonConvert.SerializeObject(ayarlar);
            Task<string> jsonContent = PostData("api/Settings/SaveOrUpdateAyarlar", stringData);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<ServiceResult>(jsonContent.Result);
            return result;
        }

        public List<YayinAyarlari>? GetYyainAyarlari()
        {
            Task<string> jsonContent = GetData("api/Settings/GetYyainAyarlari");
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<List<YayinAyarlari>>(jsonContent.Result);
            return result;
        }

        public YayinAyarlari? GetYayinAyarlariByBookId(long id)
        {
            Task<string> jsonContent = GetData("api/Settings/GetYayinAyarlariByBookId?id="+id);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<YayinAyarlari>(jsonContent.Result);
            return result;
        }

        

    }
}
