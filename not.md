# NuGet GitHub Packages Kimlik Bilgisi

GitHub Packages'tan (`HerkesYazarOlsun.Model` vb.) paket çekmek için `nuget.config` dosyasına aşağıdaki yapıyı ekle.  
Token'ı GitHub → Settings → Developer settings → Personal access tokens → **`read:packages`** scope ile oluştur.

> ⚠️ `nuget.config` dosyasını asla commit etme. `.gitignore`'a ekli olduğundan emin ol.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/mansur61/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="mansur61" />
      <add key="ClearTextPassword" value="ghp_SENIN_TOKEN_BURAYA" />
    </github>
  </packageSourceCredentials>
</configuration>
```

## Alternatif: ProjectReference (token gerekmez)

Lokal geliştirmede NuGet yerine doğrudan proje referansı kullanılabilir:

```xml
<ItemGroup>
  <ProjectReference Include="../../HerkesYazarOlsun.Servis/HerkesYazarOlsun.Model/HerkesYazarOlsun.Model.csproj" />
</ItemGroup>
```
