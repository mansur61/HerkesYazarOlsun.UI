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

function escapeRegExp(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}
function quillBase64Sil(html) {
    return html.replace(/<img[^>]*src=["']data:image[^"']*["'][^>]*>/gi, "");
}
function showConfirm(message, callback) {
    const overlay = document.getElementById("confirmOverlay");
    const msg = document.getElementById("confirmMessage");
    const okBtn = document.getElementById("confirmOkBtn");
    const cancelBtn = document.getElementById("confirmCancelBtn");

    msg.innerHTML = message;
    overlay.style.display = "block";

    okBtn.onclick = () => {
        overlay.style.display = "none";
        callback(true);
    };

    cancelBtn.onclick = () => {
        overlay.style.display = "none";
        callback(false);
    };
}
function extractBase64Images(html) {
    console.log("extractBase64Images");
    const imgRegex = /<img[^>]+src="([^">]+)"/g;
    let match;
    let images = [];

    while ((match = imgRegex.exec(html)) !== null) {
        const src = match[1];
        if (src.startsWith("data:image")) {
            images.push(src);
        }
    }

    return images;
}
//Base64 → File (Blob) dönüştür
function base64ToFile(base64, filename) {
    const arr = base64.split(',');
    const mime = arr[0].match(/:(.*?);/)[1];
    const bstr = atob(arr[1]);
    let n = bstr.length;
    const u8arr = new Uint8Array(n);

    while (n--) {
        u8arr[n] = bstr.charCodeAt(n);
    }

    return new File([u8arr], filename, { type: mime });
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

function BilgiVer(state, basarili, bilgi, uyari, mesaj) {
    if (state == 1) {
        $(basarili + tip).css("display", "block");
        $(basarili + tip + " #mesaj").html(mesaj);

        $(bilgi + tip).css("display", "none");
        $(uyari + tip).css("display", "none");

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
}
