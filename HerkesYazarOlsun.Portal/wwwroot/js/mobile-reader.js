function initMobileReader() {
    document.documentElement.classList.add('mobile-reading');
    const book = document.getElementById('features');
    const pages = Array.from(book.children).filter(node => node.tagName !== 'AUDIO');
    pages.forEach(page => page.classList.add('mobile-book-page'));
    const controls = document.createElement('div');
    controls.className = 'mobile-reader-controls';
    controls.innerHTML = '<a href="#">Kitaba dön</a><button type="button">Önceki</button><span aria-live="polite"></span><button type="button">Sonraki</button>';
    const exitImage = document.querySelector('#container nav img[onclick]');
    controls.querySelector('a').addEventListener('click', event => { event.preventDefault(); if (exitImage) exitImage.click(); });
    document.getElementById('container').prepend(controls);
    const buttons = controls.querySelectorAll('button');
    let current = 0;
    function show(index) {
        if (!pages.length) return;
        pages[current].classList.remove('is-current');
        current = Math.max(0, Math.min(index, pages.length - 1));
        pages[current].classList.add('is-current');
        pages[current].scrollTop = 0;
        controls.querySelector('span').textContent = (current + 1) + ' / ' + pages.length;
        buttons[0].disabled = current === 0;
        buttons[1].disabled = current === pages.length - 1;
    }
    buttons[0].addEventListener('click', () => show(current - 1));
    buttons[1].addEventListener('click', () => show(current + 1));
    show(0);
}
