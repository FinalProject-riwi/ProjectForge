/* ═══════════════════════════════════════════════════════════════════════════
   ProjectForge — Voice Assistant
   Hybrid mode: one-shot stack parsing (dashboard) + guided wizard (step-by-step)
   Requirements: Web Speech API — Chrome / Edge (STT); all browsers (TTS)
   ═══════════════════════════════════════════════════════════════════════════ */
(function () {
  'use strict';

  const hasTTS = typeof window.speechSynthesis !== 'undefined';
  const SpeechRec = window.SpeechRecognition || window.webkitSpeechRecognition;
  const hasSTT = !!SpeechRec;

  /* ── VoiceAssistant class ─────────────────────────────────────────────────── */
  class VoiceAssistant {
    constructor() {
      this._state      = 'idle';
      this._muted      = localStorage.getItem('pf-voice-muted') === '1';
      this._femaleVoice = null;
      this._recognition = null;
      this._audioCtx   = null;
      this._analyser   = null;
      this._micStream  = null;
      this._animFrame  = null;
      this._canvas     = null;
      this._ctx2d      = null;
      this._t          = 0;

      if (hasTTS) this._loadVoices();
      if (hasSTT) this._initSTT();
      this._createBar();
    }

    /* ── Voice loading ──────────────────────────────────────────────────────── */
    _loadVoices() {
      const pick = () => {
        const voices = window.speechSynthesis.getVoices();
        // Preferred female Spanish voices across browsers / OS
        const preferred = [
          'Google español de Estados Unidos',
          'Microsoft Sabina Online (Natural) - Spanish (Mexico)',
          'Microsoft Sabina - Spanish (Mexico)',
          'Paulina',
          'Mónica', 'Monica',
          'Luciana', 'Conchita',
          'Google español',
        ];
        this._femaleVoice =
          voices.find(v => preferred.includes(v.name)) ||
          voices.find(v => v.lang.startsWith('es') && /sabina|paulina|monica|mónica|conchita|luciana|female/i.test(v.name)) ||
          voices.find(v => v.lang.startsWith('es') && v.name.toLowerCase().includes('google')) ||
          voices.find(v => v.lang.startsWith('es')) ||
          voices.find(v => /female|woman/i.test(v.name)) ||
          null;
      };
      if (window.speechSynthesis.getVoices().length) pick();
      window.speechSynthesis.addEventListener('voiceschanged', pick);
    }

    /* ── STT init ───────────────────────────────────────────────────────────── */
    _initSTT() {
      this._recognition = new SpeechRec();
      this._recognition.continuous     = false;
      this._recognition.interimResults = false;
      this._recognition.lang           = 'es-ES';
    }

    /* ── TTS: speak ─────────────────────────────────────────────────────────── */
    speak(text, onEnd) {
      if (!hasTTS || this._muted) { setTimeout(() => onEnd?.(), 0); return; }
      window.speechSynthesis.cancel();
      const u = new SpeechSynthesisUtterance(text);
      if (this._femaleVoice) u.voice = this._femaleVoice;
      u.lang   = 'es-ES';
      u.rate   = 0.91;
      u.pitch  = 1.08;
      u.volume = 1.0;
      u.onstart = () => this._setState('speaking', 'Hablando...');
      u.onend   = () => { this._setState('idle', 'Lista'); onEnd?.(); };
      u.onerror = () => { this._setState('idle', 'Lista'); onEnd?.(); };
      window.speechSynthesis.speak(u);
    }

    stopSpeaking() {
      if (hasTTS) window.speechSynthesis.cancel();
      this._setState('idle', 'Lista');
    }

    /* ── STT: listen ────────────────────────────────────────────────────────── */
    listen(timeoutMs = 10000) {
      if (!hasSTT || !this._recognition) return Promise.resolve(null);
      return new Promise(resolve => {
        let settled = false;
        const done = (val) => {
          if (settled) return;
          settled = true;
          clearTimeout(timer);
          this._hideListeningOverlay();
          this._stopMicVisualization();
          if (val) {
            this._setState('thinking', 'Procesando...');
          } else {
            this._setState('idle', 'Lista');
          }
          resolve(val);
        };

        this._setState('listening', 'Escuchando...');
        this._showListeningOverlay(() => {
          try { this._recognition.stop(); } catch {}
          done(null);
        });
        this._startMicVisualization();

        const timer = setTimeout(() => {
          try { this._recognition.stop(); } catch {}
        }, timeoutMs);

        this._recognition.onresult = e => done(e.results[0][0].transcript);
        this._recognition.onerror  = () => done(null);
        this._recognition.onend    = () => done(null);

        try { this._recognition.start(); } catch { done(null); }
      });
    }

    stopListening() {
      try { this._recognition?.stop(); } catch {}
      this._hideListeningOverlay();
      this._stopMicVisualization();
      this._setState('idle', 'Lista');
    }

    setIdle(text = 'Lista') { this._setState('idle', text); }

    /* ── Mic visualizer ─────────────────────────────────────────────────────── */
    async _startMicVisualization() {
      try {
        this._audioCtx = this._audioCtx || new (window.AudioContext || window.webkitAudioContext)();
        this._micStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
        this._analyser = this._audioCtx.createAnalyser();
        this._analyser.fftSize = 128;
        this._audioCtx.createMediaStreamSource(this._micStream).connect(this._analyser);
      } catch { /* mic permission denied — fallback animation used instead */ }
    }

    _stopMicVisualization() {
      this._micStream?.getTracks().forEach(t => t.stop());
      this._micStream = null;
      this._analyser  = null;
    }

    /* ── Voice bar DOM ──────────────────────────────────────────────────────── */
    _createBar() {
      const bar = document.createElement('div');
      bar.id        = 'voice-bar';
      bar.className = 'voice-bar';
      bar.setAttribute('data-state', 'idle');
      bar.setAttribute('role', 'status');
      bar.setAttribute('aria-live', 'polite');
      bar.innerHTML = `
        <div class="voice-bar-inner">
          <div class="voice-info">
            <div class="voice-status-dot"></div>
            <span class="voice-status-text" id="v-status-text">Lista</span>
          </div>
          <canvas id="voice-waveform" class="voice-waveform" width="440" height="44" aria-hidden="true"></canvas>
          <div class="voice-controls">
            <button class="voice-mic-btn" id="v-mic-btn" title="Hablar con la asistente" aria-label="Hablar">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 6 0V4a3 3 0 0 0-3-3z"/>
                <path d="M19 10v2a7 7 0 0 1-14 0v-2"/>
                <line x1="12" y1="19" x2="12" y2="23"/>
                <line x1="8" y1="23" x2="16" y2="23"/>
              </svg>
            </button>
            <button class="voice-mute-btn${this._muted ? ' muted' : ''}" id="v-mute-btn" title="Silenciar/Activar voz" aria-label="Silenciar">
              ${this._muted ? '🔇' : '🔊'}
            </button>
          </div>
        </div>`;
      document.body.appendChild(bar);
      document.body.classList.add('voice-bar-active');

      this._canvas = document.getElementById('voice-waveform');
      this._ctx2d  = this._canvas?.getContext('2d');

      document.getElementById('v-mic-btn')?.addEventListener('click',  () => this._onMicClick());
      document.getElementById('v-mute-btn')?.addEventListener('click', () => this._toggleMute());

      this._animateWaveform();
    }

    _onMicClick() {
      if (this._state === 'listening') { this.stopListening(); return; }
      if (this._state === 'speaking')  { this.stopSpeaking(); }
      (window.__voiceWizardListen || window.__voiceDashboardListen || (() => {
        this.listen().then(t => { if (t) this.speak(`Dijiste: ${t}`); else this.setIdle(); });
      }))();
    }

    _toggleMute() {
      this._muted = !this._muted;
      localStorage.setItem('pf-voice-muted', this._muted ? '1' : '0');
      const btn = document.getElementById('v-mute-btn');
      if (btn) { btn.textContent = this._muted ? '🔇' : '🔊'; btn.classList.toggle('muted', this._muted); }
      if (this._muted && hasTTS) window.speechSynthesis.cancel();
    }

    /* ── State ──────────────────────────────────────────────────────────────── */
    _setState(state, label) {
      this._state = state;
      document.getElementById('voice-bar')?.setAttribute('data-state', state);
      const txt = document.getElementById('v-status-text');
      if (txt) txt.textContent = label ?? '';
      document.getElementById('v-mic-btn')?.classList.toggle('active', state === 'listening');
    }

    /* ── Waveform animation ─────────────────────────────────────────────────── */
    _animateWaveform() {
      const draw = () => {
        this._animFrame = requestAnimationFrame(draw);
        if (!this._ctx2d || !this._canvas) return;
        const W = this._canvas.width, H = this._canvas.height;
        this._ctx2d.clearRect(0, 0, W, H);
        this._t += 0.055;

        switch (this._state) {
          case 'idle':      this._drawIdle(W, H);     break;
          case 'speaking':  this._drawSpeaking(W, H); break;
          case 'listening': this._drawListening(W, H);break;
          case 'thinking':  this._drawThinking(W, H); break;
        }
      };
      draw();
    }

    _drawIdle(W, H) {
      const c = this._ctx2d;
      c.strokeStyle = 'rgba(99,102,241,0.22)';
      c.lineWidth = 1.5;
      c.beginPath();
      for (let x = 0; x < W; x++) {
        const y = H / 2 + Math.sin(x * 0.022 + this._t * 0.28) * 2.5;
        x === 0 ? c.moveTo(x, y) : c.lineTo(x, y);
      }
      c.stroke();
    }

    _drawSpeaking(W, H) {
      const c = this._ctx2d;
      const layers = [
        { col: 'rgba(99,102,241,0.75)',  f: 0.038, a: 13,  s: 1.0 },
        { col: 'rgba(167,139,250,0.45)', f: 0.058, a: 7,   s: 1.55 },
        { col: 'rgba(34,211,238,0.3)',   f: 0.028, a: 9,   s: 0.72 },
      ];
      layers.forEach(({ col, f, a, s }) => {
        c.strokeStyle = col;
        c.lineWidth = 2;
        c.beginPath();
        for (let x = 0; x < W; x++) {
          const y = H / 2
            + Math.sin(x * f + this._t * s) * a
            + Math.sin(x * f * 1.8 + this._t * s * 0.45) * (a * 0.38);
          x === 0 ? c.moveTo(x, y) : c.lineTo(x, y);
        }
        c.stroke();
      });
    }

    _drawListening(W, H) {
      const c = this._ctx2d;
      if (this._analyser) {
        const data = new Uint8Array(this._analyser.frequencyBinCount);
        this._analyser.getByteFrequencyData(data);
        const bw = W / data.length;
        const grad = c.createLinearGradient(0, 0, W, 0);
        grad.addColorStop(0,   'rgba(34,211,238,0.85)');
        grad.addColorStop(0.5, 'rgba(99,102,241,0.85)');
        grad.addColorStop(1,   'rgba(34,211,238,0.85)');
        c.fillStyle = grad;
        data.forEach((v, i) => {
          const bh = (v / 255) * H;
          c.fillRect(i * bw, H - bh, Math.max(bw - 1, 1), bh);
        });
      } else {
        // Fallback animated bars when mic permission not granted
        const bars = 24;
        const bw = W / bars;
        for (let i = 0; i < bars; i++) {
          const h = (Math.sin(i * 0.55 + this._t * 2.8) * 0.5 + 0.5) * H * 0.82;
          c.fillStyle = `rgba(34,211,238,${0.35 + (h / H) * 0.55})`;
          c.fillRect(i * bw + 1, (H - h) / 2, bw - 2, h);
        }
      }
    }

    _drawThinking(W, H) {
      const c = this._ctx2d;
      const dots = 5, sp = 18;
      const sx = (W - (dots - 1) * sp) / 2;
      for (let i = 0; i < dots; i++) {
        const ph = this._t * 2.8 + i * 0.65;
        const y = H / 2 + Math.sin(ph) * 6;
        const a = 0.35 + Math.sin(ph) * 0.4;
        c.fillStyle = `rgba(245,158,11,${a})`;
        c.beginPath();
        c.arc(sx + i * sp, y, 4, 0, Math.PI * 2);
        c.fill();
      }
    }

    /* ── Listening overlay ──────────────────────────────────────────────────── */
    _showListeningOverlay(onCancel) {
      document.getElementById('v-listen-overlay')?.remove();
      const ov = document.createElement('div');
      ov.id = 'v-listen-overlay';
      ov.className = 'voice-listening-overlay';
      ov.innerHTML = `
        <div class="voice-listening-pulse">🎤</div>
        <div class="voice-listening-title">Te escucho...</div>
        <div class="voice-listening-hint">Habla con naturalidad, en español.</div>
        <button class="voice-listening-cancel" id="v-cancel-listen">Cancelar</button>`;
      document.body.appendChild(ov);
      document.getElementById('v-cancel-listen')?.addEventListener('click', () => onCancel?.());
    }

    _hideListeningOverlay() {
      document.getElementById('v-listen-overlay')?.remove();
    }
  }

  /* ── Singleton ────────────────────────────────────────────────────────────── */
  window.voiceAssistant = new VoiceAssistant();
  const va = window.voiceAssistant;

  /* ═══════════════════════════════════════════════════════════════════════════
     DASHBOARD VOICE MODE
     Greeting + one-shot stack parsing → redirect to wizard with pre-fill
     ═══════════════════════════════════════════════════════════════════════════ */
  const dashInit = document.getElementById('voice-dashboard-init');
  if (dashInit) {
    window.__voiceDashboardMode = true;
    const username = (dashInit.dataset.username || 'amigo').split(' ')[0];

    setTimeout(() => {
      va.speak(
        `Hola ${username}. ¿Qué quieres construir hoy? ` +
        `Si ya sabes tu stack, dímelo directo y lo preparo todo. ` +
        `O usa el botón de nuevo proyecto para ir paso a paso.`
      );
    }, 700);

    window.__voiceDashboardListen = async function () {
      const transcript = await va.listen(12000);
      if (!transcript) { va.setIdle('Lista'); return; }

      const micBtn = document.getElementById('voice-dashboard-mic-btn');
      if (micBtn) micBtn.disabled = true;

      try {
        const resp = await fetch('/wizard/api/voice-parse', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ transcript })
        });
        const data = await resp.json();

        if (data.success) {
          sessionStorage.setItem('pf-voice-config', JSON.stringify(data));
          const infraLabel = data.infrastructure === 'DockerCompose' ? 'Docker'
            : data.infrastructure === 'Kubernetes' ? 'Kubernetes'
            : 'sin contenedores';
          va.speak(
            `Perfecto. Configuré ${data.architecture} con ${data.framework}, ` +
            `${data.database} y ${infraLabel}. Abriendo el asistente...`,
            () => { window.location.href = '/wizard'; }
          );
        } else {
          va.speak('No entendí bien. Inténtalo de nuevo o usa el wizard paso a paso.');
          va.setIdle('Lista');
          if (micBtn) micBtn.disabled = false;
        }
      } catch {
        va.speak('Hubo un error de red. Intenta de nuevo.');
        va.setIdle('Lista');
        if (micBtn) micBtn.disabled = false;
      }
    };

    document.getElementById('voice-dashboard-mic-btn')
      ?.addEventListener('click', window.__voiceDashboardListen);
  }

  /* ═══════════════════════════════════════════════════════════════════════════
     WIZARD VOICE MODE
     Step-by-step guidance + optional one-shot from dashboard pre-fill
     ═══════════════════════════════════════════════════════════════════════════ */
  const wizInit = document.getElementById('voice-wizard-init');
  if (wizInit) {
    window.__voiceWizardMode = true;

    // Check sessionStorage for dashboard one-shot pre-fill
    const prefillRaw = sessionStorage.getItem('pf-voice-config');
    if (prefillRaw) {
      sessionStorage.removeItem('pf-voice-config');
      try {
        const cfg = JSON.parse(prefillRaw);
        setTimeout(() => _applyVoiceConfig(cfg), 350);
      } catch { _speakStep(); }
    } else {
      // Start guided mode
      setTimeout(_speakStep, 800);
    }

    // Listen for step changes dispatched by wizard.js
    document.addEventListener('wizardStepChanged', e => {
      setTimeout(() => _speakStep(e.detail?.step), 350);
    });

    window.__voiceWizardListen = async function () {
      const text = await va.listen(10000);
      if (text) _handleWizardInput(text);
      else va.setIdle('Lista');
    };
  }

  /* ── Wizard helpers ───────────────────────────────────────────────────────── */
  function _speakStep(step) {
    const s = step || window.state?.currentStep || 1;
    const arch = window.state?.architecture || '';

    const prompts = {
      1: 'Primero el lenguaje. ¿Prefieres C Sharp punto NET, Java, Python, PHP, JavaScript o TypeScript?',
      2: `Ahora el framework${arch ? ' para ' + arch : ''}. ¿Cuál usarás? Y dime también la base de datos.`,
      3: '¿Necesitas contenedores? Puedo poner Docker Compose, Kubernetes, o dejarlo sin contenedores.',
      4: 'Patrones y librerías. La IA ya hizo sugerencias, pero puedes pedirme algo específico.',
      5: '¡Casi listo! ¿Cómo se llamará el proyecto?',
    };
    const msg = prompts[s];
    if (msg) va.speak(msg);
  }

  function _handleWizardInput(text) {
    const t    = text.toLowerCase();
    const step = window.state?.currentStep || 1;

    if (step === 1) {
      const arch = _parseArch(t);
      if (arch) {
        _selectCard('.arch-card', arch);
        if (window.state)  window.state.architecture = arch;
        if (window.frameworkCatalog?.[arch]) window.renderFrameworkOptions?.(window.frameworkCatalog[arch]);
        va.speak(`${arch} seleccionado. Pasamos al framework.`, () => setTimeout(() => window.nextStep?.(), 400));
      } else {
        va.speak('No reconocí el lenguaje. Prueba: Python, Java, C Sharp, PHP, JavaScript o TypeScript.');
      }
    } else if (step === 2) {
      const fw = _parseFramework(t, window.state?.architecture);
      const db = _parseDb(t);
      if (fw) { _selectCard('.fw-card', fw); if (window.state) window.state.framework = fw; }
      if (db) { _selectCard('.db-card', db); if (window.state) window.state.database  = db; }
      if (fw || db) {
        va.speak(`Listo. ${fw ? 'Framework: ' + fw + '. ' : ''}${db ? 'Base de datos: ' + db + '.' : ''}`);
      } else {
        va.speak('No entendí. Prueba con FastAPI, Django, PostgreSQL, MySQL…');
      }
    } else if (step === 3) {
      const infra = _parseInfra(t);
      if (infra) {
        _selectCard('.infra-card', infra);
        if (window.state) window.state.infrastructure = infra;
        const label = infra === 'None' ? 'sin contenedores' : infra;
        va.speak(`${label}. Continuamos.`, () => setTimeout(() => window.nextStep?.(), 400));
      } else {
        va.speak('Di: Docker, Kubernetes, o sin contenedores.');
      }
    } else if (step === 5) {
      const name = text.replace(/[^a-zA-Z0-9\-_\s]/g, '').trim().split(/\s+/).join('-').toLowerCase().slice(0, 60);
      if (name.length >= 2) {
        const el = document.getElementById('project-name');
        if (el) el.value = name;
        va.speak(`Nombre: ${name}. Revisa el resumen y presiona Generar.`);
      } else {
        va.speak('El nombre debe tener al menos dos caracteres. ¿Cómo lo llamarás?');
      }
    }
  }

  function _applyVoiceConfig(cfg) {
    if (cfg.architecture) {
      _selectCard('.arch-card', cfg.architecture);
      if (window.state) window.state.architecture = cfg.architecture;
      if (window.frameworkCatalog?.[cfg.architecture]) {
        window.renderFrameworkOptions?.(window.frameworkCatalog[cfg.architecture]);
      }
    }
    setTimeout(() => {
      if (cfg.framework) { _selectCard('.fw-card', cfg.framework); if (window.state) window.state.framework = cfg.framework; }
      if (cfg.database)  { _selectCard('.db-card', cfg.database);  if (window.state) window.state.database  = cfg.database;  }
      if (cfg.infrastructure) { _selectCard('.infra-card', cfg.infrastructure); if (window.state) window.state.infrastructure = cfg.infrastructure; }
      if (cfg.projectName) { const el = document.getElementById('project-name'); if (el) el.value = cfg.projectName; }
    }, 150);

    const infraLabel = cfg.infrastructure === 'DockerCompose' ? 'Docker'
      : cfg.infrastructure === 'Kubernetes' ? 'Kubernetes' : 'sin contenedores';

    va.speak(
      `Configuré ${cfg.architecture} con ${cfg.framework}, ${cfg.database} y ${infraLabel}. ` +
      `Revisa los detalles y confirma cuando estés listo.`,
      () => {
        if (window.state) { window.state.currentStep = 5; window.buildSummary?.(); window.renderStep?.(); }
      }
    );
  }

  /* ── Parsers ──────────────────────────────────────────────────────────────── */
  function _parseArch(t) {
    if (/python/.test(t)) return 'Python';
    if (/\bjava\b(?!script)/i.test(t)) return 'Java';
    if (/php|laravel|symfony/.test(t)) return 'Php';
    if (/typescript|typoscript/.test(t)) return 'TypeScript';
    if (/javascript|nodejs|node\.?js/.test(t)) return 'JavaScript';
    if (/\.?net|csharp|c sharp|aspnet|dotnet/.test(t)) return 'DotNet';
    if (/node(?!js)/.test(t)) return 'JavaScript';
    return null;
  }

  function _parseFramework(t, arch) {
    const map = {
      DotNet:     { webapi: 'AspNetCoreWebApi', mvc: 'AspNetCoreMVC', blazor: 'BlazorServer', minimal: 'MinimalApi' },
      Python:     { fastapi: 'FastAPI', django: 'Django', flask: 'Flask' },
      Java:       { spring: 'SpringBoot', quarkus: 'Quarkus', micronaut: 'Micronaut' },
      Php:        { laravel: 'Laravel', symfony: 'Symfony' },
      JavaScript: { express: 'ExpressJs', 'next.js': 'NextJs', nextjs: 'NextJs', nestjs: 'NestJs', 'nest.js': 'NestJs', node: 'NodeJs' },
      TypeScript: { 'next.js': 'NextTs', nextts: 'NextTs', 'nest.js': 'NestTs', nestts: 'NestTs', next: 'NextTs', nest: 'NestTs' },
    };
    const m = map[arch] || {};
    for (const [kw, val] of Object.entries(m)) {
      if (t.includes(kw)) return val;
    }
    return null;
  }

  function _parseDb(t) {
    if (/mysql/.test(t)) return 'MySQL';
    if (/mongo/.test(t)) return 'MongoDB';
    if (/redis/.test(t)) return 'Redis';
    if (/sqlite/.test(t)) return 'SQLite';
    if (/sql.?server|mssql/.test(t)) return 'SqlServer';
    if (/postgres/.test(t)) return 'PostgreSQL';
    return null;
  }

  function _parseInfra(t) {
    if (/kubernetes|k8s/.test(t)) return 'Kubernetes';
    if (/docker/.test(t)) return 'DockerCompose';
    if (/sin|none|no|solo|local/.test(t)) return 'None';
    return null;
  }

  function _selectCard(selector, value) {
    document.querySelectorAll(selector).forEach(card => {
      const match = card.dataset.value === value;
      card.classList.toggle('selected', match);
      const inp = card.querySelector('input');
      if (inp) inp.checked = match;
    });
  }

})();