const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const source = fs.readFileSync(__dirname + '/../HerkesYazarOlsun.Portal/wwwroot/js/mobile-reader.js', 'utf8');
function element(tagName = 'DIV') {
    const classes = new Set();
    return { tagName, scrollTop: 80, disabled: false, events: {},
        classList: { add: x => classes.add(x), remove: x => classes.delete(x), contains: x => classes.has(x) },
        addEventListener(event, fn) { this.events[event] = fn; } };
}
test('mobile reader excludes audio, changes pages, resets scroll and bounds navigation', () => {
    const pages = [element(), element('AUDIO'), element(), element()];
    const buttons = [element('BUTTON'), element('BUTTON')];
    const status = element('SPAN'), back = element('A');
    const controls = { querySelector: q => q === 'a' ? back : status, querySelectorAll: () => buttons };
    const document = { documentElement: element(), createElement: () => controls,
        querySelector: () => null,
        getElementById: id => id === 'features' ? { children: pages } : { prepend() {} } };
    const context = vm.createContext({ document });
    vm.runInContext(source, context);
    context.initMobileReader();
    assert.equal(status.textContent, '1 / 3');
    assert.equal(buttons[0].disabled, true);
    assert.equal(pages[1].classList.contains('mobile-book-page'), false);
    buttons[1].events.click();
    assert.equal(status.textContent, '2 / 3');
    assert.equal(pages[0].classList.contains('is-current'), false);
    assert.equal(pages[2].scrollTop, 0);
    buttons[1].events.click();
    assert.equal(buttons[1].disabled, true);
    buttons[1].events.click();
    assert.equal(status.textContent, '3 / 3');
    buttons[0].events.click();
    assert.equal(status.textContent, '2 / 3');
});
