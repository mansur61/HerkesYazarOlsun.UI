function dosyaSecim(deger) {
    console.log("gelindi general.js : " +deger);
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
        console.log("  dosyaAdi : " + dosyaAdi);
        //console.log("  dosyaAdi2 : " + $("#ONKAPAKFOTO").val());
    }
    else if (deger == "arkaKapakFoto") {
        var dosya = document.getElementById('dosyaArkaKapakFoto').files[0];
        if ("undefined" != dosya) {
            var dosyaAdi = dosya.name;
            $("#ARKAKAPAKFOTO").val(dosyaAdi);
        }
        console.log("  dosyaAdi : " + dosyaAdi);
        
    }
    else if (deger == "profilFoto") {
        var dosya = document.getElementById('proFotoSec').files[0];
        var dosyaAdi = dosya.name;
        $("#profilFotoName").val(dosyaAdi);
    }

    else if (deger == "dosyaAktarmaDisVeri") {
        var dosya = document.getElementById('dosyaAktar').files[0];
        var dosyaAdi = dosya.name;
        $("#aktarilacakDosya").val(dosyaAdi);
    }
    else if (deger == "dosyaAktarmaDekont") {
        var dosya = document.getElementById('dekontAktar').files[0];
        var dosyaAdi = dosya.name;
        $("#aktarilacakDekont").val(dosyaAdi);
    }

}
//"#basarili-"
//"#bilgi-"
//"#uyari-"
/*function BilgiVer(state,basarili,bilgi,uyari,mesaj) {
    if (state == 1) {
        $(basarili + tip).css("display", "block");
        $(basarili + tip + " #mesaj").html(mesaj);

        $(bilgi + tip).css("display", "none");
        $(uyari + tip).css("display", "none");

        setTimeout(function () {
            var link = window.location.href;

            window.location.href = link;
            console.log("link " + link);

            window.location.reload()

        }, 3000);
    }
    else if (state == 3) {
        $(bilgi + tip).css("display", "block");
        $(bilgi + tip + " #mesaj").html(mesaj);

        $(basarili + tip).css("display", "none");
        $(uyari + tip).css("display", "none");
    }
    else {
        $(uyari + tip).css("display", "block");
        $(bilgi + tip).css("display", "none");
        $(basarili + tip).css("display", "none");
        $(uyari + tip + " #mesaj").html(mesaj);
    }
}*/

function isAlert(metinText) {
    Swal.fire({
        title: 'Emin misiniz?',
        text: metinText,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Evet',
        cancelButtonText: 'Hayır',
        customClass: {
            confirmButton: 'btn btn-primary',
            cancelButton: 'btn btn-danger'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            return true;
        } else if (result.dismiss === Swal.DismissReason.cancel) {
            return false;
        }
    });
}