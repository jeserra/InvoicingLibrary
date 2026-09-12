let sessionId = null;

function setStepState(step, state) {
  const pill = document.querySelector(`.step-pill[data-step="${step}"]`);
  pill.classList.remove('active', 'done');
  if (state) pill.classList.add(state);
}

function showPanel(step) {
  document.getElementById(`panel-${step}`).hidden = false;
  setStepState(step, 'active');
}

function showMessage(elId, text, isError) {
  const el = document.getElementById(elId);
  el.textContent = text;
  el.className = 'msg ' + (isError ? 'error' : 'ok');
}

// ---- Paso 1: generar catalogo ----------------------------------------------------

document.getElementById('btn-generate').addEventListener('click', async () => {
  const industria = document.getElementById('industria').value;
  const res = await fetch('/api/catalog/generate', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ industria }),
  });

  if (!res.ok) {
    showMessage('msg-1', await res.text(), true);
    return;
  }

  const data = await res.json();
  sessionId = data.sessionId;
  showMessage('msg-1', `Catalogo generado (sesion ${sessionId.slice(0, 8)}...).`, false);

  const tbody = document.querySelector('#table-resumen tbody');
  tbody.innerHTML = '';
  for (const row of data.resumen) {
    const tr = document.createElement('tr');
    tr.innerHTML = `<td>${row.catalogo}</td><td>${row.conservadas}</td><td>${row.total}</td>`;
    tbody.appendChild(tr);
  }
  document.getElementById('table-resumen').hidden = false;

  setStepState(1, 'done');
  showPanel(2);
});

// ---- Paso 2: emisor + certificado -------------------------------------------------

document.getElementById('use-test-csd').addEventListener('change', (e) => {
  document.getElementById('csd-upload').hidden = e.target.checked;
});

document.getElementById('btn-emisor').addEventListener('click', async () => {
  const form = new FormData();
  form.append('sessionId', sessionId);
  form.append('rfc', document.getElementById('emisor-rfc').value);
  form.append('nombre', document.getElementById('emisor-nombre').value);
  form.append('regimenFiscal', document.getElementById('emisor-regimen').value);
  form.append('lugarExpedicion', document.getElementById('emisor-lugar').value);

  const useTestCsd = document.getElementById('use-test-csd').checked;
  form.append('useTestCsd', useTestCsd ? 'true' : 'false');

  if (!useTestCsd) {
    const cerFile = document.getElementById('cer-file').files[0];
    const keyFile = document.getElementById('key-file').files[0];
    if (!cerFile || !keyFile) {
      showMessage('msg-2', 'Suba el archivo .cer y .key, o marque "usar CSD de pruebas".', true);
      return;
    }
    form.append('cer', cerFile);
    form.append('key', keyFile);
    form.append('pwd', document.getElementById('csd-pwd').value);
  }

  const res = await fetch('/api/emisor', { method: 'POST', body: form });
  if (!res.ok) {
    showMessage('msg-2', await res.text(), true);
    return;
  }

  const data = await res.json();
  showMessage('msg-2', `Certificado cargado. No. de certificado: ${data.noCertificado}.`, false);
  setStepState(2, 'done');
  showPanel(3);
});

// ---- Paso 3: constancia fiscal + concepto -----------------------------------------

document.getElementById('btn-csf').addEventListener('click', async () => {
  const pdfFile = document.getElementById('csf-pdf').files[0];
  if (!pdfFile) {
    showMessage('msg-csf', 'Suba un PDF de Constancia de Situacion Fiscal.', true);
    return;
  }

  const form = new FormData();
  form.append('sessionId', sessionId);
  form.append('usoCFDI', document.getElementById('csf-usocfdi').value);
  form.append('pdf', pdfFile);

  const res = await fetch('/api/receptor/csf', { method: 'POST', body: form });
  if (!res.ok) {
    showMessage('msg-csf', await res.text(), true);
    return;
  }

  const data = await res.json();
  document.getElementById('rec-rfc').value = data.rfc || '';
  document.getElementById('rec-nombre').value = data.nombre || '';
  document.getElementById('rec-cp').value = data.domicilioFiscalReceptor || '';
  document.getElementById('rec-regimen').value = data.regimenFiscalReceptor || '';

  showMessage('msg-csf', data.advertencia || 'Constancia leida - revise los campos antes de continuar.', !!data.advertencia);
});

function recalcularTotal() {
  const cantidad = parseFloat(document.getElementById('concepto-cantidad').value) || 0;
  const precio = parseFloat(document.getElementById('concepto-preciounitario').value) || 0;
  const importe = cantidad * precio;
  const total = importe * 1.16;
  document.getElementById('concepto-total').value = total.toFixed(2);
}
document.getElementById('concepto-cantidad').addEventListener('input', recalcularTotal);
document.getElementById('concepto-preciounitario').addEventListener('input', recalcularTotal);
recalcularTotal();

function setupAutocomplete(inputId, suggestId, catalogo) {
  const input = document.getElementById(inputId);
  const box = document.getElementById(suggestId);
  let debounceHandle = null;

  input.addEventListener('input', () => {
    clearTimeout(debounceHandle);
    const prefix = input.value.trim();
    if (!sessionId || prefix.length === 0) {
      box.hidden = true;
      return;
    }
    debounceHandle = setTimeout(async () => {
      const res = await fetch(`/api/catalog/search?sessionId=${sessionId}&catalogo=${catalogo}&prefix=${encodeURIComponent(prefix)}`);
      if (!res.ok) return;
      const matches = await res.json();
      box.innerHTML = '';
      for (const m of matches) {
        const div = document.createElement('div');
        div.textContent = m.descripcion ? `${m.codigo} - ${m.descripcion}` : m.codigo;
        div.addEventListener('click', () => {
          input.value = m.codigo;
          box.hidden = true;
        });
        box.appendChild(div);
      }
      box.hidden = matches.length === 0;
    }, 200);
  });

  document.addEventListener('click', (e) => {
    if (e.target !== input) box.hidden = true;
  });
}
setupAutocomplete('concepto-claveprodserv', 'suggest-prodserv', 'ClaveProdServ');
setupAutocomplete('concepto-claveunidad', 'suggest-unidad', 'ClaveUnidad');

// Avanzar de paso 3 a paso 4 sin llamada al servidor - solo revela el panel.
document.getElementById('panel-3').insertAdjacentHTML('beforeend', '<button id="btn-continue-3" class="secondary">Continuar a sellar</button>');
document.getElementById('btn-continue-3').addEventListener('click', () => {
  setStepState(3, 'done');
  showPanel(4);
});

// ---- Paso 4: sellar -----------------------------------------------------------------

document.getElementById('btn-seal').addEventListener('click', async () => {
  const body = {
    sessionId,
    lugarExpedicion: document.getElementById('emisor-lugar').value,
    receptor: {
      rfc: document.getElementById('rec-rfc').value,
      nombre: document.getElementById('rec-nombre').value,
      domicilioFiscalReceptor: document.getElementById('rec-cp').value,
      regimenFiscalReceptor: document.getElementById('rec-regimen').value,
      usoCFDI: document.getElementById('csf-usocfdi').value,
    },
    concepto: {
      claveProdServ: document.getElementById('concepto-claveprodserv').value,
      claveUnidad: document.getElementById('concepto-claveunidad').value,
      descripcion: document.getElementById('concepto-descripcion').value,
      cantidad: parseFloat(document.getElementById('concepto-cantidad').value) || 0,
      valorUnitario: parseFloat(document.getElementById('concepto-preciounitario').value) || 0,
    },
  };

  const res = await fetch('/api/invoice/seal', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    showMessage('msg-4', await res.text(), true);
    return;
  }

  const data = await res.json();
  document.getElementById('xml-preview').textContent = data.xml;
  document.getElementById('xml-result').hidden = false;
  showMessage('msg-4', '', false);
  setStepState(4, 'done');
});

document.getElementById('btn-download').addEventListener('click', () => {
  window.location.href = `/api/invoice/download?sessionId=${sessionId}`;
});

document.getElementById('btn-download-pdf').addEventListener('click', () => {
  window.location.href = `/api/invoice/pdf?sessionId=${sessionId}`;
});

setStepState(1, 'active');
