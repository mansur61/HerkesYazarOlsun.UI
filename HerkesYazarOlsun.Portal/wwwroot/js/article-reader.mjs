for (const form of document.querySelectorAll('[data-confirm-delete]')) {
    form.addEventListener('submit', event => {
        if (!window.confirm('Bu makale ve yüklenen belgesi kalıcı olarak silinsin mi?')) event.preventDefault();
    });
}
const root = document.getElementById('article-reader');
if (root) {
    const spread = root.querySelector('[data-spread]');
    const status = root.querySelector('[data-status]');
    const previous = root.querySelector('[data-prev]');
    const next = root.querySelector('[data-next]');
    const zoom = root.querySelector('[data-zoom]');
    const narrow = window.matchMedia('(max-width: 768px)');
    let pdf, textPages, current = 1, count = 0, generation = 0;
    const perSpread = () => narrow.matches ? 1 : 2;
    function splitText(text) {
        const pages = [];
        while (text.length) {
            let end = text.length > 1800 ? text.lastIndexOf(' ', 1800) : text.length;
            if (end < 900) end = Math.min(1800, text.length);
            pages.push(text.slice(0, end)); text = text.slice(end).trimStart();
        }
        return pages.length ? pages : [''];
    }
    async function render() {
        const token = ++generation;
        const last = Math.min(count, current + perSpread() - 1);
        previous.disabled = current === 1; next.disabled = last === count;
        status.textContent = `${current === last ? current : current + '–' + last} / ${count}`;
        root.querySelector('[data-error]').hidden = true;
        spread.replaceChildren();
        try {
            for (let index = current; index <= last; index++) {
                const sheet = document.createElement('div');
                sheet.className = 'article-sheet'; sheet.tabIndex = 0;
                sheet.setAttribute('aria-label', 'Sayfa ' + index);
                spread.append(sheet);
                if (pdf) {
                    const page = await pdf.getPage(index);
                    if (token !== generation) return;
                    const viewport = page.getViewport({ scale: 1 });
                    const scale = (sheet.clientWidth / viewport.width) * Number(zoom.value);
                    const display = page.getViewport({ scale });
                    const density = Math.min(window.devicePixelRatio || 1, 2);
                    const canvas = document.createElement('canvas');
                    canvas.width = Math.floor(display.width * density);
                    canvas.height = Math.floor(display.height * density);
                    canvas.style.width = display.width + 'px'; canvas.style.height = display.height + 'px';
                    canvas.setAttribute('role', 'img'); canvas.setAttribute('aria-label', 'PDF sayfa ' + index);
                    sheet.append(canvas);
                    await page.render({ canvasContext: canvas.getContext('2d'), viewport: display,
                        transform: density === 1 ? null : [density, 0, 0, density, 0, 0] }).promise;
                    if (token !== generation) return;
                    page.cleanup();
                } else {
                    const content = document.createElement('div'); content.className = 'article-text-page';
                    content.style.fontSize = 18 * Number(zoom.value) + 'px';
                    content.textContent = textPages[index - 1]; sheet.append(content);
                }
            }
        } catch (error) {
            if (token === generation) root.querySelector('[data-error]').hidden = false;
            console.error('Makale sayfası açılamadı', error);
        }
    }
    try {
        if (root.dataset.kind === 'pdf') {
            const library = await import('../lib/pdfjs/legacy/build/pdf.min.mjs');
            library.GlobalWorkerOptions.workerSrc = new URL('../lib/pdfjs/legacy/build/pdf.worker.min.mjs', import.meta.url).href;
            const base = new URL('../lib/pdfjs/', import.meta.url);
            pdf = await library.getDocument({ url: root.dataset.documentUrl, isEvalSupported: false,
                cMapUrl: new URL('cmaps/', base).href, cMapPacked: true,
                standardFontDataUrl: new URL('standard_fonts/', base).href,
                wasmUrl: new URL('wasm/', base).href }).promise;
            count = pdf.numPages;
        } else {
            textPages = splitText(root.querySelector('[data-text]').textContent); count = textPages.length;
        }
        previous.addEventListener('click', () => { current = Math.max(1, current - perSpread()); render(); });
        next.addEventListener('click', () => { current = Math.min(count, current + perSpread()); render(); });
        zoom.addEventListener('change', render);
        let timer;
        window.addEventListener('resize', () => { clearTimeout(timer); timer = setTimeout(render, 180); });
        await render();
    } catch (error) {
        status.textContent = 'Belge açılamadı'; root.querySelector('[data-error]').hidden = false;
        console.error('Makale yüklenemedi', error);
    }
}
