"""Isolated browser checks using real reader assets, without contacting the live API.
Run with Python Playwright and Chromium installed: python tests/browser_readers.py
"""
from pathlib import Path
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from threading import Thread
import json
from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[1] / 'HerkesYazarOlsun.Portal/wwwroot'

def pdf_fixture():
    objects = ['<< /Type /Catalog /Pages 2 0 R >>', '<< /Type /Pages /Kids [3 0 R 5 0 R 7 0 R] /Count 3 >>']
    for n in range(3):
        content = f'BT /F1 18 Tf 30 700 Td (Table page {n + 1}) Tj ET 30 600 300 60 re S 180 600 m 180 660 l S'
        objects += [f'<< /Type /Page /Parent 2 0 R /MediaBox [0 0 400 800] /Resources << /Font << /F1 9 0 R >> >> /Contents {4 + n*2} 0 R >>',
                    f'<< /Length {len(content)} >>\nstream\n{content}\nendstream']
    objects.append('<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>')
    result = '%PDF-1.4\n'; offsets = []
    for i, obj in enumerate(objects):
        offsets.append(len(result)); result += f'{i+1} 0 obj\n{obj}\nendobj\n'
    xref = len(result)
    result += f'xref\n0 {len(objects)+1}\n0000000000 65535 f \n'
    result += ''.join(f'{offset:010} 00000 n \n' for offset in offsets)
    result += f'trailer\n<< /Size {len(objects)+1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF'
    return result.encode('ascii')

ARTICLE = '''<!doctype html><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><link rel="stylesheet" href="/css/articles.css">
<div class="article-area"><section class="article-reader" id="article-reader" data-kind="pdf" data-document-url="/sample.pdf">
<div class="article-toolbar"><button data-prev disabled>Önceki</button><span data-status></span><button data-next disabled>Sonraki</button>
<select data-zoom><option value="1">100%</option><option value="2">200%</option></select></div><p data-error hidden>Hata</p><div class="article-spread" data-spread></div></section></div>
<script type="module" src="/js/article-reader.mjs"></script>'''
BOOK = '''<!doctype html><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><link rel="stylesheet" href="/css/mobile-reader.css">
<div id="container"><nav><img onclick="location.href='/book'" /></nav><div id="main"><div id="features"><div>Kapak</div>
<div><button>Sayfayı Güncelle</button><div class="feature"><p>''' + ('Uzun kitap sayfası. ' * 1200) + '''</p></div></div><div>Arka kapak</div></div></div></div>
<script src="/js/mobile-reader.js"></script><script>initMobileReader()</script>'''

class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs): super().__init__(*args, directory=str(ROOT), **kwargs)
    def log_message(self, *args): pass
    def do_GET(self):
        fixtures = {'/article': ('text/html', ARTICLE.encode()), '/book': ('text/html', BOOK.encode()), '/sample.pdf': ('application/pdf', pdf_fixture())}
        if self.path in fixtures:
            kind, content = fixtures[self.path]; self.send_response(200); self.send_header('Content-Type', kind); self.end_headers(); self.wfile.write(content)
        else: super().do_GET()

server = ThreadingHTTPServer(('127.0.0.1', 0), Handler)
Thread(target=server.serve_forever, daemon=True).start()
base = f'http://127.0.0.1:{server.server_port}'
with sync_playwright() as playwright:
    browser = playwright.chromium.launch(headless=True)
    page = browser.new_page(viewport={'width':1280,'height':900})
    errors = []; page.on('pageerror', lambda error: errors.append(str(error)))
    page.goto(base + '/article'); page.wait_for_selector('canvas')
    page.wait_for_function("document.querySelectorAll('canvas').length === 2")
    assert page.locator('[data-status]').inner_text() == '1–2 / 3'
    page.locator('[data-next]').click(); page.wait_for_function("document.querySelector('[data-status]').textContent === '3 / 3'")
    assert page.locator('[data-next]').is_disabled()
    page.locator('[data-prev]').click(); assert page.locator('[data-status]').inner_text() == '1–2 / 3'
    page.set_viewport_size({'width':390,'height':844}); page.wait_for_timeout(500)
    assert page.locator('canvas').count() == 1
    assert page.locator('[data-status]').inner_text() == '1 / 3'
    page.locator('[data-zoom]').select_option('2'); page.wait_for_timeout(400)
    assert page.locator('.article-sheet').evaluate('(el) => el.scrollWidth > el.clientWidth')
    assert page.locator('.article-sheet').evaluate('(el) => el.scrollHeight > el.clientHeight')
    page.locator('[data-next]').click(); page.wait_for_timeout(300)
    assert page.locator('[data-status]').inner_text() == '2 / 3'
    page.screenshot(path='/private/tmp/hyo-article-mobile.png')
    page.goto(base + '/book'); page.locator('.mobile-reader-controls button').nth(1).click()
    assert page.locator('.mobile-book-page.is-current').evaluate('(el) => el.scrollHeight > el.clientHeight')
    page.locator('.mobile-book-page.is-current').evaluate('(el) => el.scrollTop = 300')
    assert page.locator('.mobile-book-page.is-current').evaluate('(el) => el.scrollTop') == 300
    assert page.evaluate('document.documentElement.scrollHeight <= window.innerHeight')
    assert page.locator('.mobile-book-page.is-current button').inner_text() == 'Sayfayı Güncelle'
    page.screenshot(path='/private/tmp/hyo-book-mobile.png')
    assert not errors, errors
    browser.close()
server.shutdown()
print('PASS PDF desktop spread, page bounds, mobile single page, zoom/scroll, editable mobile book scrolling, no browser errors')
