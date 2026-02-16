bu githuba eklenen private paketi siler local pc ye  dotnet nuget remove source github
bu private github paketi eklenmiş mi gösterir dotnet nuget list source  

dotnet nuget locals all --clear nuget cache temizlenir 

 
aşağıdaki kod terminalde çalıtırılır github private paketin localde kullanımı sağlanır

dotnet nuget add source \
  --username mansur61 \
  --password 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 \ 
  --store-password-in-clear-text \
  --name github \
  "https://nuget.pkg.github.com/mansur61/index.json"

  tek satır olarak
  dotnet nuget add source --username mansur61 --password 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 --store-password-in-clear-text --name github "https://nuget.pkg.github.com/mansur61/index.json"

YOUR_NEW_PAT = PERSONEL ACCESS TOKEN githubdan alınır Tokens (classic) bu token
 


