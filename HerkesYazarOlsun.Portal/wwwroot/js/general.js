

function dosyaSecim(deger) {
    if (deger == "sayfaFoto") {
        var dosya = document.getElementById('dosyaSayfaFoto').files[0];
        var dosyaAdi = dosya.name;
        $("#KITAPSAYFAFOTO").val(dosyaAdi);
        console.log(deger + " geldi....  , dosyaAdi : " + dosyaAdi);
    }
    else if (deger == "onKapakFoto") {
        var dosya = document.getElementById('dosyaOnKapakFoto').files[0];
        var dosyaAdi = dosya.name;
        $("#ONKAPAKFOTO").val(dosyaAdi);
    }
    else if (deger == "arkaKapakFoto") {
        var dosya = document.getElementById('dosyaArkaKapakFoto').files[0];
        var dosyaAdi = dosya.name;
        $("#ARKAKAPAKFOTO").val(dosyaAdi);
    }

}

