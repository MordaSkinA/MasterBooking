// Сравнение «макет против страницы».
// Для каждой пары открывает макет из PAGES (file://) и страницу приложения (BASE_URL),
// окно 1280x900, снимает скриншоты mockup-<имя>.png и page-<имя>.png, считает процент отличающихся
// пикселей и сравнивает структуру: классы, размеры блоков, активный пункт меню, шрифт.
//
// Запуск:   npm install && npx playwright install chromium && npm run check
// Настройка (переменные окружения):
//   BASE_URL        адрес приложения              (по умолчанию http://localhost:8080)
//   MOCKUPS_DIR     папка с макетами PAGES         (по умолчанию ../../PAGES)
//   OUT_DIR         куда писать результаты         (по умолчанию ./out)
//   ADMIN_EMAIL / ADMIN_PASSWORD  вход мастера     (по умолчанию admin@demo.com / Password123!)
//   --self-test     вместо страницы открывает тот же макет (проверка самого скрипта, ожидается 0%)
//   --only=name1,name2   прогнать только выбранные пары

import { chromium } from 'playwright';
import { PNG } from 'pngjs';
import pixelmatch from 'pixelmatch';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const BASE_URL = (process.env.BASE_URL || 'http://localhost:8080').replace(/\/$/, '');
const MOCKUPS_DIR = path.resolve(here, process.env.MOCKUPS_DIR || '../../PAGES');
const OUT_DIR = path.resolve(here, process.env.OUT_DIR || './out');
const ADMIN_EMAIL = process.env.ADMIN_EMAIL || 'admin@demo.com';
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD || 'Password123!';
const SELF_TEST = process.argv.includes('--self-test');
const ONLY = (process.argv.find(a => a.startsWith('--only=')) || '').replace('--only=', '').split(',').filter(Boolean);

// имя, макет, адрес страницы, нужен ли вход, особое действие
const PAIRS = [
  { name: 'public-1-home', mockup: 'public-1-home.html', url: '/' },
  { name: 'public-2-services', mockup: 'public-2-services.html', url: '/services' },
  { name: 'public-3-works', mockup: 'public-3-works.html', url: '/works' },
  { name: 'public-4-calendar', mockup: 'public-4-calendar.html', url: '/calendar' },
  { name: 'public-5-contacts', mockup: 'public-5-contacts.html', url: '/contacts' },
  { name: 'auth-login', mockup: 'auth-login.html', url: '/Account/Login' },
  { name: 'auth-login-error', mockup: 'auth-login-error.html', url: '/Account/Login', action: 'bad-login' },
  { name: 'panel-1-bookings', mockup: 'panel-1-bookings.html', url: '/Admin', admin: true },
  { name: 'panel-2-clients', mockup: 'panel-2-clients.html', url: '/Admin/Clients', admin: true },
  { name: 'panel-3-services', mockup: 'panel-3-services.html', url: '/Admin/Services', admin: true },
  { name: 'panel-4-requests', mockup: 'panel-4-requests.html', url: '/Admin/Requests', admin: true },
  { name: 'panel-5-showcase-reviews', mockup: 'panel-5-showcase-reviews.html', url: '/Admin/Showcase', admin: true },
  { name: 'panel-6-finance', mockup: 'panel-6-finance.html', url: '/Admin/Finance', admin: true },
  { name: 'panel-7-site-settings', mockup: 'panel-7-site-settings.html', url: '/Admin/Settings', admin: true },
  { name: 'panel-8-new-booking', mockup: 'panel-8-new-booking.html', url: '/Admin/Appointments/Create', admin: true }
].filter(p => ONLY.length === 0 || ONLY.includes(p.name));

const VIEWPORT = { width: 1280, height: 900 };

/** Снимает структуру страницы: геометрия классов, шрифт, активный пункт меню. */
const collectStructure = () => {
  const round = n => Math.round(n);
  const byClass = {};
  for (const el of document.querySelectorAll('body *')) {
    const style = getComputedStyle(el);
    if (style.display === 'none' || !el.getBoundingClientRect().width) continue;
    for (const cls of el.classList) {
      const r = el.getBoundingClientRect();
      (byClass[cls] ||= []).push({
        x: round(r.x), y: round(r.y + window.scrollY), w: round(r.width), h: round(r.height),
        tag: el.tagName.toLowerCase(), color: style.color, bg: style.backgroundColor,
        font: style.fontFamily.split(',')[0].replace(/["']/g, '').trim(), size: style.fontSize
      });
    }
  }
  const active = document.querySelector('aside .n.on, nav.s a.on');
  const body = getComputedStyle(document.body);
  const input = document.querySelector('input, textarea, select, button');
  return {
    bodyFont: body.fontFamily.split(',')[0].replace(/["']/g, '').trim(),
    inputFont: input ? getComputedStyle(input).fontFamily.split(',')[0].replace(/["']/g, '').trim() : null,
    fontLoaded: document.fonts ? [...document.fonts].some(f => f.family.replace(/["']/g, '') === 'Manrope' && f.status === 'loaded') : null,
    activeMenu: active ? active.textContent.trim() : null,
    menuItems: [...document.querySelectorAll('aside .n, nav.s a')].map(a => a.textContent.trim()),
    byClass,
    bootstrap: !![...document.styleSheets].find(s => s.href && /bootstrap/i.test(s.href)),
    placeholders: (document.body.innerText.match(/\[[A-ZА-ЯЁ][^\]\n]{1,40}\]/g) || [])
  };
};

/** Сравнивает структуру макета и страницы. */
function compareStructure(m, p) {
  const issues = [];
  if (m.bodyFont !== p.bodyFont) issues.push(`шрифт body: макет «${m.bodyFont}», страница «${p.bodyFont}»`);
  if (p.fontLoaded === false) issues.push('Manrope не загрузился на странице');
  if (p.inputFont && m.inputFont && m.inputFont !== p.inputFont) issues.push(`шрифт полей: макет «${m.inputFont}», страница «${p.inputFont}»`);
  if (p.bootstrap) issues.push('на странице подключён Bootstrap');
  if (m.activeMenu !== p.activeMenu) issues.push(`активный пункт меню: макет «${m.activeMenu}», страница «${p.activeMenu}»`);
  if (JSON.stringify(m.menuItems) !== JSON.stringify(p.menuItems)) issues.push(`пункты меню отличаются: макет [${m.menuItems.join(', ')}], страница [${p.menuItems.join(', ')}]`);
  if (p.placeholders.length) issues.push(`на странице остались заглушки в квадратных скобках: ${[...new Set(p.placeholders)].join(' ')}`);

  const TOL = 6; // допуск по размерам и положению, px
  for (const cls of Object.keys(m.byClass)) {
    const mm = m.byClass[cls];
    const pp = p.byClass[cls];
    if (!pp) { issues.push(`класс .${cls} есть в макете (${mm.length}), на странице отсутствует`); continue; }
    // блоки без текстовой зависимости: сравниваем по первому вхождению
    const a = mm[0], b = pp[0];
    const dw = Math.abs(a.w - b.w), dh = Math.abs(a.h - b.h), dx = Math.abs(a.x - b.x);
    // ширина/положение значимы всегда, высота — только для блоков, где контент не растёт от текста
    if (dw > TOL && !['.tbl', '.card'].includes('.' + cls)) issues.push(`.${cls}: ширина макет ${a.w}px, страница ${b.w}px`);
    if (dx > TOL && cls !== 'two') issues.push(`.${cls}: сдвиг по X макет ${a.x}px, страница ${b.x}px`);
    if (dh > TOL * 3 && ['t', 'btn', 'btn2', 'chip', 'logo', 'n', 'sl', 'tog', 'stat'].includes(cls)) issues.push(`.${cls}: высота макет ${a.h}px, страница ${b.h}px`);
    if (a.bg !== b.bg && ['btn', 'chip', 'card', 't'].includes(cls) && mm.length === pp.length) issues.push(`.${cls}: фон макет ${a.bg}, страница ${b.bg}`);
  }
  for (const cls of Object.keys(p.byClass)) {
    if (!m.byClass[cls]) issues.push(`класс .${cls} есть на странице (${p.byClass[cls].length}), в макете его нет`);
  }
  return issues;
}

function readPng(file) { return PNG.sync.read(fs.readFileSync(file)); }

function padTo(png, w, h) {
  if (png.width === w && png.height === h) return png;
  const out = new PNG({ width: w, height: h });
  out.data.fill(255);
  PNG.bitblt(png, out, 0, 0, png.width, png.height, 0, 0);
  return out;
}

function diffImages(fileA, fileB, outFile) {
  const a = readPng(fileA), b = readPng(fileB);
  const w = Math.max(a.width, b.width), h = Math.max(a.height, b.height);
  const A = padTo(a, w, h), B = padTo(b, w, h);
  const diff = new PNG({ width: w, height: h });
  const bad = pixelmatch(A.data, B.data, diff.data, w, h, { threshold: 0.1, includeAA: false });
  fs.writeFileSync(outFile, PNG.sync.write(diff));
  return { percent: +(bad / (w * h) * 100).toFixed(2), sizeA: [a.width, a.height], sizeB: [b.width, b.height] };
}

async function snap(page, file) {
  await page.evaluate(() => document.fonts && document.fonts.ready);
  await page.waitForTimeout(150);
  await page.screenshot({ path: file, fullPage: true });
}

async function login(context) {
  const page = await context.newPage();
  await page.goto(BASE_URL + '/Account/Login', { waitUntil: 'networkidle' });
  await page.fill('input[name="Input.Email"]', ADMIN_EMAIL);
  await page.fill('input[name="Input.Password"]', ADMIN_PASSWORD);
  await Promise.all([page.waitForNavigation({ waitUntil: 'networkidle' }), page.click('button[type=submit]')]);
  const ok = !page.url().includes('/Account/Login');
  await page.close();
  return ok;
}

async function main() {
  fs.mkdirSync(OUT_DIR, { recursive: true });
  const browser = await chromium.launch(process.env.CHROMIUM_PATH ? { executablePath: process.env.CHROMIUM_PATH } : {});
  const results = [];

  const publicCtx = await browser.newContext({ viewport: VIEWPORT, locale: 'ru-RU' });
  const adminCtx = await browser.newContext({ viewport: VIEWPORT, locale: 'ru-RU' });
  let adminOk = true;
  if (!SELF_TEST) {
    adminOk = await login(adminCtx).catch(() => false);
    if (!adminOk) console.warn('Не удалось войти мастером: страницы панели будут пропущены. Проверьте ADMIN_EMAIL / ADMIN_PASSWORD и запуск приложения.');
  }

  for (const pair of PAIRS) {
    const mockupFile = path.join(MOCKUPS_DIR, pair.mockup);
    const mockupShot = path.join(OUT_DIR, `mockup-${pair.name}.png`);
    const pageShot = path.join(OUT_DIR, `page-${pair.name}.png`);
    const diffShot = path.join(OUT_DIR, `diff-${pair.name}.png`);
    const row = { name: pair.name, url: pair.url };

    try {
      const mp = await publicCtx.newPage();
      await mp.goto(pathToFileURL(mockupFile).href, { waitUntil: 'load' });
      await snap(mp, mockupShot);
      const mStruct = await mp.evaluate(collectStructure);
      await mp.close();

      if (SELF_TEST) {
        const sp = await publicCtx.newPage();
        await sp.goto(pathToFileURL(mockupFile).href, { waitUntil: 'load' });
        await snap(sp, pageShot);
        var pStruct = await sp.evaluate(collectStructure);
        await sp.close();
      } else if (pair.admin && !adminOk) {
        row.skipped = 'нет входа мастером';
        results.push(row);
        continue;
      } else {
        const ctx = pair.admin ? adminCtx : publicCtx;
        const pg = await ctx.newPage();
        const resp = await pg.goto(BASE_URL + pair.url, { waitUntil: 'networkidle' });
        row.status = resp ? resp.status() : null;
        if (pair.action === 'bad-login') {
          await pg.fill('input[name="Input.Email"]', 'wrong@example.com');
          await pg.fill('input[name="Input.Password"]', 'wrong-password');
          await Promise.all([pg.waitForLoadState('networkidle'), pg.click('button[type=submit]')]);
        }
        await snap(pg, pageShot);
        var pStruct = await pg.evaluate(collectStructure);
        await pg.close();
      }

      Object.assign(row, diffImages(mockupShot, pageShot, diffShot));
      row.issues = compareStructure(mStruct, pStruct);
    } catch (e) {
      row.error = String(e.message || e);
    }
    results.push(row);
    const mark = row.error ? 'ОШИБКА' : row.skipped ? 'пропуск' : `${row.percent}%, замечаний: ${row.issues.length}`;
    console.log(`${pair.name.padEnd(28)} ${mark}`);
  }

  await browser.close();
  fs.writeFileSync(path.join(OUT_DIR, 'report.json'), JSON.stringify(results, null, 2));
  fs.writeFileSync(path.join(OUT_DIR, 'report.html'), renderHtml(results));
  console.log(`\nГотово. Отчёт: ${path.join(OUT_DIR, 'report.html')}`);
}

function renderHtml(results) {
  const esc = s => String(s).replace(/[&<>]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;' }[c]));
  const rows = results.map(r => `
<section>
  <h2>${esc(r.name)} <small>${esc(r.url)}${r.percent !== undefined ? ` · отличается ${r.percent}% пикселей` : ''}</small></h2>
  ${r.error ? `<p class="bad">Ошибка: ${esc(r.error)}</p>` : r.skipped ? `<p>Пропущено: ${esc(r.skipped)}</p>` : `
  <div class="pair"><figure><figcaption>Макет</figcaption><img src="mockup-${r.name}.png"></figure>
  <figure><figcaption>Страница</figcaption><img src="page-${r.name}.png"></figure>
  <figure><figcaption>Разница</figcaption><img src="diff-${r.name}.png"></figure></div>
  ${r.issues.length ? `<ul>${r.issues.map(i => `<li>${esc(i)}</li>`).join('')}</ul>` : '<p class="ok">Структурных расхождений не найдено</p>'}`}
</section>`).join('\n');
  return `<!doctype html><meta charset="utf-8"><title>Макет против страницы</title>
<style>body{font:14px system-ui;margin:24px;background:#fafafc;color:#1d1b26}h2 small{font-weight:400;color:#6a6277}
.pair{display:grid;grid-template-columns:repeat(3,1fr);gap:12px}img{width:100%;border:1px solid #e6e0ef}figcaption{font-weight:700;margin:4px 0}
.ok{color:#15553a}.bad{color:#7a2e12}section{margin-bottom:40px}</style>${rows}`;
}

main().catch(e => { console.error(e); process.exit(1); });
