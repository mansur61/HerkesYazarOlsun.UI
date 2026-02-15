# HerkesYazarOlsun.UI Proje Mimarisi

## Genel Bakış
HerkesYazarOlsun.UI, ASP.NET Core MVC tabanlı bir web uygulamasıdır. Bu proje, kullanıcı arayüzü (UI) katmanı olarak hizmet verir ve harici bir API'ye bağlanarak veri işlemlerini gerçekleştirir. Uygulama, kitap paylaşımı ve yazar platformu gibi özellikler sunmaktadır.

## Teknoloji Stack
- **Framework:** ASP.NET Core MVC (.NET 8.0)
- **Dil:** C#
- **UI Teknolojisi:** Razor Views (CSHTML)
- **HTTP İstemcisi:** HttpClient (API çağrıları için)
- **Serialization:** Newtonsoft.Json
- **Güvenlik:** Cookie Authentication, Session Management, CSRF Protection, CSP Headers
- **Konfigürasyon:** JSON dosyaları (appsettings.json)

## Mimari Katmanlar

### 1. Presentation Layer (Sunum Katmanı)
- **Controllers:** MVC Controller'ları, HTTP isteklerini işler ve View'lara veri gönderir.
  - Örnek: HomeController, AccountController, KitaplarController
- **Views:** Razor sayfaları, kullanıcı arayüzünü oluşturur.
  - Örnek: Views/Home/Index.cshtml, Views/Account/Giris.cshtml
- **Static Files:** wwwroot klasörü altında CSS, JS, resimler, vb.
  - Özel MIME tipleri için yapılandırma (mp3, ogg, wav)

### 2. Application/Service Layer (Uygulama/Hizmet Katmanı)
- **Services:** İş mantığını yönetir, API çağrıları yapar ve veriyi işler.
  - BaseService: Ortak API çağrısı mantığı (HttpClient kullanımı, header'lar)
  - Özel Servisler: BooksService, EmailService, KisiService, vb.
  - API Endpoint'leri: api/Books, api/Email, vb.
- **Helpers:** Yardımcı sınıflar ve uzantılar.
  - AppSettings: Konfigürasyon erişimi
  - GetHttpClientCustom: Özel HttpClient yapılandırması
  - Extensions: UserClaimProvider, EncryptionHelper, vb.

### 3. Infrastructure Layer (Altyapı Katmanı)
- **Middleware:** İstek işleme ara yazılımları.
  - RequestExceptionMiddleware: Exception handling (ValidationException, BadHttpRequestException)
- **Extensions:** Güvenlik ve diğer uzantılar.
  - SecurityExtensions: Authentication, Session, Cookie, Antiforgery ayarları
- **Konfigürasyon:** Program.cs'de DI (Dependency Injection) ve middleware kurulumu.

## Veri Akışı
1. Kullanıcı tarayıcıdan istek gönderir (ör. /Home/Index)
2. Controller (HomeController) isteği alır
3. Service (BooksService) harici API'ye HttpClient ile çağrı yapar
4. API'den JSON veri alınır ve deserialize edilir
5. Veri View'a gönderilir ve Razor ile render edilir
6. HTML yanıt kullanıcıya döner

## Güvenlik Özellikleri
- **Authentication:** Cookie tabanlı form authentication
- **Authorization:** Claims-based yetkilendirme
- **Session Management:** 30 dakikalık oturum süresi
- **CSRF Protection:** Antiforgery token'ları
- **CSP Headers:** Content Security Policy (Development/Production ortamlarına göre)
- **Secure Cookies:** HttpOnly, Secure, SameSite ayarları
- **HTTPS Redirection:** Production'da zorunlu

## Konfigürasyon
- **appsettings.json:** Ana konfigürasyon
- **appsettings.Development.json:** Geliştirme ortamı ayarları
- **appsettings.Production.json:** Üretim ortamı ayarları
- **HelperSettings:** Özel ayarlar (Slider kayıt sayısı, vb.)

## Harici Bağımlılıklar
- **API Backend:** AppSettings.ApiPath üzerinden erişilen harici API
- **Models:** HerkesYazarOlsun.Model kütüphanesinden ViewModel ve Entity sınıfları
- **Paketler:** Microsoft.AspNetCore.*, Newtonsoft.Json, vb.

## Build ve Deployment
- **Build:** dotnet build komutu
- **Run:** dotnet run veya Visual Studio
- **Static Files:** Development'da wwwroot, Production'da ek "Belgeler" klasörü
- **Environment:** Development/Production ayırımı

## Önerilen Geliştirme Pratikleri
- Controller'larda sadece UI mantığı, iş mantığı Services'te
- API çağrıları async/await ile
- Exception handling Middleware ile merkezi
- Güvenlik ayarları Extensions ile modüler
- Konfigürasyon appsettings ile environment-specific

Bu mimari, MVC pattern'ini takip eder ve Service Layer ile API entegrasyonu sağlar. UI ve backend ayrımı net bir şekilde yapılmıştır.