"""Run with Python Playwright/Chromium. Download the site's Quill 1.3.6 to /tmp/hyo-quill.js first.
Uses the real shared dialog, stylesheet, JS and Quill; speech/provider responses are mocked.
"""
from pathlib import Path
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from threading import Thread
from playwright.sync_api import sync_playwright
import json

PORTAL = Path(__file__).resolve().parents[1] / 'HerkesYazarOlsun.Portal'
partial = (PORTAL / 'Views/Shared/_WritingCompanion.cshtml').read_text()
partial = partial[partial.index('<dialog'):].replace('@writerScope', 'test-user')
base = '''<!doctype html><html lang="tr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><meta name="csrf-token" content="test-csrf"><link rel="stylesheet" href="/css/writing-companion.css"></head><body>
<form id="book"><button type="button" class="writing-companion-open" data-writing-open="title">Ön bilgileri doldur</button><input id="kitapAdi"><div id="onSozEditor"></div><textarea id="onsoz" hidden></textarea><div id="arkaKapakYaziEditor"></div><textarea id="arkaKapakYazi" hidden></textarea><select id="kitapKategori"><option value="" disabled selected>Seç</option><option value="1">Roman</option><option value="2">Anı</option></select>PAGE</form>
''' + partial + '''<script src="/quill.js"></script><script>
window.submitted=false;document.getElementById('book').onsubmit=e=>{e.preventDefault();window.submitted=true;};
['onSozEditor','arkaKapakYaziEditor','sayfaYaziEditor'].forEach(id=>{if(document.getElementById(id)) new Quill('#'+id);});
</script><script src="/js/writing-companion.js"></script></body></html>'''
page_markup = '<button type="button" data-writing-open="page">Sayfaya anlat</button><input id="BOOKSID" value="book-1"><input id="BOOKPAGESID" value="4"><div id="sayfaYaziEditor"></div><textarea id="sayfaYazi" hidden></textarea>'
class Handler(SimpleHTTPRequestHandler):
    def __init__(self,*args,**kwargs): super().__init__(*args,directory=str(PORTAL/'wwwroot'),**kwargs)
    def log_message(self,*args): pass
    def do_GET(self):
        if self.path in ['/write','/import']:
            data=base.replace('PAGE',page_markup if self.path=='/write' else '').encode()
        elif self.path=='/quill.js': data=Path('/private/tmp/hyo-quill.js').read_bytes()
        else: return super().do_GET()
        self.send_response(200);self.send_header('Content-Type','application/javascript' if self.path=='/quill.js' else 'text/html; charset=utf-8');self.end_headers();self.wfile.write(data)
server=ThreadingHTTPServer(('127.0.0.1',0),Handler)
Thread(target=server.serve_forever,daemon=True).start()
origin=f'http://127.0.0.1:{server.server_port}'
mock_speech='''window.SpeechRecognition=class {
 constructor(){window.lastRecognition=this;}
 start(){this.onstart();} stop(){this.onend();} abort(){this.aborted=true;if(this.onend)this.onend();}
 emit(text,final=true){let r=[{transcript:text}];r.isFinal=final;this.onresult({resultIndex:0,results:[r]});}
};window.webkitSpeechRecognition=undefined;'''
with sync_playwright() as pw:
    browser=pw.chromium.launch(headless=True)
    page=browser.new_page(viewport={'width':1440,'height':1000})
    page.add_init_script(mock_speech)
    errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
    available={'value':True};answer={'status':200,'body':{'text':'Önerilen kısa taslak.'}}; requests=[]
    page.route('**/YaziArkadasi/Durum',lambda r:r.fulfill(json={'available':available['value'],'signedIn':True}))
    def ai(r):
        requests.append(r.request.post_data_json)
        assert r.request.headers['requestverificationtoken']=='test-csrf'
        r.fulfill(status=answer['status'],json=answer['body'])
    page.route('**/YaziArkadasi/Oneri',ai)
    page.goto(origin+'/write');page.click('[data-writing-open="title"]')
    page.uncheck('#wc-auto-read')
    page.click('[data-wc-mic]');page.evaluate("lastRecognition.emit('Benim Kitabım')")
    assert page.input_value('#kitapAdi')==''
    page.click('[data-wc-mic]');page.click('[data-wc-apply]')
    assert page.input_value('#kitapAdi')=='Benim Kitabım'
    assert page.input_value('#wc-target')=='preface'
    page.fill('#wc-draft','x'*501);page.click('[data-wc-apply]')
    assert 'en fazla 500' in page.inner_text('#wc-status')
    page.fill('#wc-draft','<img src=x onerror=alert(1)> Anılarım')
    page.click('[data-wc-apply]')
    assert page.locator('#onSozEditor img').count()==0
    assert '<img' in page.inner_text('#onSozEditor')
    assert '&lt;img' in page.input_value('#onsoz')
    page.click('[data-wc-next]')
    page.fill('#wc-draft','bilinmeyen kategori');page.click('[data-wc-apply]')
    assert 'geçerli' in page.inner_text('#wc-status')
    page.select_option('#wc-category','1');page.click('[data-wc-apply]')
    assert page.input_value('#kitapKategori')=='1'
    assert page.input_value('#wc-target')=='page'
    page.fill('#wc-draft','İlk sayfa.');page.click('[data-wc-apply]')
    assert not page.locator('#writing-companion').is_visible()
    assert 'İlk sayfa.' in page.input_value('#sayfaYazi')
    page.click('[data-writing-open="page"]');page.fill('#wc-draft','İkinci paragraf.')
    page.click('[data-wc-ai="draft"]');page.wait_for_selector('#wc-ai-preview',state='visible')
    assert requests[-1]['target']=='page'
    assert 'İlk sayfa.' in requests[-1]['context']
    assert 'İkinci paragraf.'==page.input_value('#wc-draft')
    assert 'Önerilen' not in page.input_value('#sayfaYazi')
    page.once('dialog',lambda d:d.accept());page.click('[data-wc-ai-use]')
    assert page.input_value('#wc-draft')=='Önerilen kısa taslak.'
    page.click('[data-wc-apply]')
    assert 'İlk sayfa.' in page.input_value('#sayfaYazi') and 'Önerilen kısa taslak.' in page.input_value('#sayfaYazi')
    page.click('[data-writing-open="page"]');page.fill('#wc-draft','Korunacak metin')
    answer.update(status=503,body={'message':'Yapay zekâ anahtarı geçersiz veya süresi dolmuş. Mikrofonla ya da yazarak devam edebilirsin.'})
    page.click('[data-wc-ai="draft"]');page.wait_for_function("document.querySelector('#wc-ai-status').textContent.includes('süresi dolmuş')")
    assert page.input_value('#wc-draft')=='Korunacak metin'
    assert not page.locator('[data-wc-mic]').is_disabled()
    assert 'Mikrofonla ya da yazarak devam edebilirsin' in page.inner_text('#wc-ai-status')
    answer.update(status=200,body={'text':'O gün yanında kim vardı?'})
    page.click('[data-wc-ai="ask"]');page.wait_for_function("document.querySelector('#wc-question').textContent==='O gün yanında kim vardı?'")
    assert page.input_value('#wc-draft')=='Korunacak metin'
    page.click('[data-wc-mic]');page.evaluate("lastRecognition.onerror({error:'not-allowed'});lastRecognition.onend()")
    assert 'izin' in page.inner_text('#wc-status')
    assert not page.locator('[data-wc-apply]').is_disabled()
    page.click('[data-wc-mic]');page.keyboard.press('Escape')
    assert page.evaluate('lastRecognition.aborted')
    page.evaluate("lastRecognition.emit('Geç gelen cevap')")
    page.click('[data-writing-open="page"]')
    assert page.input_value('#wc-draft')=='Korunacak metin'
    page.click('[data-wc-close]');page.reload();page.click('[data-writing-open="page"]')
    assert page.input_value('#wc-draft')=='Korunacak metin'
    assert not page.evaluate('window.submitted')
    page.screenshot(path='/private/tmp/hyo-writing-desktop.png')
    page.evaluate("document.getElementById('kitapAdi').value='Korunan kitap'")
    page.select_option('#wc-target','title');page.fill('#wc-draft','Yeni ad')
    page.once('dialog',lambda d:d.dismiss());page.click('[data-wc-apply]')
    assert page.input_value('#kitapAdi')=='Korunan kitap'
    page.once('dialog',lambda d:d.accept());page.click('[data-wc-reset]')
    assert page.input_value('#wc-draft')==''
    assert page.input_value('#kitapAdi')=='Korunan kitap'
    assert page.evaluate("JSON.parse(sessionStorage.getItem(Object.keys(sessionStorage).find(k=>k.endsWith(':conversation')))).length") == 0

    # Mobile/import: no page field, no native recognition, no AI key.
    available['value']=False
    page.add_init_script('window.SpeechRecognition=undefined;window.webkitSpeechRecognition=undefined;')
    page.set_viewport_size({'width':390,'height':844});page.goto(origin+'/import');page.click('[data-writing-open="title"]')
    assert page.locator('[data-wc-mic]').is_disabled()
    assert page.locator('[data-wc-ai="draft"]').is_disabled()
    assert page.locator('#wc-target option[value="page"]').count()==0
    assert page.input_value('#wc-draft')==''
    page.fill('#wc-draft','Mobil kitap');page.click('[data-wc-apply]')
    assert page.input_value('#kitapAdi')=='Mobil kitap'
    assert page.locator('#writing-companion').evaluate('(e)=>e.scrollWidth<=e.clientWidth')
    assert page.locator('.wc-body').evaluate('(e)=>e.scrollHeight>e.clientHeight')
    page.screenshot(path='/private/tmp/hyo-writing-mobile.png')
    assert not errors,errors
    browser.close()
server.shutdown()
print('PASS desktop/mobile, real Quill, sequential fields, speech mock/permission/late results, limits, drafts, category, AI preview/failure, keyless fallback')
