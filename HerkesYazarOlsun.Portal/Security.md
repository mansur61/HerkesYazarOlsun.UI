

/*** Cookie ayarları 
 * SameSite=Strict → Cookie hiçbir cross-site istekte gönderilmez (en katı). Kullanıcı başka siteden geldiğinde oturum cookie gönderilmez.
 * 
 * SameSite=Lax → Güvenli GET navigasyonlarında cookie gönderilebilir (ör. linke tıklama). 
 * POST gibi cross-site state‑değiştiren isteklerde gönderilmez.
 * 
 * SameSite=None; Secure → Cookie cross-site isteklerde de gönderilir (üçüncü taraf), ama Secure olmalı (HTTPS).
 * 
 * ***/

 
// Güvenli header'lar (CSP, X-Frame-Options, nosniff, vs.)
/**** 
 * default-src 'self':
Tüm kaynaklar (resim, CSS, JS vb.) sadece kendi domain’inden (aynı origin) yüklenebilir.
Yani başka bir siteden script, iframe, resim çekemezsin.

* script-src 'self':
JavaScript dosyaları sadece kendi domain’inden yüklenebilir.
CDN veya üçüncü parti script (ör. Google Analytics, Bootstrap CDN) engellenir.

*object-src 'none':
<object>, <embed>, <applet> gibi eski HTML etiketlerinden hiçbirine izin verilmez.
Bunlar genelde zararlı içerik yüklemek için kullanılır.
 * 
 * 
 * ***/