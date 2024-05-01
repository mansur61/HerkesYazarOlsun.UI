

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