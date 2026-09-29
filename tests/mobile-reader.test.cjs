const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const source = fs.readFileSync(__dirname + '/../HerkesYazarOlsun.Portal/wwwroot/js/mobile-reader.js', 'utf8');
test('jump accepts displayed 1-based pages and rejects invalid values', () => {
    const input = { value: '' }, status = {}, total = {};
    const form = { querySelector: q => q === 'input' ? input : q === '.reader-total' ? total : status,
        addEventListener: (event, fn) => { form.submit = fn; } };
    const context = vm.createContext({ document: { createElement: () => form } });
    vm.runInContext(source, context);
    const requested = [];
    const jump = context.createReaderJump(38, page => requested.push(page));
    for (const value of ['4', '1', '38']) { input.value = value; form.submit({ preventDefault() {} }); }
    assert.deepEqual(requested, [3, 0, 37]);
    for (const value of ['', '0', '39', '3.5', 'abc']) { input.value = value; form.submit({ preventDefault() {} }); }
    assert.equal(requested.length, 3);
    jump.update(4); assert.equal(input.value, 4);
});
