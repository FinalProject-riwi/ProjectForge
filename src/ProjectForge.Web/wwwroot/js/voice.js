/* ═══════════════════════════════════════════════════════════════════════════
   ProjectForge — Voice Assistant  v3
   TTS: ElevenLabs (eleven_multilingual_v2) → fallback to SpeechSynthesis
   STT: Web Speech API (Chrome/Edge)
   Dashboard: voice → parse → name → auto-generate → terminal
   Wizard:    step-by-step guided voice filling
   ═══════════════════════════════════════════════════════════════════════════ */
(function () {
  'use strict';

  const hasTTS = typeof window.speechSynthesis !== 'undefined';
  const SpeechRec = window.SpeechRecognition || window.webkitSpeechRecognition;
  const hasSTT = !!SpeechRec;

  /* ── VoiceAssistant ─────────────────────────────────────────────────────── */
  class VoiceAssistant {
    constructor() {
      this._state        = 'idle';
      this._muted        = localStorage.getItem('pf-voice-muted') === '1';
      this._fbVoice      = null;   // fallback SpeechSynthesis voice
      this._recognition  = null;
      this._audioCtx     = null;
      this._analyser     = null;
      this._micStream    = null;
      this._currentAudio = null;  // active HTMLAudioElement (ElevenLabs)
      this._canvas       = null;
      this._ctx2d        = null;
      this._t            = 0;

      if (hasTTS) this._loadFallbackVoice();
      if (hasSTT) this._initSTT();
      this._createBar();
    }

    /* ── Fallback voice (SpeechSynthesis) ───────────────────────────────── */
    _loadFallbackVoice() {
      const pick = () => {
        const voices = window.speechSynthesis.getVoices();
        const names = [
          'Microsoft Sabina Online (Natural) - Spanish (Mexico)',
          'Microsoft Sabina - Spanish (Mexico)',
          'Google español de Estados Unidos',
          'Paulina', 'Mónica', 'Monica', 'Luciana', 'Conchita', 'Helena',
          'Google español',
        ];
        this._fbVoice =
          voices.find(v => names.includes(v.name)) ||
          voices.find(v => v.lang.startsWith('es') && /online|neural|natural/i.test(v.name)) ||
          voices.find(v => v.lang.startsWith('es') && /sabina|paulina|monica|conchita|luciana/i.test(v.name)) ||
          voices.find(v => v.lang.startsWith('es') && /google/i.test(v.name)) ||
          voices.find(v => v.lang.startsWith('es')) ||
          null;
      };
      if (window.speechSynthesis.getVoices().length) pick();
      window.speechSynthesis.addEventListener('voiceschanged', pick);
    }

    /* ── STT ────────────────────────────────────────────────────────────── */
    _initSTT() {
      this._recognition = new SpeechRec();
      this._recognition.continuous     = false;
      this._recognition.interimResults = false;
      this._recognition.lang           = 'es-ES';
    }

    /* ── TTS: ElevenLabs → SpeechSynthesis fallback ─────────────────────── */
    speak(text, onEnd) {
      if (!text || this._muted) { setTimeout(() => onEnd?.(), 0); return; }
      this._cancelAudio();
      window.speechSynthesis?.cancel();
      this._setState('speaking', 'Hablando...');

      const encoded = encodeURIComponent(text.trim().slice(0, 500));
      const audio   = new Audio(`/wizard/api/tts?text=${encoded}`);
      this._currentAudio = audio;

      audio.onended = () => {
        this._currentAudio = null;
        this._setState('idle', 'Lista');
        onEnd?.();
      };
      audio.onerror = () => {
        this._currentAudio = null;
        this._speakFallback(text, onEnd);
      };
      audio.play().catch(() => {
        this._currentAudio = null;
        this._speakFallback(text, onEnd);
      });
    }

    _speakFallback(text, onEnd) {
      if (!hasTTS) { this._setState('idle', 'Lista'); onEnd?.(); return; }
      this._setState('speaking', 'Hablando...');
      const u = new SpeechSynthesisUtterance(text);
      if (this._fbVoice) u.voice = this._fbVoice;
      u.lang = 'es-MX'; u.rate = 0.87; u.pitch = 0.95; u.volume = 1.0;
      u.onend   = () => { this._setState('idle', 'Lista'); onEnd?.(); };
      u.onerror = () => { this._setState('idle', 'Lista'); onEnd?.(); };
      window.speechSynthesis.speak(u);
    }

    _cancelAudio() {
      if (this._currentAudio) {
        this._currentAudio.onended = null;
        this._currentAudio.onerror = null;
        this._currentAudio.pause();
        this._currentAudio = null;
      }
    }

    stopSpeaking() {
      this._cancelAudio();
      window.speechSynthesis?.cancel();
      this._setState('idle', 'Lista');
    }

    /* ── STT: listen ────────────────────────────────────────────────────── */
    listen(timeoutMs = 10000) {
      if (!hasSTT || !this._recognition) return Promise.resolve(null);
      return new Promise(resolve => {
        let settled = false;
        const done = val => {
          if (settled) return;
          settled = true;
          clearTimeout(timer);
          this._hideOverlay();
          this._stopMic();
          this._setState(val ? 'thinking' : 'idle', val ? 'Procesando...' : 'Lista');
          resolve(val);
        };

        this._setState('listening', 'Escuchando...');
        this._showOverlay(() => { try { this._recognition.stop(); } catch {} done(null); });
        this._startMic();

        const timer = setTimeout(() => { try { this._recognition.stop(); } catch {} }, timeoutMs);
        this._recognition.onresult = e => done(e.results[0][0].transcript);
        this._recognition.onerror  = () => done(null);
        this._recognition.onend    = () => done(null);
        try { this._recognition.start(); } catch { done(null); }
      });
    }

    stopListening() {
      try { this._recognition?.stop(); } catch {}
      this._hideOverlay();
      this._stopMic();
      this._setState('idle', 'Lista');
    }

    setIdle(text = 'Lista') { this._setState('idle', text); }

    /* ── Mic visualizer ─────────────────────────────────────────────────── */
    async _startMic() {
      try {
        this._audioCtx = this._audioCtx || new (window.AudioContext || window.webkitAudioContext)();
        this._micStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
        this._analyser = this._audioCtx.createAnalyser();
        this._analyser.fftSize = 128;
        this._audioCtx.createMediaStreamSource(this._micStream).connect(this._analyser);
      } catch { /* mic denied — fallback animation */ }
    }

    _stopMic() {
      this._micStream?.getTracks().forEach(t => t.stop());
      this._micStream = null;
      this._analyser  = null;
    }

    /* ── Voice bar ──────────────────────────────────────────────────────── */
    _createBar() {
      const bar = document.createElement('div');
      bar.id = 'voice-bar'; bar.className = 'voice-bar';
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
            <button class="voice-mic-btn" id="v-mic-btn" title="Hablar" aria-label="Hablar">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 6 0V4a3 3 0 0 0-3-3z"/>
                <path d="M19 10v2a7 7 0 0 1-14 0v-2"/>
                <line x1="12" y1="19" x2="12" y2="23"/>
                <line x1="8" y1="23" x2="16" y2="23"/>
              </svg>
            </button>
            <button class="voice-mute-btn${this._muted ? ' muted' : ''}" id="v-mute-btn" title="Silenciar/Activar" aria-label="Silenciar">
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
      this._animate();
    }

    _onMicClick() {
      if (this._state === 'listening') { this.stopListening(); return; }
      if (this._state === 'speaking')  { this.stopSpeaking(); return; }
      (window.__voiceWizardListen || window.__voiceDashboardListen || (() => {
        this.listen().then(t => t ? this.speak(`Dijiste: ${t}`) : this.setIdle());
      }))();
    }

    _toggleMute() {
      this._muted = !this._muted;
      localStorage.setItem('pf-voice-muted', this._muted ? '1' : '0');
      const btn = document.getElementById('v-mute-btn');
      if (btn) { btn.textContent = this._muted ? '🔇' : '🔊'; btn.classList.toggle('muted', this._muted); }
      if (this._muted) { this._cancelAudio(); window.speechSynthesis?.cancel(); }
    }

    _setState(state, label) {
      this._state = state;
      document.getElementById('voice-bar')?.setAttribute('data-state', state);
      const txt = document.getElementById('v-status-text');
      if (txt) txt.textContent = label ?? '';
      document.getElementById('v-mic-btn')?.classList.toggle('active', state === 'listening');
    }

    /* ── Listening overlay ──────────────────────────────────────────────── */
    _showOverlay(onCancel) {
      document.getElementById('v-listen-overlay')?.remove();
      const ov = document.createElement('div');
      ov.id = 'v-listen-overlay'; ov.className = 'voice-listening-overlay';
      ov.innerHTML = `
        <div class="voice-listening-pulse">🎤</div>
        <div class="voice-listening-title">Te escucho...</div>
        <div class="voice-listening-hint">Habla con naturalidad, en español.</div>
        <button class="voice-listening-cancel" id="v-cancel-listen">Cancelar</button>`;
      document.body.appendChild(ov);
      document.getElementById('v-cancel-listen')?.addEventListener('click', () => onCancel?.());
    }

    _hideOverlay() { document.getElementById('v-listen-overlay')?.remove(); }

    /* ── Waveform animation ─────────────────────────────────────────────── */
    _animate() {
      const draw = () => {
        requestAnimationFrame(draw);
        if (!this._ctx2d || !this._canvas) return;
        const W = this._canvas.width, H = this._canvas.height;
        this._ctx2d.clearRect(0, 0, W, H);
        this._t += 0.055;
        switch (this._state) {
          case 'speaking':  this._drawSpeaking(W, H); break;
          case 'listening': this._drawListening(W, H); break;
          case 'thinking':  this._drawThinking(W, H); break;
          default:          this._drawIdle(W, H);
        }
      };
      draw();
    }

    _drawIdle(W, H) {
      const c = this._ctx2d;
      c.strokeStyle = 'rgba(99,102,241,0.22)'; c.lineWidth = 1.5; c.beginPath();
      for (let x = 0; x < W; x++) {
        const y = H / 2 + Math.sin(x * 0.022 + this._t * 0.28) * 2.5;
        x === 0 ? c.moveTo(x, y) : c.lineTo(x, y);
      }
      c.stroke();
    }

    _drawSpeaking(W, H) {
      const c = this._ctx2d;
      [
        { col: 'rgba(99,102,241,0.75)',  f: 0.038, a: 13, s: 1.0  },
        { col: 'rgba(167,139,250,0.45)', f: 0.058, a: 7,  s: 1.55 },
        { col: 'rgba(34,211,238,0.3)',   f: 0.028, a: 9,  s: 0.72 },
      ].forEach(({ col, f, a, s }) => {
        c.strokeStyle = col; c.lineWidth = 2; c.beginPath();
        for (let x = 0; x < W; x++) {
          const y = H / 2 + Math.sin(x * f + this._t * s) * a + Math.sin(x * f * 1.8 + this._t * s * 0.45) * (a * 0.38);
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
        grad.addColorStop(0, 'rgba(34,211,238,0.85)');
        grad.addColorStop(0.5, 'rgba(99,102,241,0.85)');
        grad.addColorStop(1, 'rgba(34,211,238,0.85)');
        c.fillStyle = grad;
        data.forEach((v, i) => { const bh = (v / 255) * H; c.fillRect(i * bw, H - bh, Math.max(bw - 1, 1), bh); });
      } else {
        const bars = 24, bw = W / bars;
        for (let i = 0; i < bars; i++) {
          const h = (Math.sin(i * 0.55 + this._t * 2.8) * 0.5 + 0.5) * H * 0.82;
          c.fillStyle = `rgba(34,211,238,${0.35 + (h / H) * 0.55})`;
          c.fillRect(i * bw + 1, (H - h) / 2, bw - 2, h);
        }
      }
    }

    _drawThinking(W, H) {
      const c = this._ctx2d, dots = 5, sp = 18, sx = (W - (dots - 1) * sp) / 2;
      for (let i = 0; i < dots; i++) {
        const ph = this._t * 2.8 + i * 0.65;
        c.fillStyle = `rgba(245,158,11,${0.35 + Math.sin(ph) * 0.4})`;
        c.beginPath(); c.arc(sx + i * sp, H / 2 + Math.sin(ph) * 6, 4, 0, Math.PI * 2); c.fill();
      }
    }
  }

  /* ── Singleton ──────────────────────────────────────────────────────────── */
  window.voiceAssistant = new VoiceAssistant();
  const va = window.voiceAssistant;

  /* Accessors to wizard.js internal state (exposed via window._wizardState) */
  const ws = () => window._wizardState;

  /* Suppresses wizardStepChanged speaking during batch config apply */
  let _applyingConfig = false;

  /* Helper: await a va.speak() call */
  const asyncSpeak = text => new Promise(resolve => va.speak(text, resolve));

  /* ═══════════════════════════════════════════════════════════════════════════
     DASHBOARD MODE
     Voice → Groq parse → ask project name → auto-generate → terminal
     ═══════════════════════════════════════════════════════════════════════════ */
  const dashInit = document.getElementById('voice-dashboard-init');
  if (dashInit) {
    window.__voiceDashboardMode = true;
    const username = (dashInit.dataset.username || 'amigo').split(' ')[0];

    setTimeout(() => {
      va.speak(`¡Hola ${username}! ¿Qué construimos hoy? Dime tu stack completo y lo genero por ti.`);
    }, 700);

    window.__voiceDashboardListen = async function () {
      /* ── Step 1: listen for stack ── */
      const transcript = await va.listen(14000);
      if (!transcript) { va.setIdle(); return; }

      va._setState('thinking', 'Analizando...');

      /* ── Step 2: parse with Groq ── */
      let data;
      try {
        const resp = await fetch('/wizard/api/voice-parse', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ transcript }),
        });
        data = await resp.json();
      } catch {
        va.speak('Error de red. Intenta de nuevo.');
        return;
      }

      if (!data.success) {
        va.speak('No entendí el stack. Prueba: Python con FastAPI y PostgreSQL.');
        return;
      }

      /* ── Step 3: confirm parse and ask for project name ── */
      const infraLabel = data.infrastructure === 'DockerCompose' ? 'Docker Compose'
        : data.infrastructure === 'Kubernetes' ? 'Kubernetes' : 'sin contenedores';

      await asyncSpeak(`${data.architecture} con ${data.framework}, ${data.database} y ${infraLabel}. ¿Cómo se llamará el proyecto?`);

      /* ── Step 4: listen for project name ── */
      const nameText = await va.listen(10000);
      if (!nameText) {
        va.speak('Cancelado. Puedes crear el proyecto desde el wizard manualmente.');
        return;
      }

      const projectName = nameText
        .replace(/[^a-zA-Z0-9\-_\s]/g, '').trim()
        .split(/\s+/).join('-').toLowerCase().slice(0, 60);

      if (projectName.length < 2) {
        va.speak('Nombre inválido. Usa letras y números. Inténtalo de nuevo.');
        return;
      }

      /* ── Step 5: generate in background ── */
      await asyncSpeak(`Generando ${projectName}. Un momento...`);
      va._setState('thinking', 'Creando proyecto...');

      try {
        const genResp = await fetch('/wizard/api/voice-generate', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            architecture:   data.architecture,
            framework:      data.framework,
            database:       data.database,
            infrastructure: data.infrastructure || 'None',
            projectName,
          }),
        });
        const genData = await genResp.json();
        if (genData.success) {
          window.location.href = genData.redirectUrl;
        } else {
          va.speak('Error al crear el proyecto. Intenta con el wizard manual.');
        }
      } catch {
        va.speak('Error de red al generar. Intenta de nuevo.');
      }
    };

    document.getElementById('voice-dashboard-mic-btn')
      ?.addEventListener('click', window.__voiceDashboardListen);
  }

  /* ═══════════════════════════════════════════════════════════════════════════
     WIZARD MODE
     Step-by-step voice guidance. Also accepts dashboard pre-fill via sessionStorage.
     ═══════════════════════════════════════════════════════════════════════════ */
  const wizInit = document.getElementById('voice-wizard-init');
  if (wizInit) {
    window.__voiceWizardMode = true;

    const prefillRaw = sessionStorage.getItem('pf-voice-config');
    if (prefillRaw) {
      sessionStorage.removeItem('pf-voice-config');
      try {
        const cfg = JSON.parse(prefillRaw);
        requestAnimationFrame(() => requestAnimationFrame(() => _applyVoiceConfig(cfg)));
      } catch { _speakStep(); }
    } else {
      requestAnimationFrame(() => requestAnimationFrame(() => _speakStep(ws()?.currentStep || 1)));
    }

    document.addEventListener('wizardStepChanged', e => {
      if (_applyingConfig) return;
      setTimeout(() => _speakStep(e.detail?.step), 350);
    });

    window.__voiceWizardListen = async function () {
      const text = await va.listen(10000);
      if (text) _handleWizardInput(text);
      else va.setIdle();
    };
  }

  /* ── Wizard helpers ─────────────────────────────────────────────────────── */

  function _speakStep(step) {
    const s = step || ws()?.currentStep || 1;
    const arch = ws()?.architecture || '';
    const archLabel = arch === 'DotNet' ? 'punto NET' : arch;
    const prompts = {
      1: '¿Qué lenguaje prefieres? Python, Java, C Sharp, PHP, JavaScript o TypeScript.',
      2: `¿Qué framework${archLabel ? ' de ' + archLabel : ''} usarás? Dime también la base de datos.`,
      3: '¿Necesitas contenedores? Docker Compose, Kubernetes, o sin contenedores.',
      4: 'La IA ya hizo sugerencias. ¿Cambias algo?',
      5: '¿Cómo se llamará el proyecto?',
    };
    if (prompts[s]) va.speak(prompts[s]);
  }

  function _clickCard(selector, value) {
    if (!value) return false;
    const card = document.querySelector(`${selector}[data-value="${value}"]`);
    if (card) { card.click(); return true; }
    return false;
  }

  function _handleWizardInput(text) {
    const t    = text.toLowerCase();
    const step = ws()?.currentStep || 1;

    if (step === 1) {
      const arch = _parseArch(t);
      if (!arch) {
        va.speak('No reconocí el lenguaje. Prueba: Python, Java, C Sharp, PHP, JavaScript o TypeScript.');
        return;
      }
      const fw    = _parseFramework(t, arch);
      const db    = _parseDb(t);
      const infra = _parseInfra(t);

      if (fw || db) {
        _applyVoiceConfig({ architecture: arch, framework: fw, database: db, infrastructure: infra || 'None' });
      } else {
        _clickCard('.arch-card', arch);
        const label = arch === 'DotNet' ? 'punto NET' : arch;
        va.speak(`${label}.`, () => setTimeout(() => window.nextStep?.(), 300));
      }

    } else if (step === 2) {
      const arch = ws()?.architecture;
      const fw   = _parseFramework(t, arch);
      const db   = _parseDb(t);

      if (fw) _clickCard('.fw-card', fw);
      if (db) _clickCard('.db-card', db);

      if (fw || db) {
        const hasBoth = !!(ws()?.framework && ws()?.database);
        if (hasBoth) {
          va.speak('Listo.', () => setTimeout(() => window.nextStep?.(), 300));
        } else if (!ws()?.framework) {
          va.speak(`${db} seleccionada. ¿Qué framework?`);
        } else {
          va.speak(`${fw || ws()?.framework} seleccionado. ¿Qué base de datos?`);
        }
      } else {
        va.speak('No entendí. Di el framework y la base de datos, como FastAPI y PostgreSQL.');
      }

    } else if (step === 3) {
      const infra = _parseInfra(t);
      if (infra) {
        _clickCard('.infra-card', infra);
        const label = infra === 'None' ? 'sin contenedores' : infra;
        va.speak(`${label}.`, () => setTimeout(() => window.nextStep?.(), 300));
      } else {
        va.speak('Di: Docker, Kubernetes, o sin contenedores.');
      }

    } else if (step === 5) {
      const name = text.replace(/[^a-zA-Z0-9\-_\s]/g, '').trim().split(/\s+/).join('-').toLowerCase().slice(0, 60);
      if (name.length >= 2) {
        const el = document.getElementById('project-name');
        if (el) el.value = name;
        va.speak(`${name}. Presiona Generar cuando estés listo.`);
      } else {
        va.speak('El nombre debe tener al menos dos letras. ¿Cómo lo llamarás?');
      }
    }
  }

  function _applyVoiceConfig(cfg) {
    _applyingConfig = true;

    // Click arch card — triggers renderFrameworkOptions() inside wizard.js
    _clickCard('.arch-card', cfg.architecture);

    // Wait one event-loop tick; renderFrameworkOptions is sync but give DOM a tick
    setTimeout(() => {
      if (cfg.framework) _clickCard('.fw-card', cfg.framework);
      if (cfg.database)  _clickCard('.db-card', cfg.database);
      _clickCard('.infra-card', cfg.infrastructure || 'None');
      if (cfg.projectName) {
        const el = document.getElementById('project-name');
        if (el) el.value = cfg.projectName;
      }

      const hasAll = !!(ws()?.framework && ws()?.database);

      setTimeout(() => {
        const w = ws();
        if (w) {
          w.currentStep = hasAll ? 5 : 2;
          if (hasAll) window.buildSummary?.();
          window.renderStep?.();
        }
        _applyingConfig = false;
      }, 60);

      const arch = cfg.architecture || '';
      const fw   = ws()?.framework  || cfg.framework  || '';
      const db   = ws()?.database   || cfg.database   || '';
      const infraLabel = cfg.infrastructure === 'DockerCompose' ? 'Docker Compose'
        : cfg.infrastructure === 'Kubernetes' ? 'Kubernetes' : 'sin contenedores';

      if (hasAll) {
        va.speak(`${arch} con ${fw}, ${db} y ${infraLabel}. ¿Cómo se llamará el proyecto?`);
      } else {
        va.speak(`${arch} listo. Elige el framework y la base de datos.`);
      }
    }, 80);
  }

  /* ── Parsers ──────────────────────────────────────────────────────────────── */
  function _parseArch(t) {
    if (/python/.test(t))                                return 'Python';
    if (/\bjava\b(?!script)/i.test(t))                  return 'Java';
    if (/php|laravel|symfony/.test(t))                   return 'Php';
    if (/typescript|typoscript/.test(t))                 return 'TypeScript';
    if (/javascript|nodejs|node\.?js/.test(t))           return 'JavaScript';
    if (/\.?net|csharp|c.?sharp|aspnet|dotnet/.test(t)) return 'DotNet';
    if (/\bnode\b/.test(t))                              return 'JavaScript';
    return null;
  }

  function _parseFramework(t, arch) {
    const map = {
      DotNet:     { 'web.?api': 'AspNetCoreWebApi', mvc: 'AspNetCoreMVC', 'blazor.?server': 'BlazorServer', blazor: 'BlazorServer', minimal: 'MinimalApi' },
      Python:     { fastapi: 'FastAPI', django: 'Django', flask: 'Flask' },
      Java:       { spring: 'SpringBoot', quarkus: 'Quarkus', micronaut: 'Micronaut' },
      Php:        { laravel: 'Laravel', symfony: 'Symfony' },
      JavaScript: { 'nestjs|nest\\.js': 'NestJs', 'nextjs|next\\.js': 'NextJs', express: 'ExpressJs', '\\bnode\\b': 'NodeJs' },
      TypeScript: { 'nestts|nest': 'NestTs', 'nextts|next': 'NextTs' },
    };
    const m = map[arch] || {};
    for (const [pattern, val] of Object.entries(m)) {
      if (new RegExp(pattern).test(t)) return val;
    }
    return null;
  }

  function _parseDb(t) {
    if (/mysql/.test(t))              return 'MySQL';
    if (/mongo/.test(t))              return 'MongoDB';
    if (/redis/.test(t))              return 'Redis';
    if (/sqlite/.test(t))             return 'SQLite';
    if (/sql.?server|mssql/.test(t))  return 'SqlServer';
    if (/postgres/.test(t))           return 'PostgreSQL';
    return null;
  }

  function _parseInfra(t) {
    if (/kubernetes|k8s/.test(t))          return 'Kubernetes';
    if (/docker/.test(t))                  return 'DockerCompose';
    if (/sin|none|no\b|solo|local/.test(t)) return 'None';
    return null;
  }

})();