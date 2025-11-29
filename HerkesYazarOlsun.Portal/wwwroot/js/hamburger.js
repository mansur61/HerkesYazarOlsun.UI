(function ($) {
    $(function () {
        // URL sonu # varsa temizle
        if (window.location.href.endsWith("#")) {
            history.replaceState(null, null, window.location.href.replace(/#$/, ""));
        }

        // Desktop hover
        $('.dropdown').on('mouseenter', function () {
            if (window.innerWidth >= 768) $(this).addClass('open');
        }).on('mouseleave', function () {
            if (window.innerWidth >= 768) $(this).removeClass('open');
        });

        // Mobile click dropdown
        $('.dropdown-toggle').on('click', function (e) {
            if (window.innerWidth < 768) {
                e.preventDefault();
                $(this).parent().toggleClass('open');
            }
        });

        // Hamburger toggle
        $('.navbar-toggle').on('click', function () {
            $('#main-navbar').slideToggle();
        });

        // Window resize
        $(window).on('resize', function () {
            if (window.innerWidth >= 768) {
                $('#main-navbar').show();
            } else {
                $('#main-navbar').hide();
                $('.dropdown').removeClass('open');
            }
        });
    });
})(jQuery);
