# Yazı Arkadaşı — kurulum ve kapsam

KitapOlustur (yeni kitap ve yazmaya devam et) ve KitapEkleWordOrPdf ön bilgileri ekranlarında ortak yardımcı. Mobil web ve masaüstü web desteklenir. Portalın yayın paketi yeterlidir; servis API'sinde veya veritabanında migration gerektirmez.

## API anahtarı gerektirmeyen özellikler

- Türkçe tarayıcı konuşma tanıma (SpeechRecognition / webkitSpeechRecognition), düzenlenebilir taslak, manuel yazma alternatifi.
- Tarayıcının seslendirmesiyle alan sorularını okuma; kitap adı, önsöz, arka kapak yazısı, kategori ve varsa kitap sayfası.
- Kullanıcı onayıyla Quill editörlerine güvenli düz metin aktarımı. Sayfaya varsayılan olarak ekleme; üzerine yazma ayrıca onaylanır.
- Önsöz 500, arka kapak 200, sayfa 1000 karakter sınırı; taşan metin kesilmez.
- Kayıt başına 90 saniye; kapatma, sekme değiştirme veya sayfadan ayrılmada dinleme durur.
- Taslak ve son konuşmalar sessionStorage'da hesap/ekran/kitap/sayfa kapsamında tutulur. Aynı sekmede yenilemeden sonra devam edilebilir. Cihazlar arasında veya sekme kapandıktan sonra kalıcı kayıt değildir. Yeni konuşma düğmesi yardımcı geçmişini siler. Kitap kaydı mevcut Kaydet akışındadır.

HTTPS ve tarayıcı/işletim sistemi mikrofon izni gerekir. Konuşma tanıma tüm tarayıcılarda çalışmaz; destek yoksa klavye ve cihaz klavyesinin dikte özelliği kullanılabilir. Bazı tarayıcılar sesi kendi sunucularına gönderir; çevrimdışı çalışma garantisi yoktur. Site ses kaydı yüklemez/saklamaz. Türkçe seslendirme cihazdaki seslere bağlıdır.

## İsteğe bağlı OpenAI desteği

Anahtar yalnızca Portal sunucusunda tanımlanır. Tarayıcıya veya kaynak ZIP'e gerçek anahtar koymayın. `appsettings.json` içinde `WritingCompanion` ayarları bulunur; `ApiKey` bilerek boş bırakılmıştır.

Canlı ortamda gerçek anahtarı Plesk uygulama ortam değişkeni olarak `OPENAI_API_KEY` veya `WritingCompanion__ApiKey` adıyla tanımlayın ve uygulamayı yeniden başlatın. Alternatif olarak sunucuya özel `appsettings.Production.json` içinde `WritingCompanion:ApiKey` ayarlanabilir; bu dosyayı kaynak kontrolüne veya dağıtım ZIP'ine anahtarla eklemeyin. `WritingCompanion:ApiKey` boşsa `OPENAI_API_KEY` kullanılır.

```json
"WritingCompanion": {
  "Enabled": true,
  "ApiKey": "",
  "Model": "gpt-4.1-mini",
  "DailyRequestLimit": 60
}
```

Diğer ortam değişkenleri: `WritingCompanion__Enabled`, `WritingCompanion__Model`, `WritingCompanion__DailyRequestLimit`. Ayarlar eksikse mikrofon ve elle yazma çalışır; AI düğmeleri anahtar tanımlanana kadar devre dışıdır.

Anahtar yoksa uygulama açılır, mikrofon ve elle yazma çalışır; AI düğmeleri devre dışıdır. AI için gerçek kullanıcı hesabıyla giriş gerekir, gözlemci hesabı kullanamaz. OpenAI hesabında API erişimi ve kullanılabilir bakiye gerekir.

AI düğmeleri: Bana soru sor, Taslağa dönüştür, Daha sade yaz, Yazımı düzelt. Kullanıcının taslağı, formdaki metinler/kategori seçenekleri ve son konuşma turları gönderilir. Word/PDF'nin tamamı ve önceden kaydedilmiş kitabın tüm bölümleri otomatik okunmaz. Ayrı öneri önizlemesi kullanıcı seçmedikçe mevcut taslağı ve formu değiştirmez.

Model yanıtları Responses API üzerinden store:false ile alınır. Bu parametre sağlayıcının tüm veri saklama politikalarının kapatıldığı anlamına gelmez. Giriş doğrulaması, CSRF, istek/çıktı boyutları ve timeout uygulanır; sağlayıcı hataları veya anahtarı kullanıcıya döndürülmez. Günlük istek sınırı kullanıcı başına uygulama işlemi belleğindedir; IIS yeniden başlarsa sıfırlanır, çoklu worker arasında paylaşılmaz. Sağlayıcı hesabında ayrıca bütçe sınırı belirlenebilir.

## Testler

- `dotnet run --project tests/WritingCompanionChecks`: sağlayıcı taklidiyle istek doğrulama, yetkilendirme, boyutlar, veri saklama parametresi ve hata sonuçları.
- `python tests/browser_writing_companion.py`: Python Playwright + Chromium; `/private/tmp/hyo-quill.js` dosyasında uygulamada kullanılan Quill 1.3.6 gerekir. Gerçek Quill ile mobil/masaüstü form aktarımı, XSS metni, sınırlar, mikrofon olayları/izin hataları ve AI öneri onayı/hata senaryoları. Mikrofon/AI sağlayıcısı taklit edilir; gerçek cihaz ve API anahtarıyla canlı kabul testi ayrıca yapılmalıdır.

Resmî kaynaklar: https://developer.mozilla.org/en-US/docs/Web/API/SpeechRecognition , https://developers.openai.com/api/docs/guides/text
