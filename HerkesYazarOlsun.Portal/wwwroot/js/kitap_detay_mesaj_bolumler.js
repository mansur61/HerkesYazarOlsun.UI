(function ($) {
    $(function () {

        var $tabLinks = $('[data-toggle="tab"]');

        function activateTab(hash) {
            if (!hash) hash = '#aciklama';

            // target link
            var $link = $tabLinks.filter('[href="' + hash + '"]');
            if ($link.length === 0) return;

            // NAV: remove active from li and links, then set it on clicked link's li and the link
            $tabLinks.closest('li').removeClass('active');
            $tabLinks.removeClass('active');

            $link.closest('li').addClass('active');
            $link.addClass('active');

            // PANES: ensure only one active pane
            var $panes = $('.tab-content .tab-pane');
            $panes.removeClass('active show in'); // remove all possible "active" markers

            var $target = $(hash);
            if ($target.length === 0) return;

            $target.addClass('active');

            // Maintain consistency with either Bootstrap 3 (in) or 4/5 (show)
            // If any pane in markup uses 'show' keep using 'show', otherwise use 'in'
            if ($panes.filter('.show').length) {
                $target.addClass('show');
            } else {
                $target.addClass('in');
            }

            // trigger shown event for compatibility (some scripts may listen)
            try {
                $target.trigger($.Event('shown.bs.tab', { relatedTarget: $target.get(0) }));
            } catch (e) { /* ignore */ }
        }

        // Click handler (works regardless of Bootstrap JS)
        $tabLinks.off('click.manualTab').on('click.manualTab', function (e) {
            e.preventDefault();
            var href = $(this).attr('href') || $(this).data('target');
            activateTab(href);

            // update URL hash without scrolling
            if (history && history.replaceState) {
                history.replaceState(null, null, href);
            } else {
                location.hash = href;
            }
        });

        // Activate tab from URL hash on load (or default #aciklama)
        activateTab(location.hash || '#aciklama');

        // Optional: if other code changes hash, react to it
        $(window).on('hashchange', function () {
            activateTab(location.hash);
        });

    });
})(jQuery);