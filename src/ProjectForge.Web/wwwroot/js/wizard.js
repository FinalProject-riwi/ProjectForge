// ── TabBuilder Wizard ─────────────────────────────────────────────────────────

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

const architectureMeta = {
  DotNet:     { label: 'C# / .NET',   logo: '/img/logos/languages/csharp.png',     desc: 'ASP.NET Core, Blazor, Minimal API' },
  Java:       { label: 'Java',        logo: '/img/logos/languages/java.png',        desc: 'Spring Boot, Quarkus, Micronaut' },
  Python:     { label: 'Python',      logo: '/img/logos/languages/python.png',      desc: 'FastAPI, Django, Flask' },
  Php:        { label: 'PHP',         logo: '/img/logos/languages/php.png',         desc: 'Laravel, Symfony' },
  JavaScript: { label: 'JavaScript',  logo: '/img/logos/languages/javascript.png',  desc: 'Node.js, Express, NestJS, Next.js' },
  TypeScript: { label: 'TypeScript',  logo: '/img/logos/languages/typescript.png',  desc: 'NestTS, Next.js (TS)' },
};

const databaseMeta = {
  PostgreSQL: { logo: '/img/logos/databases/postgresql.png', badge: 'Recomendado' },
  MySQL:      { logo: '/img/logos/databases/mysql.png',      badge: 'Popular' },
  SqlServer:  { logo: '/img/logos/databases/sqlserver.png',  badge: 'Empresarial' },
  MongoDB:    { logo: '/img/logos/databases/mongodb.png',    badge: 'NoSQL' },
  Redis:      { logo: '/img/logos/databases/redis.png',      badge: 'Cache/Cola' },
  SQLite:     { logo: '/img/logos/databases/sqlite.png',     badge: 'Desarrollo' },
};

const infrastructureMeta = {
  None:         { icon: '💻',  logo: null,                               label: 'Sin contenedores', desc: 'Solo el código del proyecto' },
  DockerCompose:{ icon: '🐳',  logo: '/img/logos/infra/docker.png',      label: 'Docker Compose',   desc: 'Ideal para desarrollo local y un solo servidor' },
  Kubernetes:   { icon: '⚙️', logo: '/img/logos/infra/kubernetes.png',  label: 'Kubernetes',       desc: 'Escalado horizontal, múltiples VPS' },
};

const frameworkCatalog = {
  DotNet: [
    { value: 'AspNetCoreWebApi', label: 'ASP.NET Core Web API', logo: '/img/logos/frameworks/aspdotnet.png', versions: ['10.0', '8.0', '7.0'] },
    { value: 'AspNetCoreMVC',    label: 'ASP.NET Core MVC',     logo: '/img/logos/frameworks/aspdotnet.png', versions: ['10.0', '8.0'] },
    { value: 'BlazorServer',     label: 'Blazor Server',        logo: '/img/logos/frameworks/aspdotnet.png', versions: ['10.0', '8.0'] },
    { value: 'BlazorWasm',       label: 'Blazor WebAssembly',   logo: '/img/logos/frameworks/aspdotnet.png', versions: ['10.0', '8.0'] },
    { value: 'MinimalApi',       label: 'Minimal API',          logo: '/img/logos/frameworks/aspdotnet.png', versions: ['10.0', '8.0'] },
  ],
  Java: [
    { value: 'SpringBoot', label: 'Spring Boot', logo: '/img/logos/frameworks/springboot.png', versions: ['3.3', '3.2', '2.7'] },
    { value: 'Quarkus',    label: 'Quarkus',     logo: '/img/logos/frameworks/quarkus.png',    versions: ['3.x'] },
    { value: 'Micronaut',  label: 'Micronaut',   logo: '/img/logos/frameworks/micronaut.png',  versions: ['4.x'] },
  ],
  Python: [
    { value: 'FastAPI', label: 'FastAPI', logo: '/img/logos/frameworks/fastapi.png', versions: ['0.115', '0.110'] },
    { value: 'Django',  label: 'Django',  logo: '/img/logos/frameworks/django.png',  versions: ['5.0', '4.2'] },
    { value: 'Flask',   label: 'Flask',   logo: '/img/logos/frameworks/flask.png',   versions: ['3.0', '2.3'] },
  ],
  Php: [
    { value: 'Laravel',  label: 'Laravel',  logo: '/img/logos/frameworks/laravel.png',  versions: ['11.x', '10.x'] },
    { value: 'Symfony',  label: 'Symfony',  logo: '/img/logos/frameworks/symfony.png',  versions: ['7.x', '6.x'] },
  ],
  JavaScript: [
    { value: 'NodeJs',    label: 'Node.js',    logo: '/img/logos/frameworks/nodejs.png',    versions: ['22.x', '20.x'] },
    { value: 'ExpressJs', label: 'Express.js', logo: '/img/logos/frameworks/expressjs.png', versions: ['5.x', '4.x'] },
    { value: 'NestJs',    label: 'NestJS',     logo: '/img/logos/frameworks/nestjs.png',    versions: ['10.x'] },
    { value: 'NextJs',    label: 'Next.js',    logo: '/img/logos/frameworks/nextjs.png',    versions: ['14.x'] },
  ],
  TypeScript: [
    { value: 'NestTs', label: 'NestJS (TypeScript)', logo: '/img/logos/frameworks/nestts.png', versions: ['10.x'] },
    { value: 'NextTs', label: 'Next.js (TypeScript)', logo: '/img/logos/frameworks/nextts.png', versions: ['14.x'] },
  ],
};

function normalizeToken(value) {
  return String(value || '').toLowerCase().replace(/[^a-z0-9]/g, '');
}

// ── Navigation ────────────────────────────────────────────────────────────────

async function nextStep() {
  if (!validateStep(state.currentStep)) return;
  if (state.currentStep === 3) await loadStep4Data();
  if (state.currentStep === 4) {
    buildSummary();
    // Verificar prerrequisitos al entrar al paso 5 (resumen final)
    await showPrereqModalIfNeeded(state.architecture, () => {
      state.currentStep++;
      renderStep();
    });
    return;
  }
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

function resetSelections() {
  document.querySelectorAll('.db-card, .infra-card').forEach(card => card.classList.remove('selected'));
  document.querySelectorAll('.db-card input, .infra-card input').forEach(input => {
    input.checked = input.name === 'infrastructure' && input.value === 'None';
  });
  document.querySelector('.infra-card[data-value="None"]')?.classList.add('selected');
  const vpsSection = document.getElementById('vps-section');
  if (vpsSection) vpsSection.style.display = 'none';
}

function clearStep4() {
  const patterns = document.getElementById('patterns-list');
  const libraries = document.getElementById('libraries-list');
  if (patterns) patterns.innerHTML = '';
  if (libraries) libraries.innerHTML = '';
}

// ── Step 1: Architecture ──────────────────────────────────────────────────────

function initializeArchitectureCards() {
  document.querySelectorAll('.arch-card').forEach(card => {
    card.addEventListener('click', () => {
      const newArch = card.dataset.value;
      if (state.architecture !== newArch) {
        state.framework = null;
        state.frameworkVersion = null;
        state.database = null;
        state.infrastructure = 'None';
        state.patterns = [];
        state.libraries = [];
        resetSelections();
        clearStep4();
        renderFrameworkOptions([]);
      }
      document.querySelectorAll('.arch-card').forEach(c => c.classList.remove('selected'));
      card.classList.add('selected');
      card.querySelector('input').checked = true;
      state.architecture = newArch;

      if (frameworkCatalog[newArch]) {
        renderFrameworkOptions(frameworkCatalog[newArch]);
      }
    });
  });
}

// ── Step 2: Framework ─────────────────────────────────────────────────────────

function renderFrameworkOptions(options) {
  const container = document.getElementById('framework-options');
  if (!container) return;

  if (!options || options.length === 0) {
    container.innerHTML = '<p style="color:var(--text-muted)">Selecciona una arquitectura primero.</p>';
    const versionSelect = document.getElementById('framework-version-select');
    if (versionSelect) {
      versionSelect.innerHTML = '<option value="">Selecciona un framework primero</option>';
    }
    return;
  }

  container.innerHTML = options.map(opt => `
    <label class="fw-card${state.framework === opt.value ? ' selected' : ''}" data-value="${opt.value}">
      <input type="radio" name="framework" value="${opt.value}" ${state.framework === opt.value ? 'checked' : ''} />
      <img src="${opt.logo}" alt="${opt.label}" class="fw-logo" onerror="this.style.display='none'" />
      <span class="fw-name">${opt.label}</span>
    </label>
  `).join('');

  // Set version select
  const versionSelect = document.getElementById('framework-version-select');
  if (versionSelect && state.framework) {
    const cur = options.find(o => o.value === state.framework);
    if (cur) {
      versionSelect.innerHTML = cur.versions.map(v => `<option value="${v}">${v}</option>`).join('');
      state.frameworkVersion = cur.versions[0];
    }
  } else if (versionSelect) {
    versionSelect.innerHTML = '<option value="">Selecciona un framework primero</option>';
  }

  // Attach click handlers
  container.querySelectorAll('.fw-card').forEach(card => {
    card.addEventListener('click', () => {
      container.querySelectorAll('.fw-card').forEach(c => c.classList.remove('selected'));
      card.classList.add('selected');
      card.querySelector('input').checked = true;
      state.framework = card.dataset.value;

      const selected = options.find(o => o.value === state.framework);
      if (selected && versionSelect) {
        versionSelect.innerHTML = selected.versions.map(v => `<option value="${v}">${v}</option>`).join('');
        state.frameworkVersion = selected.versions[0];
      }
      clearStep4();
    });
  });
}

// ── Step 2: Database ──────────────────────────────────────────────────────────

function initializeDatabaseCards() {
  document.querySelectorAll('.db-card').forEach(card => {
    card.addEventListener('click', () => {
      document.querySelectorAll('.db-card').forEach(c => c.classList.remove('selected'));
      card.classList.add('selected');
      card.querySelector('input').checked = true;
      state.database = card.dataset.value;
      clearStep4();
    });
  });
}

// ── Step 3: Infrastructure ────────────────────────────────────────────────────

function initializeInfrastructureCards() {
  document.querySelectorAll('.infra-card').forEach(card => {
    card.addEventListener('click', () => {
      document.querySelectorAll('.infra-card').forEach(c => c.classList.remove('selected'));;
      card.classList.add('selected');
      card.querySelector('input').checked = true;
      state.infrastructure = card.dataset.value;
      document.getElementById('vps-section').style.display =
        state.infrastructure === 'Kubernetes' ? 'block' : 'none';
      clearStep4();
    });
  });
}

document.getElementById('framework-version-select')?.addEventListener('change', e => {
  state.frameworkVersion = e.target.value;
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

// ── Step 4: AI Suggestions ────────────────────────────────────────────────────

async function loadStep4Data() {
  if (!state.architecture || !state.framework) return;
  const banner = document.getElementById('ai-rationale');
  if (banner) banner.textContent = 'Cargando catálogo y consultando IA...';

  try {
    const [patternsResp, librariesResp] = await Promise.all([
      fetch(`/wizard/api/patterns/${state.architecture}/${state.framework}`),
      fetch(`/wizard/api/libraries/${state.architecture}/${state.framework}`),
    ]);

    const patternCatalog = patternsResp.ok ? await patternsResp.json() : [];
    const libraryCatalog = librariesResp.ok ? await librariesResp.json() : [];

    let suggestedPatterns = [];
    let suggestedLibraries = [];
    let rationale = '';

    try {
      const suggestResp = await fetch('/wizard/api/suggest', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          architecture: state.architecture,
          framework: state.framework,
          database: state.database,
          infrastructure: state.infrastructure,
          alreadySelectedPatterns: state.patterns
        })
      });
      if (suggestResp.ok) {
        const data = await suggestResp.json();
        suggestedPatterns = data.suggestedPatterns || [];
        suggestedLibraries = data.suggestedLibraries || [];
        rationale = data.rationale || '';
      } else {
        rationale = 'No se pudieron cargar sugerencias de IA. Selecciona manualmente del catálogo.';
      }
    } catch {
      rationale = 'No se pudieron cargar sugerencias de IA. Selecciona manualmente del catálogo.';
    }

    if (banner) banner.textContent = rationale || 'Selecciona del catálogo completo. La IA marcará sus sugerencias con ✦.';
    renderPatterns(patternCatalog || [], suggestedPatterns);
    renderLibraries(libraryCatalog || [], suggestedLibraries);
  } catch (e) {
    console.error('Error cargando catálogos de patrones y librerías:', e);
    if (banner) banner.textContent = 'No se pudieron cargar los catálogos. Recarga el wizard.';
    renderPatterns([], []);
    renderLibraries([], []);
  }
}

function renderPatterns(all, suggested) {
  const selected = state.patterns[0] ?? (suggested[0] ?? null);
  const suggestedSet = new Set((suggested || []).map(normalizeToken));

  document.getElementById('patterns-list').innerHTML = all.length
    ? all.map(p => {
        const ok = suggestedSet.has(normalizeToken(p.value)) || suggestedSet.has(normalizeToken(p.label));
        const isSelected = selected && normalizeToken(selected) === normalizeToken(p.value);
        return `<label class="${[ok ? 'suggested' : '', isSelected ? 'selected' : ''].filter(Boolean).join(' ')}">
          <input type="radio" name="pattern-choice" value="${p.value}" ${isSelected ? 'checked' : ''} />
          ${p.label} ${ok ? '<span style="font-size:.7rem;color:#8b5cf6">✦ IA</span>' : ''}
        </label>`;
      }).join('')
    : '<p style="color:var(--text-muted);font-size:.95rem">No hay patrones disponibles para esta combinación.</p>';

  state.patterns = selected ? [selected] : [];

  document.querySelectorAll('input[name="pattern-choice"]').forEach(input => {
    input.addEventListener('change', () => {
      if (!input.checked) return;
      state.patterns = [input.value];
      document.querySelectorAll('#patterns-list label').forEach(label => {
        const radio = label.querySelector('input[name="pattern-choice"]');
        label.classList.toggle('selected', !!radio?.checked);
      });
    });
  });
}

function renderLibraries(all, suggested) {
  const suggestedSet = new Set((suggested || []).map(normalizeToken));
  document.getElementById('libraries-list').innerHTML = all.length
    ? all.map(l => {
        const ok = suggestedSet.has(normalizeToken(l.value)) || suggestedSet.has(normalizeToken(l.label));
        const checked = ok || state.libraries.includes(l.value);
        return `<label class="${ok ? 'suggested' : ''}">
            <input type="checkbox" value="${l.value}" ${checked ? 'checked' : ''} onchange="toggleSel('libraries','${l.value}',this.checked)" />
            ${l.label} ${l.badge ? `<span style="font-size:.7rem;color:#8b5cf6">${l.badge}</span>` : ''} ${ok ? '<span style="font-size:.7rem;color:#8b5cf6">✦ IA</span>' : ''}
          </label>`;
      }).join('')
    : '<p style="color:var(--text-muted);font-size:.95rem">No hay librerías disponibles para esta combinación.</p>';

  // Seed libraries state from suggested (if nothing already selected)
  if (state.libraries.length === 0) {
    state.libraries = all.filter(l => suggestedSet.has(normalizeToken(l.value)) || suggestedSet.has(normalizeToken(l.label))).map(l => l.value);
  }
}

function toggleSel(key, value, checked) {
  if (checked) { if (!state[key].includes(value)) state[key].push(value); }
  else { state[key] = state[key].filter(v => v !== value); }
}

// ── Step 5: Summary ───────────────────────────────────────────────────────────

function buildSummary() {
  const el = document.getElementById('config-summary');
  if (!el) return;

  const archMeta = architectureMeta[state.architecture] || {};
  const dbMeta = databaseMeta[state.database] || {};
  const infraMeta = infrastructureMeta[state.infrastructure] || {};

  const logoHtml = (src, alt) => src
    ? `<img src="${src}" alt="${alt}" style="width:20px;height:20px;object-fit:contain;vertical-align:middle;margin-right:6px" onerror="this.style.display='none'" />`
    : '';

  el.innerHTML = [
    ['Arquitectura', `${logoHtml(archMeta.logo, archMeta.label)}${archMeta.label || state.architecture}`],
    ['Framework',    state.framework + (state.frameworkVersion ? ' ' + state.frameworkVersion : '')],
    ['Base de Datos', `${logoHtml(dbMeta.logo, state.database)}${state.database}`],
    ['Infraestructura', `${infraMeta.logo ? logoHtml(infraMeta.logo, infraMeta.label) : infraMeta.icon + ' '}${infraMeta.label || state.infrastructure}`],
    ['Patrones', state.patterns.join(', ') || 'Ninguno'],
    ['Librerías', state.libraries.slice(0, 5).join(', ') + (state.libraries.length > 5 ? '…' : '') || 'Ninguna'],
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
  if (!/^[a-zA-Z0-9_-]+$/.test(name)) { showError('El nombre solo puede contener letras, números, guiones y guiones bajos'); return; }

  const btn = document.getElementById('btn-submit');
  btn.disabled = true;
  btn.textContent = 'Creando proyecto...';

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
initializeArchitectureCards();
initializeDatabaseCards();
initializeInfrastructureCards();
renderFrameworkOptions([]);
renderStep();

// Set None as default infrastructure selected
document.querySelector('.infra-card[data-value="None"]')?.classList.add('selected');

// ═══════════════════════════════════════════════════════════════════════════════
// PREREQUISITE CHECK — detecta herramientas instaladas antes de generar
// ═══════════════════════════════════════════════════════════════════════════════

/** Llama al endpoint que verifica qué herramientas están instaladas en el servidor. */
// ═══════════════════════════════════════════════════════════════════════════════
// PREREQUISITE CHECK — SOLO AVISO (NO BLOQUEA)
// ═══════════════════════════════════════════════════════════════════════════════

async function checkPrerequisites(architecture) {
  try {
    const res = await fetch(`/api/v1/tools/check-prerequisites?architecture=${encodeURIComponent(architecture)}`, {
      headers: { 'Accept': 'application/json' }
    });
    if (!res.ok) return { allInstalled: true, tools: [] };
    return await res.json();
  } catch {
    return { allInstalled: true, tools: [] };
  }
}

/**
 * SOLO AVISO — nunca bloquea navegación
 */
async function showPrereqModalIfNeeded(architecture, onProceed) {
  const data = await checkPrerequisites(architecture);

  const missing = (data.tools ?? []).filter(t => !t.installed);

  // 👉 SI NO HAY FALTANTES: no hacer nada especial
  if (missing.length === 0) {
    onProceed?.();
    return;
  }

  // 🟡 SI HAY FALTANTES: mostrar aviso pero NO bloquear
  const archLabels = {
    DotNet: 'C# / .NET', Java: 'Java', Python: 'Python',
    Php: 'PHP', JavaScript: 'JavaScript', TypeScript: 'TypeScript'
  };

  const labelEl = document.getElementById('prereq-arch-label');
  if (labelEl) {
    labelEl.textContent = archLabels[architecture] ?? architecture;
  }

  const list = document.getElementById('prereq-tools-list');
  if (list) {
    list.innerHTML = missing.map(t => `
      <li class="prereq-tool-item">
        <span class="prereq-tool-status">⚠️</span>
        <span class="prereq-tool-info">
          <span class="prereq-tool-name">${escapeHtml(t.name)}</span>
          <span class="prereq-tool-desc">${escapeHtml(t.description)}</span>
        </span>
        <a href="${escapeHtml(t.installUrl)}" target="_blank" rel="noopener noreferrer" class="prereq-tool-link">
          Instalar ↗
        </a>
      </li>
    `).join('');
  }

  const backdrop = document.getElementById('prereq-modal-backdrop');
  if (backdrop) {
    backdrop.style.display = 'flex';
    backdrop.setAttribute('aria-hidden', 'false');
  }

  // ⚠️ NO guardar callback, NO bloquear flujo
  onProceed?.();
}

/** Cierra modal */
function closePrereqModal() {
  const backdrop = document.getElementById('prereq-modal-backdrop');
  if (!backdrop) return;

  backdrop.style.display = 'none';
  backdrop.setAttribute('aria-hidden', 'true');
}

/** Botón "continuar" ahora solo cierra */
function proceedAnywayPrereq() {
  closePrereqModal();
}

function escapeHtml(str) {
  const d = document.createElement('div');
  d.textContent = String(str ?? '');
  return d.innerHTML;
}

// Cerrar modal con Escape
document.addEventListener('keydown', e => {
  if (e.key === 'Escape' && document.getElementById('prereq-modal-backdrop')?.style.display === 'flex') {
    closePrereqModal();
  }
});

// Cerrar modal al hacer clic en el backdrop (fuera del box)
document.getElementById('prereq-modal-backdrop')?.addEventListener('click', function(e) {
  if (e.target === this) closePrereqModal();
});
