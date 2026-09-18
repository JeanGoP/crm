const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const path = require('node:path');
const ts = require('../frontend/node_modules/typescript');
const context = { exports: {} };
vm.runInNewContext(ts.transpileModule(fs.readFileSync(path.join(__dirname, '../frontend/src/creditPhones.ts'), 'utf8'),
  { compilerOptions: { module: ts.ModuleKind.CommonJS } }).outputText, context);
const { normalizeCreditPhone: normalize, findDuplicateCreditPhones: find, creditPhoneFieldError: error, duplicateCreditPhoneMessage: message } = context.exports;
for (const input of ['3001234567', '300 123-4567', '(300) 123.4567', '+57 (300) 123-4567', '57 3001234567', '0057 3001234567'])
  assert.equal(normalize(input), '3001234567');
assert.equal(normalize(null), '');
assert.equal(normalize(' - () '), '');
assert.equal(normalize('5731234567'), '5731234567');
assert.notEqual(normalize('+58 3001234567'), normalize('3001234567'));
assert.equal(normalize('+1 202-555-0100'), normalize('001 2025550100'));
const paths = ['mobile', 'reference1Mobile', 'reference2Mobile', ...[0,1].flatMap(i => ['mobile', 'reference1Mobile', 'reference2Mobile'].map(f => `coDebtors.${i}.${f}`))];
const put = (app, key, val) => { const parts = key.split('.'); const field = parts.pop(); let target = app; for (const part of parts) target = target[part]; target[field] = val; };
const fresh = () => { const app = { coDebtors: [{ name: 'Ana' }, { name: 'Luis' }] }; paths.forEach((key, i) => put(app, key, String(3000000000+i))); return app; };
assert.equal(find(fresh()).length, 0);
for (let i=0;i<paths.length;i++) for (let j=i+1;j<paths.length;j++) {
  const app=fresh(); put(app,paths[i],'+57 (310) 999-8877'); put(app,paths[j],'0057 3109998877');
  const before=JSON.stringify(app), groups=find(app);
  assert.equal(groups.length,1); assert.equal(groups[0].fields.length,2);
  assert.ok(error(groups,paths[i])); assert.ok(error(groups,paths[j]));
  assert.ok(message(groups).includes(groups[0].fields[0].label));
  assert.ok(message(groups).includes(groups[0].fields[1].label));
  assert.equal(JSON.stringify(app),before);
  put(app,paths[j],'3119998877'); assert.equal(find(app).length,0);
}
const mirrored=fresh(); mirrored.coDebtorName='Ana'; mirrored.coDebtorMobile=mirrored.coDebtors[0].mobile;
assert.equal(find(mirrored).length,0);
assert.equal(find({ mobile:'3001234567', coDebtorName:'Anterior', coDebtorMobile:'+57 3001234567' }).length,1);
assert.equal(find({ mobile:'3001234567', coDebtorName:'Retirado', coDebtorMobile:'+57 3001234567', coDebtors:[] }).length,0);
assert.equal(find({ mobile:'', reference1Mobile:'- ()', reference2Mobile:null }).length,0);
const triple=fresh(); paths.slice(0,3).forEach(p=>put(triple,p,'3001111111'));
assert.equal(find(triple)[0].fields.length,3);
console.log('OK: normalización, 36 cruces de teléfonos, nombres de campos, corrección, vacíos, prefijos extranjeros y compatibilidad.');
