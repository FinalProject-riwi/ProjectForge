// ── ProjectForge Wizard ───────────────────────────────────────────────────────

const state = {
  currentStep: 1,
  totalSteps: 5,
  architecture: null,
  framework: null,
  frameworkVersion: null,
  database: null,
  infrastructure: 'None',
  patterns: [],
  libraries: [],
  vpsRows: 0,
};

// ── Navigation ────────────────────────────────────────────────────────────────

async function nextStep() {
  if (!validateStep(state.currentStep)) return;
  if (state.currentStep === 2) await loadStep4Data();
  if (state.currentStep === 4) buildSummary();
  state.currentStep++;
  renderStep();
}

function prevStep() {
  if (state.currentStep > 1) { state.currentStep--; renderStep(); }
}

function renderStep() {
  document.querySelectorAll('.wizard-step').forEach(s => s.classList.remove('active'));
  document.querySelector(`.wizard-step[data-step="${state.currentStep}"]`)?.classList.add('active');

  document.querySelectorAll('.progress-step').forEach((el, i) => {
    const n = i + 1;
    el.classList.toggle('active', n === state.currentStep);
    el.classList.toggle('done',   n < state.currentStep);
  });

  const fill = ((state.currentStep - 1) / (state.totalSteps - 1)) * 100;
  document.getElementById('progress-fill').style.width = fill + '%';

  document.getElementById('btn-prev').style.display   = state.currentStep > 1 ? 'flex' : 'none';
  document.getElementById('btn-next').style.display   = state.currentStep < state.totalSteps ? 'flex' : 'none';
  document.getElementById('btn-submit').style.display = state.currentStep === state.totalSteps ? 'flex' : 'none';
}

// ── Validation ────────────────────────────────────────────────────────────────

function validateStep(step) {
  if (step === 1) {
    if (!state.architecture) { showError('Selecciona una arquitectura'); return false; }
  }
  if (step === 2) {
    if (!state.framework) { showError('Selecciona un framework'); return false; }
    if (!state.database)  { showError('Selecciona una base de datos'); return false; }
  }
  return true;
}

function showError(msg) {
  document.querySelector('.toast-error')?.remove();
  const t = document.createElement('div');
  t.className = 'toast-error';
  t.textContent = msg;
  t.style.cssText = 'position:fixed;bottom:2rem;right:2rem;background:#ef4444;color:#fff;padding:.75rem 1.25rem;border-radius:8px;z-index:9999;font-size:.9rem;box-shadow:0 4px 12px rgba(0,0,0,.3)';
  document.body.appendChild(t);
  setTimeout(() => t.remove(), 3500);
}

// ── Step 1: Architecture ──────────────────────────────────────────────────────

document.querySelectorAll('.arch-card').forEach(card => {
  card.addEventListener('click', () => {
    document.querySelectorAll('.arch-card').forEach(c => c.classList.remove('selected'));
    card.classList.add('selected');
    card.querySelector('input').checked = true;
    state.architecture = card.dataset.value;
<<<<<<< HEAD
    // Cargar frameworks para el step 2 en anticipación
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
    loadFrameworks(state.architecture);
  });
});

// ── Step 2: Framework ─────────────────────────────────────────────────────────

async function loadFrameworks(arch) {
  if (!arch) return;
  try {
    const resp = await fetch(`/wizard/api/frameworks/${arch}`);
    if (!resp.ok) throw new Error('HTTP ' + resp.status);
    const options = await resp.json();
    renderFrameworkOptions(options);
  } catch (e) {
    console.error('Error cargando frameworks:', e);
  }
}

function renderFrameworkOptions(options) {
  const container = document.getElementById('framework-options');
  if (!container) return;

<<<<<<< HEAD
  // Limpiar selección previa
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
  state.framework = null;
  state.frameworkVersion = null;

  container.innerHTML = options.map(opt => `
    <div class="db-card framework-option" data-value="${opt.value}" data-versions='${JSON.stringify(opt.availableVersions)}'>
      <strong>${opt.label}</strong>
    </div>
  `).join('');

<<<<<<< HEAD
  // Registrar listeners en los elementos recién creados
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
  container.querySelectorAll('.framework-option').forEach(card => {
    card.addEventListener('click', () => {
      container.querySelectorAll('.framework-option').forEach(c => c.classList.remove('selected'));
      card.classList.add('selected');
      const versions = JSON.parse(card.dataset.versions || '[]');
      state.framework = card.dataset.value;
      state.frameworkVersion = versions[0] ?? '';

      const sel = document.getElementById('framework-version-select');
      sel.innerHTML = versions.map(v => `<option value="${v}">${v}</option>`).join('');
    });
  });
}

document.getElementById('framework-version-select')?.addEventListener('change', e => {
  state.frameworkVersion = e.target.value;
});

// ── Step 2: Database ──────────────────────────────────────────────────────────

<<<<<<< HEAD
// Delegación de eventos en el contenedor padre — funciona aunque el DOM cambie
document.querySelector('.db-grid')?.addEventListener('click', e => {
  const card = e.target.closest('.db-card');
  if (!card || card.closest('#framework-options')) return; // ignorar clicks en frameworks
=======
document.querySelector('.db-grid')?.addEventListener('click', e => {
  const card = e.target.closest('.db-card');
  if (!card || card.closest('#framework-options')) return;
>>>>>>> 0dc2a35 (complete java,python,typescript)
  document.querySelectorAll('.db-grid .db-card').forEach(c => c.classList.remove('selected'));
  card.classList.add('selected');
  state.database = card.dataset.value;
});

// ── Step 3: Infrastructure ────────────────────────────────────────────────────

document.querySelectorAll('.infra-card').forEach(card => {
  card.addEventListener('click', () => {
    document.querySelectorAll('.infra-card').forEach(c => c.classList.remove('selected'));
    card.classList.add('selected');
    state.infrastructure = card.dataset.value;
    document.getElementById('vps-section').style.display =
      state.infrastructure === 'Kubernetes' ? 'block' : 'none';
  });
});

function addVpsRow() {
  state.vpsRows++;
  const i = state.vpsRows;
  const row = document.createElement('div');
  row.className = 'vps-row'; row.id = `vps-row-${i}`;
  row.innerHTML = `
    <input class="form-input" placeholder="Label (ej: master-1)" />
    <input class="form-input" placeholder="IP / Hostname" />
    <input class="form-input" placeholder="22" value="22" style="width:70px" />
    <input class="form-input" placeholder="Usuario" />
    <input class="form-input" type="password" placeholder="Contraseña" />
    <select class="form-select" style="width:100px">
      <option value="master">Master</option>
      <option value="worker" selected>Worker</option>
    </select>
    <button type="button" class="btn btn-ghost btn-sm" onclick="this.closest('.vps-row').remove()">✕</button>
  `;
  document.getElementById('vps-list').appendChild(row);
}

<<<<<<< HEAD
// ── Step 4: AI Suggestions ────────────────────────────────────────────────────
=======
// ── Step 4: AI Suggestions + fallback BD ─────────────────────────────────────
>>>>>>> 0dc2a35 (complete java,python,typescript)

async function loadStep4Data() {
  if (!state.architecture || !state.framework) return;
  const banner = document.getElementById('ai-rationale');
  if (banner) banner.textContent = 'Consultando IA para sugerencias personalizadas...';

  try {
    const resp = await fetch('/wizard/api/suggest', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        architecture: state.architecture,
        framework: state.framework,
        database: state.database,
        infrastructure: state.infrastructure,
        alreadySelectedPatterns: []
      })
    });
<<<<<<< HEAD
    const data = await resp.json();
    if (banner) banner.textContent = data.rationale || '';
    renderPatterns(data.suggestedPatterns || []);
    renderLibraries(data.suggestedLibraries || []);
  } catch {
    if (banner) banner.textContent = 'No se pudieron cargar sugerencias de IA. Selecciona manualmente.';
    renderPatterns([]);
=======
    if (!resp.ok) throw new Error('HTTP ' + resp.status);
    const data = await resp.json();

    // Si la IA falló (ok=false), mostramos el motivo real y caemos a la BD.
    if (data.ok === false) {
      if (banner) banner.textContent = data.rationale || ('Sugerencias IA no disponibles. Mostrando opciones para ' + state.architecture + '.');
      await loadPatternsFromDb();
      await loadLibrariesFromDb();
      return;
    }

    if (banner) banner.textContent = data.rationale || '';
    await renderPatternsFromAI(data.suggestedPatterns || []);
    renderLibraries(data.suggestedLibraries || []);
  } catch {
    if (banner) banner.textContent = 'Sugerencias IA no disponibles. Mostrando opciones para ' + state.architecture + '.';
    await loadPatternsFromDb();
    await loadLibrariesFromDb();
  }
}

async function loadPatternsFromDb() {
  try {
    const resp = await fetch(`/wizard/api/patterns/${state.architecture}`);
    if (!resp.ok) throw new Error('HTTP ' + resp.status);
    const patterns = await resp.json();
    renderPatternsFromDb(patterns);
  } catch {
    renderPatternsFromDb([]);
  }
}

function renderPatternsFromDb(patterns) {
  const container = document.getElementById('patterns-list');
  if (!patterns.length) {
    container.innerHTML = '<p style="color:var(--text-muted);font-size:.85rem">Sin patrones disponibles para esta arquitectura.</p>';
    state.patterns = [];
    return;
  }
  container.innerHTML = patterns.map(p => `<label title="${p.description || ''}">
    <input type="radio" name="design-pattern" value="${p.name}"
           onchange="selectSingle('patterns','${p.name.replace(/'/g, "\\'")}')" />
    ${p.name}
  </label>`).join('');
  state.patterns = [];
}

async function renderPatternsFromAI(suggested) {
  // Load architecture-specific patterns from DB, then mark AI-suggested ones
  try {
    const resp = await fetch(`/wizard/api/patterns/${state.architecture}`);
    if (!resp.ok) throw new Error('HTTP ' + resp.status);
    const dbPatterns = await resp.json();
    const dbNames = dbPatterns.map(p => p.name);

    // Merge: DB patterns + any AI-suggested ones not in DB
    const allNames = [...new Set([...dbNames, ...suggested])];
    // Selección única: solo marcamos el PRIMER patrón sugerido por la IA.
    const firstSuggested = suggested.length ? suggested[0] : null;

    document.getElementById('patterns-list').innerHTML = allNames.map(name => {
      const isAi = suggested.includes(name);
      const checked = name === firstSuggested;
      return `<label class="${isAi ? 'suggested' : ''}">
        <input type="radio" name="design-pattern" value="${name}" ${checked ? 'checked' : ''}
               onchange="selectSingle('patterns','${name.replace(/'/g, "\\'")}')" />
        ${name} ${isAi ? '<span style="font-size:.7rem;color:#8b5cf6">✦ IA</span>' : ''}
      </label>`;
    }).join('');
    state.patterns = firstSuggested ? [firstSuggested] : [];
  } catch {
    // Fallback: show only AI suggestions
    renderPatternsLegacy(suggested);
  }
}

function renderPatternsLegacy(suggested) {
  const all = ['Repository', 'CQRS', 'Clean Architecture', 'Mediator', 'DDD', 'Hexagonal Architecture', 'Microservices', 'Event Sourcing', 'Saga'];
  const firstSuggested = suggested.length ? suggested[0] : null;
  document.getElementById('patterns-list').innerHTML = all.map(p => {
    const isAi = suggested.includes(p);
    const checked = p === firstSuggested;
    return `<label class="${isAi ? 'suggested' : ''}">
      <input type="radio" name="design-pattern" value="${p}" ${checked ? 'checked' : ''}
             onchange="selectSingle('patterns','${p}')" />
      ${p} ${isAi ? '<span style="font-size:.7rem;color:#8b5cf6">✦ IA</span>' : ''}
    </label>`;
  }).join('');
  state.patterns = firstSuggested ? [firstSuggested] : [];
}

async function loadLibrariesFromDb() {
  try {
    const resp = await fetch(`/wizard/api/libraries/${state.architecture}`);
    if (!resp.ok) throw new Error('HTTP ' + resp.status);
    const libs = await resp.json();
    renderLibrariesFromDb(libs);
  } catch {
>>>>>>> 0dc2a35 (complete java,python,typescript)
    renderLibraries([]);
  }
}

<<<<<<< HEAD
function renderPatterns(suggested) {
  const all = ['Repository', 'CQRS', 'Clean Architecture', 'Mediator', 'DDD', 'Hexagonal Architecture', 'Microservices', 'Event Sourcing', 'Saga'];
  document.getElementById('patterns-list').innerHTML = all.map(p => {
    const ok = suggested.includes(p);
    return `<label class="${ok ? 'suggested' : ''}">
      <input type="checkbox" value="${p}" ${ok ? 'checked' : ''}
             onchange="toggleSel('patterns','${p}',this.checked)" />
      ${p} ${ok ? '<span style="font-size:.7rem;color:#8b5cf6">✦ IA</span>' : ''}
    </label>`;
  }).join('');
  state.patterns = [...suggested];
=======
function renderLibrariesFromDb(libs) {
  const container = document.getElementById('libraries-list');
  container.innerHTML = libs.length
    ? libs.map(l => `<label title="${l.description || ''}">
        <input type="checkbox" value="${l.packageName}"
               onchange="toggleSel('libraries','${l.packageName}',this.checked)" />
        ${l.name} <span style="font-size:.7rem;color:var(--text-muted)">${l.category}</span>
      </label>`).join('')
    : '<p style="color:var(--text-muted);font-size:.85rem">Sin librerías disponibles para esta arquitectura.</p>';
  state.libraries = [];
>>>>>>> 0dc2a35 (complete java,python,typescript)
}

function renderLibraries(suggested) {
  const libs = [...new Set(suggested)];
  document.getElementById('libraries-list').innerHTML = libs.length
    ? libs.map(l => `<label class="suggested">
        <input type="checkbox" value="${l}" checked onchange="toggleSel('libraries','${l}',this.checked)" />
        ${l} <span style="font-size:.7rem;color:#8b5cf6">✦ IA</span>
      </label>`).join('')
    : '<p style="color:var(--text-muted);font-size:.85rem">Sin sugerencias de librerías disponibles.</p>';
  state.libraries = [...libs];
}

function toggleSel(key, value, checked) {
  if (checked) { if (!state[key].includes(value)) state[key].push(value); }
  else { state[key] = state[key].filter(v => v !== value); }
}

<<<<<<< HEAD
=======
// Selección ÚNICA para patrones de diseño: solo un patrón a la vez.
function selectSingle(key, value) {
  state[key] = [value];
}

>>>>>>> 0dc2a35 (complete java,python,typescript)
// ── Step 5: Summary ───────────────────────────────────────────────────────────

function buildSummary() {
  const el = document.getElementById('config-summary');
  if (!el) return;
  el.innerHTML = [
<<<<<<< HEAD
    ['Arquitectura', state.architecture],
    ['Framework',    state.framework + (state.frameworkVersion ? ' ' + state.frameworkVersion : '')],
    ['Base de Datos', state.database],
    ['Infraestructura', state.infrastructure],
    ['Patrones', state.patterns.join(', ') || 'Ninguno'],
    ['Librerías', state.libraries.slice(0, 5).join(', ') + (state.libraries.length > 5 ? '…' : '') || 'Ninguna'],
=======
    ['Arquitectura',   state.architecture],
    ['Framework',      state.framework + (state.frameworkVersion ? ' ' + state.frameworkVersion : '')],
    ['Base de Datos',  state.database],
    ['Infraestructura',state.infrastructure],
    ['Patrones',       state.patterns.join(', ') || 'Ninguno'],
    ['Librerías',      state.libraries.slice(0, 5).join(', ') + (state.libraries.length > 5 ? '…' : '') || 'Ninguna'],
>>>>>>> 0dc2a35 (complete java,python,typescript)
  ].map(([label, value]) => `
    <div class="summary-item">
      <label>${label}</label>
      <div class="value">${value ?? '—'}</div>
    </div>`).join('');
}

// ── Form Submit ───────────────────────────────────────────────────────────────

document.getElementById('wizard-form').addEventListener('submit', function(e) {
  e.preventDefault();
  const name = document.getElementById('project-name')?.value?.trim();
  if (!name) { showError('Ingresa un nombre para el proyecto'); return; }

  const btn = document.getElementById('btn-submit');
  btn.disabled = true;
  btn.textContent = 'Creando proyecto...';

<<<<<<< HEAD
  // Inyectamos los campos del estado del wizard como hidden inputs y hacemos
  // un submit nativo del form. Esto garantiza que:
  //   1. El antiforgery token ya está en el form (puesto por @Html.AntiForgeryToken())
  //   2. El browser maneja los redirects correctamente (no fetch)
  //   3. No hay problemas con SameSite cookies ni CORS
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
  const form = e.target;
  form.method = 'POST';
  form.action = '/wizard/step5';

  function setHidden(name, value) {
    let el = form.querySelector('[name="' + name + '"][data-wizard-state]');
    if (!el) {
      el = document.createElement('input');
      el.type = 'hidden';
      el.name = name;
      el.setAttribute('data-wizard-state', '1');
      form.appendChild(el);
    }
    el.value = value;
  }

  setHidden('__arch',     state.architecture ?? '');
  setHidden('__fw',       state.framework ?? '');
  setHidden('__fwv',      state.frameworkVersion ?? '');
  setHidden('__db',       state.database ?? '');
  setHidden('__infra',    state.infrastructure ?? 'None');
  setHidden('__patterns', JSON.stringify(state.patterns));
  setHidden('__libs',     JSON.stringify(state.libraries));

  form.submit();
});

function collectVpsData() {
  const rows = [];
  for (let i = 1; i <= state.vpsRows; i++) {
    const row = document.getElementById(`vps-row-${i}`);
    if (!row) continue;
    const inputs = row.querySelectorAll('input, select');
    rows.push({
      label: inputs[0].value, host: inputs[1].value,
      port: parseInt(inputs[2].value) || 22,
      username: inputs[3].value, password: inputs[4].value,
      role: inputs[5].value
    });
  }
  return rows;
}

// ── Init ──────────────────────────────────────────────────────────────────────
<<<<<<< HEAD
renderStep();
=======
renderStep();
>>>>>>> 0dc2a35 (complete java,python,typescript)
