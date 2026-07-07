#!/usr/bin/env node
// Headless Chrome + Chrome DevTools Protocol ile sayfa doğrulama.
//
// Playwright'in kendi chromium indirmesi bu ortamda çok yavaş kaldığı için
// (ağ ~80KB/s, tam indirme 30+ dakika), sistemde zaten kurulu olan
// `google-chrome` binary'sini `--headless=new --remote-debugging-port` ile
// başlatıp `chrome-remote-interface` (küçük, binary indirmeyen bir paket) ile
// CDP üzerinden sürüyoruz.
//
// ÖNEMLİ: DataTables gibi bazı kütüphaneler hatalarını console.error değil,
// native `alert()` ile gösteriyor. Bu yüzden Page.javascriptDialogOpening
// MUTLAKA dinlenmeli — aksi halde gerçek bir hata "temiz" diye raporlanır
// (bu script'in yazılma sebebi tam olarak bu kaçırılan hataydı).
//
// Kullanım:
//   cd scripts && npm install   (ilk sefer)
//   node browser-check.js http://localhost:5299/Cari
//
// Çıkış kodu: dialog/exception/console.error/4xx-5xx yakalanırsa 1, aksi halde 0.

const { spawn } = require('child_process');
const CDP = require('chrome-remote-interface');

const DEBUG_PORT = 9222;
const CHROME_BIN = process.env.CHROME_BIN || 'google-chrome';

function chromeAlreadyRunning() {
    return fetch(`http://127.0.0.1:${DEBUG_PORT}/json/version`).then(() => true).catch(() => false);
}

function launchChrome() {
    const child = spawn(
        CHROME_BIN,
        [
            '--headless=new',
            '--no-sandbox',
            '--disable-gpu',
            `--remote-debugging-port=${DEBUG_PORT}`,
            '--remote-debugging-address=127.0.0.1',
            '--user-data-dir=/tmp/sakaryaerp-browser-check-profile',
            'about:blank',
        ],
        { stdio: 'ignore', detached: true }
    );
    child.unref();
}

async function waitForChrome(timeoutMs = 8000) {
    const start = Date.now();
    while (Date.now() - start < timeoutMs) {
        if (await chromeAlreadyRunning()) return;
        await new Promise((r) => setTimeout(r, 200));
    }
    throw new Error('Chrome remote debugging portu açılmadı (timeout).');
}

async function checkPage(url) {
    const problems = [];
    let client, target;
    try {
        target = await CDP.New({ port: DEBUG_PORT, url: 'about:blank' });
        client = await CDP({ port: DEBUG_PORT, target });
        const { Page, Runtime, Network } = client;

        Runtime.consoleAPICalled((params) => {
            if (params.type === 'error' || params.type === 'warning') {
                const args = params.args.map((a) => (a.value !== undefined ? a.value : a.description)).join(' ');
                problems.push(`[console.${params.type}] ${args}`);
            }
        });
        Runtime.exceptionThrown((params) => {
            const desc = params.exceptionDetails.exception?.description || JSON.stringify(params.exceptionDetails);
            problems.push(`[exception] ${desc}`);
        });
        Page.javascriptDialogOpening((params) => {
            problems.push(`[dialog.${params.type}] ${params.message}`);
            Page.handleJavaScriptDialog({ accept: true }).catch(() => {});
        });
        Network.responseReceived((params) => {
            if (params.response.status >= 400) {
                problems.push(`[http-${params.response.status}] ${params.response.url}`);
            }
        });

        await Runtime.enable();
        await Page.enable();
        await Network.enable();
        await Page.navigate({ url });
        await Page.loadEventFired();
        await new Promise((r) => setTimeout(r, 2000));
    } finally {
        if (client) await client.close().catch(() => {});
        if (target) await CDP.Close({ port: DEBUG_PORT, id: target.id }).catch(() => {});
    }
    return problems;
}

(async () => {
    const url = process.argv[2];
    if (!url) {
        console.error('Kullanım: node browser-check.js <url> [<url2> ...]');
        process.exit(2);
    }
    const urls = process.argv.slice(2);

    if (!(await chromeAlreadyRunning())) {
        launchChrome();
        await waitForChrome();
    }

    let hadProblem = false;
    for (const u of urls) {
        console.log(`\n=== ${u} ===`);
        const problems = await checkPage(u);
        if (problems.length === 0) {
            console.log('Temiz — console/exception/dialog/http hatası yok.');
        } else {
            hadProblem = true;
            problems.forEach((p) => console.log(p));
        }
    }
    process.exit(hadProblem ? 1 : 0);
})();
