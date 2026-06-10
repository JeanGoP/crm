import { spawn } from 'node:child_process';
import { mkdir, writeFile } from 'node:fs/promises';
import { join, resolve } from 'node:path';

const chromePath = process.env.CHROME_PATH
  ?? 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const baseUrl = process.env.CRM_MANUAL_URL ?? 'https://crmmontelibano.netlify.app';
const email = process.env.CRM_MANUAL_EMAIL;
const password = process.env.CRM_MANUAL_PASSWORD;
const tenant = process.env.CRM_MANUAL_TENANT ?? 'demo';
const outDir = resolve('docs/assets/manual');
const port = Number(process.env.CRM_MANUAL_DEBUG_PORT ?? 9333);
const profileDir = resolve('C:/tmp', `crm-manual-chrome-${Date.now()}`);

if (!email || !password) {
  throw new Error('Defina CRM_MANUAL_EMAIL y CRM_MANUAL_PASSWORD para iniciar sesion.');
}

await mkdir(outDir, { recursive: true });

const chrome = spawn(chromePath, [
  `--remote-debugging-port=${port}`,
  '--remote-debugging-address=127.0.0.1',
  '--headless=new',
  '--disable-gpu',
  '--disable-dev-shm-usage',
  '--no-first-run',
  '--no-default-browser-check',
  '--hide-scrollbars',
  '--window-size=1440,1100',
  `--user-data-dir=${profileDir}`,
  'about:blank'
], { stdio: ['ignore', 'ignore', 'pipe'] });

let chromeErrors = '';
chrome.stderr.on('data', (chunk) => {
  chromeErrors += chunk.toString();
});

const sleep = (ms) => new Promise((resolveSleep) => setTimeout(resolveSleep, ms));

async function getJson(url, attempts = 40) {
  let lastError;
  for (let i = 0; i < attempts; i += 1) {
    try {
      const response = await fetch(url);
      if (response.ok) return response.json();
      lastError = new Error(`HTTP ${response.status}`);
    } catch (error) {
      lastError = error;
    }
    await sleep(250);
  }
  throw new Error(`${lastError?.message ?? 'No fue posible conectar con Chrome'}\n${chromeErrors.slice(-1200)}`);
}

function connect(wsUrl) {
  const ws = new WebSocket(wsUrl);
  let id = 0;
  const pending = new Map();

  ws.addEventListener('message', (event) => {
    const payload = JSON.parse(event.data);
    if (payload.id && pending.has(payload.id)) {
      const { resolve: resolvePending, reject } = pending.get(payload.id);
      pending.delete(payload.id);
      if (payload.error) reject(new Error(payload.error.message));
      else resolvePending(payload.result);
    }
  });

  const opened = new Promise((resolveOpen, rejectOpen) => {
    ws.addEventListener('open', resolveOpen, { once: true });
    ws.addEventListener('error', rejectOpen, { once: true });
  });

  return {
    opened,
    send(method, params = {}) {
      const callId = id += 1;
      ws.send(JSON.stringify({ id: callId, method, params }));
      return new Promise((resolveCall, rejectCall) => {
        pending.set(callId, { resolve: resolveCall, reject: rejectCall });
      });
    },
    close() {
      ws.close();
    }
  };
}

async function evaluate(client, expression) {
  const result = await client.send('Runtime.evaluate', {
    expression,
    awaitPromise: true,
    returnByValue: true
  });
  if (result.exceptionDetails) {
    throw new Error(result.exceptionDetails.text ?? 'Error evaluando la pagina.');
  }
  return result.result.value;
}

async function navigate(client, path) {
  await client.send('Page.navigate', { url: `${baseUrl}${path}` });
  await sleep(2600);
  await evaluate(client, `document.fonts ? document.fonts.ready.then(() => true) : true`);
  await sleep(500);
}

async function screenshot(client, fileName) {
  await evaluate(client, `window.scrollTo(0, 0); true`);
  await sleep(300);
  const result = await client.send('Page.captureScreenshot', {
    format: 'png',
    captureBeyondViewport: true,
    fromSurface: true
  });
  await writeFile(join(outDir, fileName), Buffer.from(result.data, 'base64'));
}

async function clickButton(client, label) {
  await evaluate(client, `
    (() => {
      const wanted = ${JSON.stringify(label)}.toLowerCase();
      const buttons = [...document.querySelectorAll('button, a')];
      const match = buttons.find((button) => (button.innerText || button.textContent || '').trim().toLowerCase().includes(wanted));
      if (!match) return false;
      match.click();
      return true;
    })()
  `);
  await sleep(900);
}

async function neutralizeInternalLabels(client) {
  await evaluate(client, `
    (() => {
      const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
      const replacements = [
        ['Tabla Montelibano', 'Tabla financiera'],
        ['Usar tabla Montelibano en cotizaciones', 'Usar tabla financiera en cotizaciones']
      ];
      while (walker.nextNode()) {
        for (const [from, to] of replacements) {
          walker.currentNode.nodeValue = walker.currentNode.nodeValue.replaceAll(from, to);
        }
      }
      return true;
    })()
  `);
}

async function firstCustomerId(client) {
  return evaluate(client, `
    fetch('/crmapi-proxy/api/customers', {
      headers: {
        'X-Tenant': ${JSON.stringify(tenant)},
        'Authorization': 'Bearer ' + JSON.parse(localStorage.getItem('crm-session')).accessToken
      }
    })
      .then((response) => response.ok ? response.json() : [])
      .then((rows) => rows && rows.length ? rows[0].id : null)
  `);
}

try {
  const tabs = await getJson(`http://127.0.0.1:${port}/json`);
  const page = tabs.find((item) => item.type === 'page') ?? tabs[0];
  const client = connect(page.webSocketDebuggerUrl);
  await client.opened;
  await client.send('Page.enable');
  await client.send('Runtime.enable');
  await client.send('Emulation.setDeviceMetricsOverride', {
    width: 1440,
    height: 1100,
    deviceScaleFactor: 1,
    mobile: false
  });

  await navigate(client, '/login');
  await screenshot(client, '01-login.png');

  const session = await evaluate(client, `
    fetch('/crmapi-proxy/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'X-Tenant': ${JSON.stringify(tenant)} },
      body: JSON.stringify({
        email: ${JSON.stringify(email)},
        password: ${JSON.stringify(password)},
        tenant: ${JSON.stringify(tenant)}
      })
    }).then(async (response) => {
      if (!response.ok) throw new Error(await response.text());
      return response.json();
    })
  `);

  await evaluate(client, `
    localStorage.setItem('crm-session', ${JSON.stringify(JSON.stringify({
      accessToken: session.accessToken,
      refreshToken: session.refreshToken,
      user: session.user
    }))});
    true
  `);

  await navigate(client, '/');
  await screenshot(client, '02-dashboard.png');

  await navigate(client, '/clientes');
  await screenshot(client, '03-clientes.png');

  const customerId = await firstCustomerId(client);
  if (customerId) {
    await navigate(client, `/clientes/${customerId}`);
    await screenshot(client, '03-clientes-360.png');
  }

  await navigate(client, '/productos');
  await screenshot(client, '04-productos.png');

  await navigate(client, '/cotizaciones');
  await screenshot(client, '05-cotizaciones.png');
  await clickButton(client, 'Nueva cotizacion');
  await screenshot(client, '05-cotizaciones-simulador.png');

  await navigate(client, '/solicitudes-credito');
  await screenshot(client, '06-solicitudes-credito.png');

  await navigate(client, '/prospectos');
  await screenshot(client, '07-prospectos.png');

  await navigate(client, '/pipeline');
  await screenshot(client, '08-pipeline.png');

  await navigate(client, '/actividades');
  await screenshot(client, '09-actividades.png');

  await navigate(client, '/configuracion');
  await neutralizeInternalLabels(client);
  await screenshot(client, '10-configuracion.png');

  await navigate(client, '/reportes');
  await screenshot(client, '11-reportes.png');

  client.close();
} finally {
  chrome.kill();
}
