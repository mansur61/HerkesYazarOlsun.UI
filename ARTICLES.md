# Makaleler

Yazar kendi profilindeki Makale Ekle bağlantısıyla en fazla 20 MB .docx veya PDF yükler. Dosya metni taslak olarak açılır. Yayınla işleminden sonra makale Home/Index carousel'inde ve yazar profilinde görünür. Taslak okuma ve yayınlama sahiplik kontrolüyle korunur; POST işlemleri antiforgery doğrulaması kullanır.

Özgün belgeler ve JSON kayıtları web kökü dışında `App_Data/Makaleler` altında saklanır. Üretimde `Articles:StoragePath` (ortam değişkeni: `Articles__StoragePath`) kalıcı, uygulamanın yazabildiği bir dizine ayarlanmalı ve bu dizin yedeklenmelidir. Deploy sırasında silinmemelidir. Bu depolama tek portal süreci içindir; birden fazla replica için ortak veritabanı/depolama entegrasyonu gerekir.

Okuma görünümü belgenin metnini gösterir; görseller ve Word/PDF sayfa düzeni aktarılmaz. Eski .doc formatı .docx olarak kaydedilmelidir. Taranmış, metinsiz veya şifreli PDF'ler kabul edilmez.

Kontrol: `dotnet build HerkesYazarOlsun.Portal --no-restore` ve `node --test tests/mobile-reader.test.cjs`.
