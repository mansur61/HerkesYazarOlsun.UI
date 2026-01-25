$(document).ready(function () {

    // HASH temizleme
    if (window.location.hash) {
        history.replaceState(null, null, window.location.pathname);
    }

    $('.nav-tabs').on('click', '.nav-link', function (e) {
        e.preventDefault();

        // UI
        $('.nav-tabs .nav-link').removeClass('active');
        $('.tab-pane').removeClass('active show');

        $(this).addClass('active');
        const target = $(this).attr('href');
        $(target).addClass('active show');

        // 🔥 FONKSİYON ÇAĞIR
        const fnName = $(this).data('fn');
        if (fnName && typeof window[fnName] === "function") {
            window[fnName]();
        }
    });

});



// ================= SPINNER =================
function showSpinner() {
    $("#globalSpinner").show();
}

function hideSpinner() {
    $("#globalSpinner").hide();
}


// ================= GLOBAL =================
let yildizPuani = 0;
let yazarId = appConfig.yazarId;
let profil = appConfig.profil;


// ================= BİLGİLENDİRME =================
function Bilgilendirme(follow) {

    let message = follow === 1 ? " Takip Etti" : " Takipten Çıkardı";
    let yorum = "(" + $("#kullaniciAdi").text() + ") adlı kişi sizi " + message;

    ajaxPost(appConfig.urls.mailNotify, {
        konu: "Herkes Yazar Olsun'dan Bildirim",
        kime: appConfig.email,
        tip: 0,
        mesaj: yorum
    }, handleResponse);
}

function ajaxPost(url, data, successFn, errorFn) {
    $.ajax({
        url: url,
        type: "POST",
        data: data,
        success: successFn,
        error: errorFn || function () {
            console.log("Ajax hata aldı:", url);
        }
    });
}


// ================= YILDIZ =================
function YildizVer() {

    if (yildizPuani === 0) {
        showUyari("Yıldız veriniz.");
        return;
    }

    ajaxPost(appConfig.urls.writerStars, {
        id: yazarId,
        puan: yildizPuani
    }, function (data) {
        handleResponse(data);
        if (data.state === 1) {
            setTimeout(() => location.reload(), 3000);
        }
    });
}


// ================= TAKİP =================
function YazarTakip(follow) {

    let kural = $('#kural').text().trim();

    ajaxPost(appConfig.urls.writerFollow, {
        id: yazarId,
        follow: follow
    }, function (data) {
        handleResponse(data);

        if (data.state === 1) {
            if (kural === "True") {
                Bilgilendirme(follow);
            }
            setTimeout(() => location.reload(), 3000);
        }
    });
}


// ================= UI MESSAGE =================
function handleResponse(data) {

    $(".alert").hide();

    if (data.state === 1) {
        showBasarili(data.message);
    }
    else if (data.state === 3) {
        showBilgi(data.message);
    }
    else {
        showUyari(data.message);
    }
}

function showBasarili(msg) {
    $("#basarili-" + profil).show().find("#mesaj").html(msg);
}

function showBilgi(msg) {
    $("#bilgi-" + profil).show().find("#mesaj").html(msg);
}

function showUyari(msg) {
    $("#uyari-" + profil).show().find("#mesaj").html(msg);
}


// ================= YILDIZ UI =================
let yildizlar = document.getElementsByClassName("yildizver");

function YildizIptalEt() {
    yildizPuani = 0;
    resetStars();
}

function YildizVerChange(adet) {
    yildizPuani = adet;
    resetStars();
    for (let i = 0; i < adet; i++) {
        $(yildizlar[i]).css("color", "dodgerblue");
    }
}

function resetStars() {
    for (let i = 0; i < yildizlar.length; i++) {
        $(yildizlar[i]).css("color", "black");
    }
}

for (let i = 0; i < yildizlar.length; i++) {
    yildizlar[i].addEventListener("click", function () {
        YildizVerChange(parseInt(this.dataset.adet));
    });
}


// ================= KİTAPLAR =================
function loadKitaplar(postData, targetDivId) {
alert("loadKitaplar ");
    if ($("#" + targetDivId).children().length > 0) return;
    alert("loadKitaplar 2");
    showSpinner();

    ajaxPost(appConfig.urls.kitapFiltre, postData,
        function (data) {
            $("#" + targetDivId).html(data);
        },
        function () {
            console.log("Kitaplar yüklenemedi");
        }
    );

    hideSpinner();
}

function DevamEdenKitaplar() {
    loadKitaplar({
        yazarIId: yazarId,
        DevamEdenKitaplar: true,
        profilKitapTuru: "devameden",
        tip: profil
    }, "devameden");
}

function TamamlananKitaplar() {
    loadKitaplar({
        yazarIId: yazarId,
        BitenKitaplar: true,
        profilKitapTuru: "tamamlanan",
        tip: profil
    }, "tamamlanan");
}

function YayinlananKitaplar() {
    loadKitaplar({
        yazarIId: yazarId,
        YayinlananKitaplar: true,
        profilKitapTuru: "yayin",
        tip: profil
    }, "yayin");
}
