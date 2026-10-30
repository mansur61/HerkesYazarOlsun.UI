(function () {
    'use strict';
    function init() {
        var dialog = document.getElementById('writing-companion');
        if (!dialog || dialog.dataset.ready) return;
        dialog.dataset.ready = 'true';
        var $ = function (selector) { return dialog.querySelector(selector); };
        var fields = [
            { id: 'title', label: 'Kitap adı', target: 'kitapAdi', question: 'Kitabının adı ne olsun? Henüz karar vermediysen bu alanı atlayabilirsin.' },
            { id: 'preface', label: 'Önsöz', target: 'onsoz', editor: 'onSozEditor', max: 500, question: 'Bu kitabı neden yazıyorsun? Okuyucuna başlangıçta ne söylemek istersin?' },
            { id: 'back', label: 'Arka kapak yazısı', target: 'arkaKapakYazi', editor: 'arkaKapakYaziEditor', max: 200, question: 'Kitabını birkaç cümleyle tanıtır mısın? Okuyucu bu kitapta ne bulacak?' },
            { id: 'category', label: 'Kategori', target: 'kitapKategori', question: 'Kitabının kategorisini söyle veya aşağıdaki listeden seç.' },
            { id: 'page', label: 'Sayfa metni', target: 'sayfaYazi', editor: 'sayfaYaziEditor', max: 1000, question: 'Bu sayfada ne anlatmak istiyorsun? Bir anıyı, bir sahneyi veya düşüncelerini kendi sözlerinle anlat.' }
        ].filter(function (field) { return document.getElementById(field.target); });
        if (!fields.length) return;
        fields.forEach(function (field) { field.initialQuestion = field.question; });
        var selector = $('#wc-target'), draft = $('#wc-draft'), status = $('#wc-status');
        var category = $('#wc-category'), mic = $('[data-wc-mic]');
        var Recognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        var canRecognize = !!Recognition && window.isSecureContext;
        var recognition = null, timer = null, active, drafts = {}, opener;
        var aiAvailable = false, aiRequest = null, aiHistory = [];
        var bookId = (document.getElementById('BOOKSID') || {}).value || 'new';
        var pageId = (document.getElementById('BOOKPAGESID') || {}).value || '';
        var storageKey = 'hyo-writing-v1:' + dialog.dataset.writerScope + ':' + location.pathname + ':' + bookId + ':' + pageId;
        try {
            var saved = JSON.parse(sessionStorage.getItem(storageKey) || '{}');
            fields.forEach(function (f) { if (typeof saved[f.id] === 'string') drafts[f.id] = saved[f.id]; });
        } catch (_) { /* Storage can be disabled; the in-memory draft still works. */ }
        try {
            var previousHistory = JSON.parse(sessionStorage.getItem(storageKey + ':conversation') || '[]');
            if (Array.isArray(previousHistory)) aiHistory = previousHistory.filter(function (turn) {
                return turn && ['user', 'assistant'].includes(turn.role) && typeof turn.text === 'string' && turn.text.length <= 6000;
            }).slice(-12);
        } catch (_) { }
        fields.forEach(function (field) { selector.add(new Option(field.label, field.id)); });
        var sourceCategory = document.getElementById('kitapKategori');
        if (sourceCategory) {
            Array.from(sourceCategory.options).forEach(function (option) {
                var copy = new Option(option.text, option.value);
                copy.disabled = option.disabled;
                category.add(copy);
            });
            category.value = sourceCategory.value;
        }

        function save() {
            if (active) drafts[active.id] = draft.value;
            try { sessionStorage.setItem(storageKey, JSON.stringify(drafts)); } catch (_) { }
        }
        function editor(field) {
            var node = field.editor && document.getElementById(field.editor);
            return node && window.Quill ? window.Quill.find(node) : null;
        }
        function currentText(field) {
            var quill = editor(field);
            if (quill) return quill.getText().replace(/\n$/, '');
            if (field.editor) return '';
            return document.getElementById(field.target).value;
        }
        function updateCount() {
            $('#wc-count').textContent = active.max ? draft.value.length + ' / ' + active.max + ' karakter' : draft.value.length + ' karakter';
        }
        function normalize(text) { return text.toLocaleLowerCase('tr-TR').replace(/[.,!?]/g, '').trim(); }
        function matchCategory() {
            var normalized = normalize(draft.value);
            var match = Array.from(category.options).find(function (o) { return o.value && normalize(o.text) === normalized; });
            category.value = match ? match.value : '';
        }
        function readQuestion() {
            if (recognition || !window.speechSynthesis || !window.SpeechSynthesisUtterance) return;
            window.speechSynthesis.cancel();
            var utterance = new SpeechSynthesisUtterance(active.question);
            utterance.lang = 'tr-TR';
            var voice = window.speechSynthesis.getVoices().find(function (v) { return v.lang.toLowerCase().startsWith('tr'); });
            if (voice) utterance.voice = voice;
            utterance.onerror = function () { if (dialog.open) status.textContent = 'Soru seslendirilemedi; ekrandaki soruyu okuyarak devam edebilirsin.'; };
            window.speechSynthesis.speak(utterance);
        }
        function choose(id) {
            save();
            active = fields.find(function (f) { return f.id === id; }) || fields[0];
            selector.value = active.id;
            draft.value = drafts[active.id] || '';
            $('#wc-question').textContent = active.question;
            category.hidden = $('#wc-category-label').hidden = active.id !== 'category';
            $('#wc-append-label').hidden = active.id !== 'page';
            $('[data-wc-next]').textContent = fields.indexOf(active) === fields.length - 1 ? 'Bitir' : 'Bu alanı atla →';
            $('#wc-interim').textContent = '';
            $('#wc-ai-preview').hidden = true;
            status.textContent = canRecognize ? 'Konuşmaya başlamak için mikrofon düğmesine bas.' : 'Bu tarayıcıda sesle yazma kullanılamıyor. Buraya yazabilir veya klavyenin mikrofonunu kullanabilirsin.';
            if (active.id === 'category') {
                if (draft.value) matchCategory();
                else category.value = sourceCategory.value;
            }
            updateCount();
            if ($('#wc-auto-read').checked) readQuestion();
        }
        function busy(value) {
            value = value || !!aiRequest;
            selector.disabled = draft.readOnly = category.disabled = value;
            $('[data-wc-apply]').disabled = $('[data-wc-next]').disabled = $('[data-wc-clear]').disabled = $('[data-wc-read]').disabled = value;
            $('#wc-auto-read').disabled = value;
            mic.disabled = !canRecognize || !!aiRequest;
            dialog.querySelectorAll('[data-wc-ai]').forEach(function (button) { button.disabled = value || !aiAvailable; });
            $('[data-wc-ai-use]').disabled = $('[data-wc-reset]').disabled = value;
            mic.setAttribute('aria-pressed', String(!!recognition));
            mic.textContent = recognition ? '■ Konuşmayı bitir' : '🎙 Konuşmaya başla';
        }
        function abort() {
            if (aiRequest) { aiRequest.abort(); aiRequest = null; $('#wc-ai-status').textContent = 'İstek iptal edildi; taslağın korunuyor.'; }
            clearTimeout(timer);
            var previous = recognition;
            recognition = null;
            if (previous) { try { previous.abort(); } catch (_) { } }
            busy(false);
            $('#wc-interim').textContent = '';
            if (window.speechSynthesis) window.speechSynthesis.cancel();
            save();
        }
        function close() { abort(); dialog.close(); }
        function next() {
            var index = fields.indexOf(active);
            if (index + 1 < fields.length) choose(fields[index + 1].id);
            else close();
        }
        function apply() {
            if (recognition) return;
            var value = draft.value.trim();
            if (active.id === 'category') {
                if (!category.value) { status.textContent = 'Lütfen listeden geçerli bir kategori seç.'; return; }
                sourceCategory.value = category.value;
                sourceCategory.dispatchEvent(new Event('change', { bubbles: true }));
                drafts[active.id] = '';
                draft.value = '';
                next();
                return;
            }
            if (!value) { status.textContent = 'Önce bir şeyler anlat veya yaz.'; return; }
            var quill = editor(active);
            if (active.editor && !quill) { status.textContent = 'Yazı alanı henüz hazır değil. Taslağın korunuyor; formdaki ilgili adımı açıp tekrar dene.'; return; }
            var old = currentText(active);
            var append = active.id === 'page' && $('#wc-append').checked;
            var combined = append && old.trim() ? old + '\n' + value : value;
            var target = document.getElementById(active.target);
            var limit = active.max || (target.maxLength > 0 ? target.maxLength : 0);
            if (limit && combined.length > limit) {
                status.textContent = 'Bu alan en fazla ' + limit + ' karakter olabilir. ' + (append ? 'Mevcut metinle birlikte ' : '') + combined.length + ' karakter oldu; taslağını kısaltıp tekrar dene.';
                return;
            }
            if (!append && (old.trim() || (quill && quill.root.querySelector('img'))) && !window.confirm('Bu alanda zaten içerik var. Onayladığın taslakla değiştirmek istiyor musun?')) return;
            if (quill) {
                if (append) quill.insertText(Math.max(0, quill.getLength() - 1), (old.trim() ? '\n' : '') + value, 'user');
                else quill.setText(value, 'user');
                target.value = quill.root.innerHTML;
            } else target.value = value;
            target.dispatchEvent(new Event('input', { bubbles: true }));
            target.dispatchEvent(new Event('change', { bubbles: true }));
            draft.value = '';
            save();
            if (active.id === 'page') close();
            else next();
        }
        function bookContext() {
            return fields.map(function (field) {
                if (field.id === 'category') return 'Kategori seçenekleri: ' + Array.from(sourceCategory.options).filter(function (o) { return o.value; }).map(function (o) { return o.text; }).join(', ');
                return field.label + ': ' + currentText(field).slice(0, 1200);
            }).join('\n').slice(0, 6000);
        }
        function saveHistory() {
            aiHistory = aiHistory.slice(-12);
            while (aiHistory.reduce(function (sum, turn) { return sum + turn.text.length; }, 0) > 18000) aiHistory.shift();
            try { sessionStorage.setItem(storageKey + ':conversation', JSON.stringify(aiHistory)); } catch (_) { }
        }
        async function askAi(action) {
            if (!aiAvailable || aiRequest || recognition) return;
            var text = draft.value.trim();
            if (action !== 'ask' && !text) { $('#wc-ai-status').textContent = 'Önce bir şeyler anlat veya taslağını yaz.'; return; }
            if (text.length > 6000) { $('#wc-ai-status').textContent = 'Yapay zekâya göndermek için taslağını 6000 karakterin altına indir.'; return; }
            var controller = new AbortController();
            aiRequest = controller;
            busy(true);
            if (window.speechSynthesis) window.speechSynthesis.cancel();
            $('#wc-ai-status').textContent = 'Yazı arkadaşın düşünüyor…';
            var timeout = setTimeout(function () { controller.abort(); }, 55000);
            try {
                var response = await fetch('/YaziArkadasi/Oneri', {
                    method: 'POST', credentials: 'same-origin', signal: controller.signal,
                    headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': (document.querySelector('meta[name="csrf-token"]') || {}).content || '' },
                    body: JSON.stringify({ action: action, target: active.id, text: text, context: bookContext(), history: aiHistory })
                });
                if (response.status === 429) throw new Error('Günlük yapay zekâ kullanım sınırına ulaştın. Sesli veya elle yazmaya devam edebilirsin.');
                if (response.status === 401 || response.status === 403) throw new Error('Yapay zekâ desteği için hesabına yeniden giriş yapmalısın.');
                if (response.status === 400) throw new Error('İstek doğrulanamadı. Taslağını koruyup sayfayı yenileyerek tekrar dene.');
                var data = await response.json();
                if (!response.ok) throw new Error(data.message || 'Yapay zekâ yanıt veremiyor. Daha sonra tekrar dene.');
                if (typeof data.text !== 'string' || !data.text.trim()) throw new Error('Kullanılabilir yanıt alınamadı.');
                if (aiRequest !== controller || !dialog.open) return;
                if (text) aiHistory.push({ role: 'user', text: text });
                aiHistory.push({ role: 'assistant', text: data.text });
                saveHistory();
                if (action === 'ask') {
                    $('#wc-question').textContent = data.text;
                    active.question = data.text;
                    $('#wc-ai-status').textContent = 'Soruyu yanıtlamak için anlatmaya devam edebilirsin.';
                    if ($('#wc-auto-read').checked) readQuestion();
                } else {
                    $('#wc-ai-result').value = data.text;
                    $('#wc-ai-preview').hidden = false;
                    $('#wc-ai-status').textContent = 'Öneri hazır. Kontrol edip seç; mevcut form metnin değiştirilmedi.';
                }
            } catch (error) {
                if (aiRequest === controller) $('#wc-ai-status').textContent = error.name === 'AbortError' ? 'İstek zamanında tamamlanamadı. Taslağın korunuyor.' : (error instanceof TypeError || error instanceof SyntaxError ? 'Bağlantı kurulamadı. Taslağın korunuyor; tekrar deneyebilirsin.' : error.message);
            } finally {
                clearTimeout(timeout);
                if (aiRequest === controller) { aiRequest = null; busy(false); }
            }
        }
        function startOrStop() {
            if (recognition) {
                status.textContent = 'Konuşma tamamlanıyor…';
                recognition.stop();
                return;
            }
            if (!canRecognize) return;
            if (window.speechSynthesis) window.speechSynthesis.cancel();
            var rec = new Recognition(), failed = false;
            recognition = rec;
            rec.lang = 'tr-TR';
            rec.continuous = true;
            rec.interimResults = true;
            busy(true);
            status.textContent = 'Mikrofon izni bekleniyor…';
            rec.onstart = function () {
                if (recognition !== rec) return;
                status.textContent = 'Dinleniyor… Bitir düğmesine basınca kontrol edip ekleyebilirsin. En fazla 90 saniye.';
                timer = setTimeout(function () { if (recognition === rec) rec.stop(); }, 90000);
            };
            rec.onresult = function (event) {
                if (recognition !== rec || !dialog.open) return;
                var interim = '';
                for (var i = event.resultIndex; i < event.results.length; i++) {
                    var result = event.results[i], text = result[0].transcript.trim();
                    if (result.isFinal) draft.value += (draft.value && !/\s$/.test(draft.value) ? ' ' : '') + text;
                    else interim += text + ' ';
                }
                $('#wc-interim').textContent = interim;
                if (active.id === 'category') matchCategory();
                updateCount();
                save();
            };
            rec.onerror = function (event) {
                if (recognition !== rec) return;
                failed = true;
                var messages = {
                    'not-allowed': 'Mikrofon izni verilmedi. Tarayıcı ayarlarından izin verebilir veya yazarak devam edebilirsin.',
                    'service-not-allowed': 'Tarayıcının ses tanıma hizmeti kullanılamıyor. Yazarak devam edebilirsin.',
                    'audio-capture': 'Mikrofon bulunamadı veya başka bir uygulama tarafından kullanılıyor.',
                    'network': 'Ses tanıma bağlantısı kurulamadı. İnternetini kontrol et; taslağın korunuyor.',
                    'no-speech': 'Ses algılanamadı. Mikrofon düğmesiyle tekrar deneyebilirsin.',
                    'language-not-supported': 'Bu tarayıcı Türkçe ses tanımayı desteklemiyor. Yazarak devam edebilirsin.'
                };
                status.textContent = messages[event.error] || 'Ses tanıma durdu. Taslağını kontrol edip yeniden deneyebilirsin.';
            };
            rec.onend = function () {
                if (recognition !== rec) return;
                clearTimeout(timer);
                recognition = null;
                busy(false);
                $('#wc-interim').textContent = '';
                if (!failed) status.textContent = 'Dinleme bitti. Metni kontrol et; onaylamadan forma eklenmez.';
            };
            try { rec.start(); } catch (_) { abort(); status.textContent = 'Mikrofon başlatılamadı. Yazarak devam edebilirsin.'; }
        }
        document.querySelectorAll('[data-writing-open]').forEach(function (button) {
            button.addEventListener('click', function () {
                opener = button;
                dialog.showModal();
                choose(button.dataset.writingOpen);
                selector.focus();
            });
        });
        selector.addEventListener('change', function () { choose(selector.value); });
        draft.addEventListener('input', function () { save(); updateCount(); if (active.id === 'category') matchCategory(); });
        category.addEventListener('change', function () {
            draft.value = category.selectedOptions[0] && category.value ? category.selectedOptions[0].text : '';
            save(); updateCount();
        });
        dialog.querySelectorAll('[data-wc-ai]').forEach(function (button) {
            button.addEventListener('click', function () { askAi(button.dataset.wcAi); });
        });
        $('[data-wc-reset]').addEventListener('click', function () {
            if (!window.confirm('Bu sekmedeki yardımcı taslakları ve konuşma geçmişi silinsin mi? Formdaki metinler korunur.')) return;
            drafts = {}; aiHistory = []; draft.value = '';
            fields.forEach(function (field) { field.question = field.initialQuestion; });
            save(); saveHistory(); choose(active.id);
            $('#wc-ai-status').textContent = aiAvailable ? 'Yeni konuşmaya hazırız.' : 'Sesli ve elle doldurmaya devam edebilirsin.';
        });
        $('[data-wc-ai-use]').addEventListener('click', function () {
            if (draft.value.trim() && !window.confirm('Anlatım taslağını bu öneriyle değiştirmek istiyor musun? Formdaki metin henüz değişmez.')) return;
            draft.value = $('#wc-ai-result').value;
            save(); updateCount();
            if (active.id === 'category') matchCategory();
            $('#wc-ai-preview').hidden = true;
            status.textContent = 'Taslak seçildi. Forma aktarmak için Onayla ve alana ekle düğmesini kullan.';
        });
        fetch('/YaziArkadasi/Durum', { credentials: 'same-origin' }).then(function (response) {
            if (!response.ok) throw new Error();
            return response.json();
        }).then(function (data) {
            aiAvailable = data.available === true;
            $('#wc-ai-status').textContent = aiAvailable ? 'Yapay zekâ desteği hazır. Anlattıklarını birlikte geliştirebiliriz.' : (data.signedIn ? 'Yapay zekâ henüz etkinleştirilmedi. Sesle ve elle yazma kullanılabilir.' : 'Yapay zekâ desteği için hesabına giriş yap. Sesle ve elle yazma kullanılabilir.');
            busy(!!recognition);
        }).catch(function () { $('#wc-ai-status').textContent = 'Yapay zekâ bağlantısı kontrol edilemedi. Sesle ve elle yazmaya devam edebilirsin.'; });
        $('[data-wc-read]').addEventListener('click', readQuestion);
        $('[data-wc-mic]').addEventListener('click', startOrStop);
        $('[data-wc-apply]').addEventListener('click', apply);
        $('[data-wc-next]').addEventListener('click', next);
        $('[data-wc-close]').addEventListener('click', close);
        $('[data-wc-clear]').addEventListener('click', function () {
            if (draft.value && !window.confirm('Bu alana ait taslağı temizlemek istiyor musun? Formdaki yazı değişmez.')) return;
            draft.value = ''; save(); updateCount();
        });
        dialog.addEventListener('cancel', abort);
        dialog.addEventListener('close', function () { abort(); if (opener) opener.focus(); });
        document.addEventListener('visibilitychange', function () { if (document.hidden) abort(); });
        window.addEventListener('pagehide', abort);
        mic.disabled = !canRecognize;
        if (!window.speechSynthesis || !window.SpeechSynthesisUtterance) {
            $('[data-wc-read]').hidden = true;
            $('#wc-auto-read').checked = false;
            $('#wc-auto-read').parentElement.hidden = true;
        }
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();
}());
