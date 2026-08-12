// Render catalogue decks (HTML) to PDF via Playwright Chromium.
// Usage: node docs/design_catalogue/src/build.mjs [deck.html ...]
// With no args, renders every src/<plane>/*.html to docs/design_catalogue/<plane>/<name>.pdf
import { chromium } from 'playwright';
import { readdir, mkdir } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const srcRoot = path.dirname(fileURLToPath(import.meta.url));
const outRoot = path.dirname(srcRoot);
const planes = ['grid-plane', 'information-plane', 'hud'];

async function collect() {
  const args = process.argv.slice(2);
  if (args.length) return args.map(a => path.resolve(a));
  const files = [];
  for (const plane of planes) {
    const dir = path.join(srcRoot, plane);
    try {
      for (const f of await readdir(dir)) if (f.endsWith('.html')) files.push(path.join(dir, f));
    } catch { /* plane dir may not exist yet */ }
  }
  try {
    for (const f of await readdir(srcRoot)) if (f.endsWith('.html')) files.push(path.join(srcRoot, f));
  } catch {}
  return files;
}

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });

for (const file of await collect()) {
  const rel = path.relative(srcRoot, file);
  const plane = planes.includes(path.dirname(rel)) ? path.dirname(rel) : '';
  const outDir = path.join(outRoot, plane);
  await mkdir(outDir, { recursive: true });
  const out = path.join(outDir, path.basename(file, '.html') + '.pdf');

  await page.goto(pathToFileURL(file).href, { waitUntil: 'networkidle' });
  await page.evaluate(() => document.fonts.ready);
  await page.pdf({
    path: out,
    width: '1280px',
    height: '720px',
    printBackground: true,
    margin: { top: 0, right: 0, bottom: 0, left: 0 },
  });
  console.log('rendered', path.relative(outRoot, out));
}

await browser.close();
