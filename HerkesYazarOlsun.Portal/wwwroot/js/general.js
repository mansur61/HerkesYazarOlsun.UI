function dosyaSecim(deger) {
    console.log("gelindi general.js : " +deger);
    if (deger == "sayfaFoto") {
        var dosya = document.getElementById('dosyaSayfaFoto').files[0];
        if (dosya) {
            var dosyaAdi = dosya.name;
            $("#KITAPSAYFAFOTO").val(dosyaAdi);
            console.log(deger + " geldi....  , dosyaAdi : " + dosyaAdi);
        }
        
    }
    else if (deger == "onKapakFoto") {
        var dosya = document.getElementById('dosyaOnKapakFoto').files[0];
        if (dosya) {
            var dosyaAdi = dosya.name;
            $("#ONKAPAKFOTO").val(dosyaAdi);
            console.log("  dosyaAdi : " + dosyaAdi); 
        }
       
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
        if (dosya) {
            var dosyaAdi = dosya.name;
            $("#profilFotoName").val(dosyaAdi);
        }
        
    }

    else if (deger == "dosyaAktarmaDisVeri") {
        var dosya = document.getElementById('dosyaAktarmaDis').files[0];
        if (dosya) {
            var dosyaAdi = dosya.name;
            $("#aktarilacakDosya").val(dosyaAdi);
        }
        
    }
    else if (deger == "dosyaAktarmaDekont") {
        var dosya = document.getElementById('dekontAktar').files[0];
        if (dosya) {
            var dosyaAdi = dosya.name;
            $("#aktarilacakDekont").val(dosyaAdi);
        }
        
    }

}
//"#basarili-"
//"#bilgi-"
//"#uyari-"
/*function BilgiVer(state,basarili,bilgi,uyari,mesaj) {
    if (state == 1) {
        $(basarili  ).css("display", "block");
        $(basarili   + " #mesaj").html(mesaj);

        $(bilgi  ).css("display", "none");
        $(uyari  ).css("display", "none");

        setTimeout(function () {
            var link = window.location.href;

            window.location.href = link;
            console.log("link " + link);

            window.location.reload()

        }, 3000);
    }
    else if (state == 3) {
        $(bilgi  ).css("display", "block");
        $(bilgi   + " #mesaj").html(mesaj);

        $(basarili  ).css("display", "none");
        $(uyari  ).css("display", "none");
    }
    else {
        $(uyari  ).css("display", "block");
        $(bilgi  ).css("display", "none");
        $(basarili  ).css("display", "none");
        $(uyari   + " #mesaj").html(mesaj);
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

function UyariVer(state, basarili, bilgi, uyari, mesaj) {
    //console.log("geldi UyariVer ", state);
    if (state == 1) {
        $(basarili  ).css("display", "block");
        $(basarili   + " #mesaj").html(mesaj);

        $(bilgi  ).css("display", "none");
        $(uyari  ).css("display", "none");

        /*setTimeout(function () {
            var link = window.location.href;

            window.location.href = link;
            console.log("link " + link);

            window.location.reload()

        }, 3000);*/
    }
    else if (state == 3) {
        $(bilgi  ).css("display", "block");
        $(bilgi   + " #mesaj").html(mesaj);

        $(basarili  ).css("display", "none");
        $(uyari  ).css("display", "none");
    }
    else {
        $(uyari  ).css("display", "block");
        $(bilgi  ).css("display", "none");
        $(basarili  ).css("display", "none");
        $(uyari   + " #mesaj").html(mesaj);
    }
}
