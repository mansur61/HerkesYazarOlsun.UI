# Makaleler: servis entegrasyonu

Portal artık yerel JSON/dosya deposu kullanmaz. `MakaleApiService`, mevcut API adresini ve JWT/refresh akışını kullanır. Word/PDF dosyası multipart olarak servise iletilir. Word metni serviste çıkarılır. PDF özgün haliyle saklanır ve PDF.js ile görüntülenir; taranmış/metinsiz belgeler de desteklenir.

## Veriler ve eşzamanlı kullanım

- `Makaleler`: yazar, başlık, metin, taslak/yayın tarihi.
- `MakaleBelgeler`: özgün Word/PDF içeriği; ayrı tablo, makaleye bire bir bağlı.
- Belge ve makale tek `SaveChangesAsync` işlemiyle atomik kaydedilir. Birden fazla portal/API kopyası aynı veritabanını kullanabilir.
- Liste API'si en fazla 50 özet döndürür (portal 12); metin ve belge liste sorgusunda alınmaz. Ana sayfa carousel'i son 12 makaleyi gösterir, Tüm makaleler bağlantısı sayfalı arşivi açar. Yazar, üst menüde Yayınlanan Kitaplarım yanındaki Makalelerim alanından taslaklarını yönetir; profilde makale yönetimi bulunmaz.
- Taslak erişimi ve yayınlama API'de doğrulanmış JWT `user_id` ile kontrol edilir. Yayınlama tekrarlandığında ilk yayın tarihi korunur.
- API süreci başına en fazla iki belge işlenir; yoğunlukta 429 dönülür. Dosya 20 MB, PDF 200 sayfa, açılmış Word içeriği 50 MB ile sınırlıdır.

Özgün belgeleri veritabanında tutmak disk paylaşımı gereksinimini kaldırır ve atomik kayıt sağlar; veritabanı/yedek boyutu dosyalarla büyür. Çok yüksek dosya hacminde belge tablosu nesne depolamasına taşınabilir.

## Dağıtım

Önce kullanılan veritabanına `AddMakaleler` migration'ı uygulanmalı, ardından servis ve portal birlikte yayınlanmalıdır. 28.09.2026 tarihinde kullanıcının talimatıyla canlı SQL Server `herkesya_` veritabanına `20260928070703_AddMakaleler` migration'ı başarıyla uygulandı. Test ve PostgreSQL veritabanlarına işlem yapılmadı. Servis/portal yayınlama bu işlem kapsamında yapılmadı.

SQL Server için servis repo kökünde:

```sh
dotnet ef database update 20260928070703_AddMakaleler --project HerkesYazarOlsun.DataLayer --context SqlServerContext
```

Bağlantı yalnızca servis projesindeki `HerkesYazarOlsun/appsettings.json`, ortama ait `appsettings.{Environment}.json` ve ortam değişkenlerinden alınır. DataLayer ayar dosyası kaldırılmıştır. EF komutu repo veya servis dizininden çalıştırılabilir; çalışma dizini bağlantı kaynağını değiştirmez.

PostgreSQL için `20260928070745_AddMakaleler` migration'ı eklendi. PostgreSQL'in önceki snapshot'ı mevcut modellerden eski olduğundan otomatik oluşan ilgisiz değişiklikler çıkarıldı; yalnızca iki makale tablosu ve indeksler eklenir. Diğer şema farkları bu iş kapsamında giderilmedi. PostgreSQL kullanılıyorsa mevcut şema/snapshot farkı ayrıca incelenerek migration SQL'i uygulanmalıdır; pending-model uyarısını genel olarak kapatmayın.

Önceki `App_Data/Makaleler` klasörü artık okunmaz. Çalışma alanında aktarılacak eski kayıt bulunmadı. Başka ortamda eski dosyalar varsa silinmeden saklanmalı ve geçişten önce ayrıca aktarılmalıdır. `Articles:StoragePath` ayarı artık kullanılmaz.

## Kontrol

```sh
dotnet build HerkesYazarOlsun.Portal --no-restore
# Servis repo kökünde:
dotnet build HerkesYazarOlsun/HerkesYazarOlsun.Service.csproj --no-restore
dotnet run --project tests/ArticleChecks
```

Kontroller Word/PDF metin çıkarma, bozuk/metinsiz dosya reddi, taslak liste erişimi, sayfalama sınırı, ilişkiler ve migration kapsamını doğrular. Gerçek veritabanında eşzamanlı yük testi ve tarayıcıdan uçtan uca test ayrıca yapılmalıdır.

## Okuyucu ve makale yönetimi

PDF: masaüstünde iki sayfa, mobilde tek sayfa; yakınlaştırma ve sayfa içi kaydırma. Tablo/dekont görünümü özgün belgeden çizilir, metne dönüştürülmez. Word içeriği sayfalara ayrılan metin olarak gösterilir; birebir düzen için PDF yüklenmelidir. PDF.js dosyaları `wwwroot/lib/pdfjs` altında yerel olarak bulunur.

`GET api/makaleler/{id}/belge` yayınlanan belgeyi veya sahibinin taslağını getirir. `DELETE api/makaleler/{id}` yalnızca sahibine açıktır; bağlı özgün belge de silinir. Portal silme işlemi antiforgery ve kullanıcı onayı kullanır. Bu değişiklikler için yeni veritabanı migration'ı gerekmez.

Kitap okuma ve güncelleme mobilde aynı okuyucuyu kullanır. `BookPagination` kapak/önsöz/kapanış/arka kapak dahil toplamı hesaplar; yazmaya devam et numarası bu toplam + 1 olarak sunucudan alınır. Örnek: 34 içerik sayfası → okuyucu toplamı 38 → yeni sayfa 39.

Tarayıcı doğrulaması: `python tests/browser_readers.py` (Python Playwright ve Chromium gerekir). Gerçek okuyucu dosyalarını kullanan izole test sunucusunda çift/tek sayfa, gezinme, yakınlaştırma ve mobil kitap içi kaydırma doğrulanır; canlı API'ye bağlanmaz.
