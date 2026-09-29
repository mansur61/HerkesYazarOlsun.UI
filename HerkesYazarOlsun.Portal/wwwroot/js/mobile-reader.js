function initMobileReader() {
    document.documentElement.classList.add('mobile-reading');
    const book = document.getElementById('features');
    const pages = Array.from(book.children).filter(node => node.hasAttribute('data-book-page'));
    if (!pages.length) return;
    pages.forEach(page => page.classList.add('mobile-book-page'));
    const controls = document.createElement('div');
    controls.className = 'mobile-reader-controls';
    controls.innerHTML = '<a href="#">Kitaba dön</a><button type="button">Önceki</button><span aria-live="polite"></span><button type="button">Sonraki</button>';
    const exitImage = document.querySelector('#container nav img[onclick]');
    controls.querySelector('a').addEventListener('click', event => { event.preventDefault(); if (exitImage) exitImage.click(); });
    document.getElementById('container').prepend(controls);
    const buttons = controls.querySelectorAll('button');
    let current = 0;
    const jump = createReaderJump(pages.length, index => show(index));
    controls.after(jump.form);
    function show(index) {
        if (!pages.length) return;
        pages[current].classList.remove('is-current');
        current = Math.max(0, Math.min(index, pages.length - 1));
        pages[current].classList.add('is-current');
        pages[current].scrollTop = 0;
        jump.update(current + 1);
        controls.querySelector('span').textContent = (current + 1) + ' / ' + pages.length;
        buttons[0].disabled = current === 0;
        buttons[1].disabled = current === pages.length - 1;
    }
    window.goToMobileBookPage = pageNumber => show(pageNumber - 1);
    buttons[0].addEventListener('click', () => show(current - 1));
    buttons[1].addEventListener('click', () => show(current + 1));
    show(0);
}

function createReaderJump(count, go) {
    const form = document.createElement('form');
    form.className = 'reader-jump';
    form.innerHTML = '<label>Sayfa <input name="page" type="number" min="1" step="1" required aria-label="Gidilecek sayfa"></label><span class="reader-total"></span><button type="submit" aria-label="Sayfaya git">Git <span aria-hidden="true">➜</span></button><span role="status" aria-live="polite"></span>';
    const input = form.querySelector('input'); input.max = String(count);
    form.querySelector('.reader-total').textContent = '/ ' + count;
    form.addEventListener('submit', event => {
        event.preventDefault();
        const page = Number(input.value);
        if (!Number.isInteger(page) || page < 1 || page > count) {
            form.querySelector('[role="status"]').textContent = '1 ile ' + count + ' arasında bir sayfa girin.';
            return;
        }
        form.querySelector('[role="status"]').textContent = '';
        go(page - 1);
    });
    return { form, update(page) { input.value = page; } };
}
function initDesktopReaderNavigation(book) {
    if (!book) return;
    let navigationTimer;
    function navigate(index) {
        clearTimeout(navigationTimer);
        if (book.animating) {
            navigationTimer = setTimeout(() => navigate(index), 50);
            return;
        }
        // The visible number is always the actual zero-based page index + 1.
        book.showPage(index, false);
        jump.update(index + 1);
    }
    const jump = createReaderJump(book.pages.length, navigate);
    document.querySelector('#container > nav').after(jump.form);
    const previous = book.onShowPage;
    book.onShowPage = function (...args) {
        if (previous) previous.apply(this, args);
        queueMicrotask(() => jump.update(book.currentPage + 1));
    };
    jump.update(book.currentPage + 1);
}
