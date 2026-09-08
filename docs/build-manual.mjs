import { readFileSync, writeFileSync } from 'node:fs';
const original = readFileSync(new URL('./MANUAL_DEL_SISTEMA.html', import.meta.url), 'utf8');
const template = readFileSync(new URL('./MANUAL_INTERACTIVO.template.html', import.meta.url), 'utf8');
const figures = [...original.matchAll(/<figure>[\s\S]*?<\/figure>/g)].map(match => match[0]);
const gallery = figures.map(figure => figure
  .replace(/<img(?: loading="lazy")? /, '<img loading="lazy" ')
  .replace(/<figcaption>(?:Captura de referencia de versión anterior · )?/, '<figcaption>Captura de referencia de versión anterior · ')).join('\n');
if (!figures.length) throw new Error('No se encontraron las capturas del manual existente.');
writeFileSync(new URL('./MANUAL_DEL_SISTEMA.html', import.meta.url), template.replace('<!-- CAPTURAS -->', gallery));
console.log(`Manual generado con ${figures.length} capturas integradas.`);
