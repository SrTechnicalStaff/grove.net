// Screenshot each .slide of a deck HTML to PNGs for review.
// Usage: node snap.mjs <deck.html> <outdir>
import { chromium } from 'playwright';
import { mkdir } from 'node:fs/promises';
import path from 'node:path';
import { pathToFileURL } from 'node:url';

const [file, outdir] = process.argv.slice(2);
await mkdir(outdir, { recursive: true });
const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
await page.goto(pathToFileURL(path.resolve(file)).href, { waitUntil: 'networkidle' });
await page.evaluate(() => document.fonts.ready);
const slides = await page.locator('.slide').count();
for (let i = 0; i < slides; i++) {
  await page.locator('.slide').nth(i).screenshot({ path: path.join(outdir, `slide-${String(i + 1).padStart(2, '0')}.png`) });
}
console.log('snapped', slides, 'slides');
await browser.close();
