function initDesktopReaderLayout(book) {
    if (!book || window.matchMedia('(max-width: 768px)').matches) return;

    document.documentElement.classList.add('desktop-reading');
    var main = document.getElementById('main');
    var toolbar = document.querySelector('.book-reader-toolbar');
    var frame;

    function resize() {
        var width = main.clientWidth;
        // Keep a usable page height on short windows; the document can scroll.
        var top = main.getBoundingClientRect().top + window.scrollY;
        var height = Math.max(500, window.innerHeight - top - 24);
        if (width > 0) {
            book.setDimensions(width, height);
            // WowBook also uses these dimensions when zooming.
            book._originalWidth = width;
            book._originalHeight = height;
        }
    }

    function scheduleResize() {
        window.cancelAnimationFrame(frame);
        frame = window.requestAnimationFrame(resize);
    }

    resize();
    window.addEventListener('resize', scheduleResize);
    if (window.ResizeObserver) {
        var observer = new ResizeObserver(scheduleResize);
        observer.observe(toolbar);
    }
}
