 $(document).ready(function () {
        // Desktop: hover ile aç/kapa
        $('.dropdown').hover(
            function () {
                if ($(window).width() >= 768) {
                    $(this).addClass('open');
                }
            },
            function () {
                if ($(window).width() >= 768) {
                    $(this).removeClass('open');
                }
            }
        );

        // Mobil: dropdown toggle
        $('.dropdown-toggle').click(function (e) {
            if ($(window).width() < 768) {
                e.preventDefault();
                var $parent = $(this).parent('.dropdown');
                $parent.toggleClass('open');
            }
        });

        // Hamburger toggle
        $('.navbar-toggle').click(function () {
            $('#main-navbar').slideToggle();
        });

        // Sayfa resize
        $(window).resize(function () {
            if ($(window).width() >= 768) {
                $('#main-navbar').show().css('max-height', '');
            } else {
                $('#main-navbar').hide().css('max-height', '80vh');
                $('.dropdown').removeClass('open');
            }
        });

        // Başlangıç kontrolü
        if ($(window).width() >= 768) {
            $('#main-navbar').show();
        } else {
            $('#main-navbar').hide().css('max-height', '80vh');
        }
    });
