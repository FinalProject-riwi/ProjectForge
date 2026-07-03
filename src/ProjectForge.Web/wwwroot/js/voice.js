/* ═══════════════════════════════════════════════════════════════════════════
   ProjectForge — Voice Assistant  v4
   TTS  : ElevenLabs (eleven_multilingual_v2) → fallback SpeechSynthesis
   STT  : Web Speech API  (Chrome / Edge)
   Scope: ONLY project creation — out-of-scope answered warmly and redirected.
   Flow : Free natural speech → Groq parses → auto-generate → terminal
   ═══════════════════════════════════════════════════════════════════════════ */
(function () {
  'use strict';

  const hasTTS    = typeof window.speechSynthesis !== 'undefined';
  const SpeechRec = window.SpeechRecognition || window.webkitSpeechRecognition;
  const hasSTT    = !!SpeechRec;

  /* ── Preferred recognition languages (try in order) ────────────────────── */
  const STT_LANGS = ['es-US', 'es-ES', 'es-MX', 'es'];

  /* ═══════════════════════════════════════════════════════════════════════════
     VoiceAssistant
     ═══════════════════════════════════════════════════════════════════════════ */
  class VoiceAssistant {
    constructor() {
      this._state        = 'idle';
      this._muted        = localStorage.getItem('pf-voice-muted') === '1';
      this._fbVoice      = null;
      this._rec          = null;
      this._recLangIdx   = 0;
      this._audioCtx     = null;
      this._analyser     = null;
      this._micStream    = null;
      this._curAudio     = null;
      this._canvas       = null;
      this._ctx2d        = null;
      this._t            = 0;

      if (hasTTS) this._loadFbVoice();
      if (hasSTT) this._initSTT();
      this._createBar();
    }

    /* ── Fallback voice ─────────────────────────────────────────────────── */
    _loadFbVoice() {
      const pick = () => {
        const v = window.speechSynthesis.getVoices();
        const pref = [
          'Microsoft Sabina Online (Natural) - Spanish (Mexico)',
          'Microsoft Sabina - Spanish (Mexico)',
          'Google español de Estados Unidos',
          'Paulina', 'Mónica', 'Monica', 'Luciana', 'Helena', 'Google español',
        ];
        this._fbVoice =
          v.find(x => pref.includes(x.name)) ||
          v.find(x => x.lang.startsWith('es') && /online|neural|natural/i.test(x.name)) ||
          v.find(x => x.lang.startsWith('es') && /sabina|paulina|monica|luciana|helena/i.test(x.name)) ||
          v.find(x => x.lang.startsWith('es') && /google/i.test(x.name)) ||
          v.find(x => x.lang.startsWith('es')) || null;
      };
      if (window.speechSynthesis.getVoices().length) pick();
      window.speechSynthesis.addEventListener('voiceschanged', pick);
    }

    /* ── STT init ───────────────────────────────────────────────────────── */
    _initSTT() {
      try {
        this._rec = new SpeechRec();
        this._rec.continuous     = false;
        this._rec.interimResults = false;
        this._rec.lang           = STT_LANGS[0];
        this._rec.maxAlternatives = 3;
      } catch { this._rec = null; }
    }

    /* ── TTS: ElevenLabs → SpeechSynthesis fallback ─────────────────────── */
    speak(text, onEnd) {
      if (!text || this._muted) { setTimeout(() => onEnd?.(), 0); return; }
      this._cancelAudio(); window.speechSynthesis?.cancel();
      this._setState('speaking', 'Hablando...');

      const audio = new Audio(`/wizard/api/tts?text=${encodeURIComponent(text.trim().slice(0, 500))}`);
      this._curAudio = audio;
      audio.onended = () => { this._curAudio = null; this._setState('idle', 'Lista'); onEnd?.(); };
      audio.onerror = () => { this._curAudio = null; this._fbSpeak(text, onEnd); };
      audio.play().catch(() => { this._curAudio = null; this._fbSpeak(text, onEnd); });
    }

    _fbSpeak(text, onEnd) {
      if (!hasTTS) { this._setState('idle', 'Lista'); onEnd?.(); return; }
      this._setState('speaking', 'Hablando...');
      const u = new SpeechSynthesisUtterance(text);
      if (this._fbVoice) u.voice = this._fbVoice;
      u.lang = 'es-MX'; u.rate = 0.87; u.pitch = 0.95; u.volume = 1;
      u.onend = u.onerror = () => { this._setState('idle', 'Lista'); onEnd?.(); };
      window.speechSynthesis.speak(u);
    }

    _cancelAudio() {
      if (!this._curAudio) return;
      this._curAudio.onended = this._curAudio.onerror = null;
      this._curAudio.pause();
      this._curAudio = null;
    }

    stopSpeaking() { this._cancelAudio(); window.speechSynthesis?.cancel(); this._setState('idle', 'Lista'); }

    /* ── STT: listen ────────────────────────────────────────────────────── */
    listen(timeoutMs = 12000) {
      if (!hasSTT || !this._rec) return Promise.resolve(null);
      return new Promise(resolve => {
        let settled = false;
        const done = val => {
          if (settled) return; settled = true;
          clearTimeout(timer);
          this._hideOverlay(); this._stopMic();
          // Try rotating language on failure for better recognition
          if (!val) this._recLangIdx = (this._recLangIdx + 1) % STT_LANGS.length;
          this._setState(val ? 'thinking' : 'idle', val ? 'Procesando...' : 'Lista');
          resolve(val);
        };

        this._setState('listening', 'Escuchando...');
        this._showOverlay(() => { try { this._rec?.stop(); } catch {} done(null); });
        this._startMic();

        const timer = setTimeout(() => { try { this._rec?.stop(); } catch {} }, timeoutMs);

        this._rec.lang = STT_LANGS[this._recLangIdx];
        this._rec.onresult = e => {
          // Use the best alternative with highest confidence
          const best = [...Array(e.results[0].length)]
            .map((_, i) => e.results[0][i])
            .sort((a, b) => b.confidence - a.confidence)[0];
          done(best?.transcript || null);
        };
        this._rec.onerror = () => done(null);
        this._rec.onend   = () => done(null);
        try { this._rec.start(); } catch { done(null); }
      });
    }

    stopListening() {
      try { this._rec?.stop(); } catch {}
      this._hideOverlay(); this._stopMic(); this._setState('idle', 'Lista');
    }

    setIdle(t = 'Lista') { this._setState('idle', t); }
    setState(s, t) { this._setState(s, t); }  // public alias for external callers

    /* ── Mic visualizer ─────────────────────────────────────────────────── */
    async _startMic() {
      try {
        this._audioCtx  = this._audioCtx || new (window.AudioContext || window.webkitAudioContext)();
        this._micStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
        this._analyser  = this._audioCtx.createAnalyser(); this._analyser.fftSize = 128;
        this._audioCtx.createMediaStreamSource(this._micStream).connect(this._analyser);
      } catch { /* mic denied — animated fallback */ }
    }

    _stopMic() { this._micStream?.getTracks().forEach(t => t.stop()); this._micStream = null; this._analyser = null; }

    /* ── Voice bar ──────────────────────────────────────────────────────── */
    _createBar() {
      const bar = Object.assign(document.createElement('div'), {
        id: 'voice-bar', className: 'voice-bar',
      });
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
            <button class="voice-mute-btn${this._muted ? ' muted' : ''}" id="v-mute-btn" title="Silenciar" aria-label="Silenciar">
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
        this._ctx2d.clearRect(0, 0, W, H); this._t += 0.055;
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
      [
        { col: 'rgba(99,102,241,0.75)',  f: 0.038, a: 13, s: 1.0  },
        { col: 'rgba(167,139,250,0.45)', f: 0.058, a: 7,  s: 1.55 },
        { col: 'rgba(34,211,238,0.3)',   f: 0.028, a: 9,  s: 0.72 },
      ].forEach(({ col, f, a, s }) => {
        const c = this._ctx2d;
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
        const d = new Uint8Array(this._analyser.frequencyBinCount);
        this._analyser.getByteFrequencyData(d);
        const bw = W / d.length, g = c.createLinearGradient(0, 0, W, 0);
        g.addColorStop(0, 'rgba(34,211,238,0.85)'); g.addColorStop(0.5, 'rgba(99,102,241,0.85)'); g.addColorStop(1, 'rgba(34,211,238,0.85)');
        c.fillStyle = g;
        d.forEach((v, i) => { const bh = (v / 255) * H; c.fillRect(i * bw, H - bh, Math.max(bw - 1, 1), bh); });
      } else {
        const bars = 24, bw = W / bars;
        for (let i = 0; i < bars; i++) {
          const h = (Math.sin(i * 0.55 + this._t * 2.8) * 0.5 + 0.5) * H * 0.82;
          this._ctx2d.fillStyle = `rgba(34,211,238,${0.35 + (h / H) * 0.55})`;
          this._ctx2d.fillRect(i * bw + 1, (H - h) / 2, bw - 2, h);
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

  /* Wizard state accessor — safe on non-wizard pages */
  const ws = () => window._wizardState;
  let _applyingConfig = false;
  const asyncSpeak = text => new Promise(resolve => va.speak(text, resolve));

  /* ═══════════════════════════════════════════════════════════════════════════
     DASHBOARD MODE
     Free conversational voice → Groq parses → confirms → name → generate → terminal
     ═══════════════════════════════════════════════════════════════════════════ */
  const dashInit = document.getElementById('voice-dashboard-init');
  if (dashInit) {
    window.__voiceDashboardMode = true;
    const username = (dashInit.dataset.username || 'amigo').split(' ')[0];

    setTimeout(() => {
      va.speak(`¡Hola ${username}! Soy tu asistente de TabBuilder. Cuéntame qué tipo de proyecto quieres crear.`);
    }, 700);

    window.__voiceDashboardListen = async function () {
      /* ── Turn 1: user describes what they want ── */
      const transcript = await va.listen(18000);
      if (!transcript) { va.setIdle(); return; }

      va.setState('thinking', 'Analizando...');

      let data;
      try {
        const resp = await fetch('/wizard/api/voice-parse', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ transcript }),
        });
        data = await resp.json();
      } catch {
        va.speak('Tuve un problema de red. ¿Puedes intentarlo de nuevo?');
        return;
      }

      /* ── Out of scope: gentle redirect ── */
      if (!data.inScope) {
        await asyncSpeak(data.outOfScopeReply || 'Solo puedo ayudarte a crear proyectos de software con ProjectForge. ¿Quieres que creemos uno?');
        // Give them a chance to reply with a project request
        const retry = await va.listen(12000);
        if (retry) window.__voiceDashboardListen();  // restart flow with their new message
        return;
      }

      /* ── Build a natural confirmation ── */
      const archLabel = _archLabel(data.architecture);
      const infraLabel = _infraLabel(data.infrastructure);
      const fwLabel   = data.framework || archLabel;

      /* ── If Groq already extracted a project name, skip that turn ── */
      if (data.projectName && data.projectName.length >= 2) {
        await asyncSpeak(`Perfecto. ${archLabel} con ${fwLabel}, ${data.database} y ${infraLabel}. Proyecto: ${data.projectName}. Generando...`);
        await _generate(data, data.projectName);
        return;
      }

      /* ── Ask for project name naturally ── */
      await asyncSpeak(`Entendido. ${archLabel} con ${fwLabel} y ${data.database}. ¿Cómo quieres llamar el proyecto?`);

      /* ── Turn 2: project name ── */
      const nameRaw = await va.listen(12000);
      if (!nameRaw) {
        va.speak('No escuché el nombre. Puedes continuar en el wizard manualmente.');
        return;
      }

      const projectName = _extractName(nameRaw);
      if (!projectName) {
        va.speak('No pude entender el nombre. Usa solo letras, números y guiones.');
        return;
      }

      await asyncSpeak(`Generando ${projectName}...`);
      await _generate(data, projectName);
    };

    document.getElementById('voice-dashboard-mic-btn')
      ?.addEventListener('click', window.__voiceDashboardListen);
  }

  /* ── Background generation helper ──────────────────────────────────────── */
  async function _generate(data, projectName) {
    va.setState('thinking', 'Creando proyecto...');
    try {
      const resp = await fetch('/wizard/api/voice-generate', {
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
      const result = await resp.json();
      if (result.success) {
        window.location.href = result.redirectUrl;
      } else {
        va.speak('Algo salió mal al crear el proyecto. Puedes intentarlo en el wizard.');
      }
    } catch {
      va.speak('Error de red. Intenta de nuevo o usa el wizard manualmente.');
    }
  }

  /* ── Extract a clean project name from natural speech ─────────────────── */
  function _extractName(raw) {
    // Strip common "name" prefix phrases in Spanish
    const clean = raw
      .replace(/^(qu[eé] se llame?|ll[aá]malo|ll[aá]mala|el nombre es|se llamar[aá]|llamado|llamada|se llama|lo llamamos|ponle|puedes llamarlo|quiero que se llame)\s+/i, '')
      .replace(/[^a-zA-Z0-9\-_áéíóúñÁÉÍÓÚÑ\s]/g, '')
      .trim()
      .replace(/\s+/g, '-')
      .toLowerCase()
      .slice(0, 60);

    // Remove accents for valid folder/repo names
    return clean.normalize('NFD').replace(/[̀-ͯ]/g, '').replace(/[^a-z0-9\-_]/g, '') || null;
  }

  function _archLabel(arch) {
    return { DotNet: 'C Sharp .NET', Java: 'Java', Python: 'Python', Php: 'PHP', JavaScript: 'JavaScript', TypeScript: 'TypeScript' }[arch] || arch;
  }

  function _infraLabel(infra) {
    return { DockerCompose: 'Docker Compose', Kubernetes: 'Kubernetes', None: 'sin contenedores' }[infra] || 'sin contenedores';
  }

  /* ═══════════════════════════════════════════════════════════════════════════
     WIZARD MODE
     Step-by-step voice guidance (keeps working for manual users who open /wizard directly)
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
      const text = await va.listen(12000);
      if (!text) { va.setIdle(); return; }

      /* Check scope even in wizard */
      va.setState('thinking', 'Procesando...');
      try {
        const resp = await fetch('/wizard/api/voice-parse', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ transcript: text }),
        });
        const data = await resp.json();
        if (!data.inScope) {
          va.speak(data.outOfScopeReply || 'Solo puedo ayudarte con el proyecto. ¿Continuamos?');
          return;
        }
        _handleWizardInput(text, data);
      } catch {
        _handleWizardInput(text, null);
      }
    };
  }

  /* ── Wizard step speaker ────────────────────────────────────────────────── */
  function _speakStep(step) {
    const s = step || ws()?.currentStep || 1;
    const arch = ws()?.architecture || '';
    const al = arch === 'DotNet' ? 'punto NET' : arch;
    const prompts = {
      1: '¿Qué tecnología quieres usar? Por ejemplo: Python, Java, Node, C Sharp, PHP...',
      2: `¿Qué framework${al ? ' de ' + al : ''} prefieres? Y dime la base de datos.`,
      3: '¿Necesitas contenedores? Docker, Kubernetes, o sin contenedores.',
      4: 'La IA ya sugirió patrones y librerías. ¿Cambias algo?',
      5: '¿Cómo se llamará el proyecto?',
    };
    if (prompts[s]) va.speak(prompts[s]);
  }

  /* ── Wizard input handler (uses parsed Groq data when available) ─────── */
  function _handleWizardInput(text, parsedData) {
    const t    = text.toLowerCase();
    const step = ws()?.currentStep || 1;

    if (step === 1) {
      const arch = parsedData?.architecture || _parseArch(t);
      if (!arch) { va.speak('No reconocí el lenguaje. ¿Python, Java, Node, C Sharp o PHP?'); return; }

      const fw    = parsedData?.framework    || _parseFramework(t, arch);
      const db    = parsedData?.database     || _parseDb(t);
      const infra = parsedData?.infrastructure || _parseInfra(t);

      if (fw || db) {
        _applyVoiceConfig({ architecture: arch, framework: fw, database: db, infrastructure: infra || 'None' });
      } else {
        _clickCard('.arch-card', arch);
        va.speak(`${_archLabel(arch)}.`, () => setTimeout(() => window.nextStep?.(), 300));
      }

    } else if (step === 2) {
      const arch = ws()?.architecture;
      const fw   = parsedData?.framework || _parseFramework(t, arch);
      const db   = parsedData?.database  || _parseDb(t);

      if (fw) _clickCard('.fw-card', fw);
      if (db) _clickCard('.db-card', db);

      if (fw || db) {
        const hasBoth = !!(ws()?.framework && ws()?.database);
        if (hasBoth) {
          va.speak('Listo.', () => setTimeout(() => window.nextStep?.(), 300));
        } else if (!ws()?.framework) {
          va.speak(`${db} lista. ¿Qué framework?`);
        } else {
          va.speak(`${fw || ws()?.framework} listo. ¿Qué base de datos?`);
        }
      } else {
        va.speak('Dime el framework y la base de datos.');
      }

    } else if (step === 3) {
      const infra = parsedData?.infrastructure || _parseInfra(t);
      if (infra) {
        _clickCard('.infra-card', infra);
        va.speak(`${_infraLabel(infra)}.`, () => setTimeout(() => window.nextStep?.(), 300));
      } else {
        va.speak('Di Docker, Kubernetes, o sin contenedores.');
      }

    } else if (step === 5) {
      const name = parsedData?.projectName || _extractName(text);
      if (name && name.length >= 2) {
        const el = document.getElementById('project-name');
        if (el) el.value = name;
        va.speak(`${name}. Cuando estés listo, presiona Generar.`);
      } else {
        va.speak('¿Cómo se llamará el proyecto?');
      }
    }
  }

  function _applyVoiceConfig(cfg) {
    _applyingConfig = true;
    _clickCard('.arch-card', cfg.architecture);
    setTimeout(() => {
      if (cfg.framework) _clickCard('.fw-card', cfg.framework);
      if (cfg.database)  _clickCard('.db-card', cfg.database);
      _clickCard('.infra-card', cfg.infrastructure || 'None');
      if (cfg.projectName) { const el = document.getElementById('project-name'); if (el) el.value = cfg.projectName; }

      const hasAll = !!(ws()?.framework && ws()?.database);
      setTimeout(() => {
        const w = ws();
        if (w) { w.currentStep = hasAll ? 5 : 2; if (hasAll) window.buildSummary?.(); window.renderStep?.(); }
        _applyingConfig = false;
      }, 60);

      const arch = cfg.architecture || '', fw = ws()?.framework || cfg.framework || '', db = ws()?.database || cfg.database || '';
      if (hasAll) {
        va.speak(`${_archLabel(arch)} con ${fw}, ${db} y ${_infraLabel(cfg.infrastructure)}. ¿Cómo se llamará el proyecto?`);
      } else {
        va.speak(`${_archLabel(arch)} seleccionado. Elige el framework y la base de datos.`);
      }
    }, 80);
  }

  function _clickCard(selector, value) {
    if (!value) return false;
    const card = document.querySelector(`${selector}[data-value="${value}"]`);
    if (card) { card.click(); return true; }
    return false;
  }

  /* ── Parsers (fallback when Groq is unavailable) ─────────────────────── */
  function _parseArch(t) {
    if (/python/.test(t)) return 'Python';
    if (/\bjava\b(?!script)/i.test(t)) return 'Java';
    if (/php|laravel|symfony/.test(t)) return 'Php';
    if (/typescript|typoscript/.test(t)) return 'TypeScript';
    if (/javascript|nodejs|node\.?js/.test(t)) return 'JavaScript';
    if (/\.?net|csharp|c.?sharp|aspnet|dotnet/.test(t)) return 'DotNet';
    if (/\bnode\b/.test(t)) return 'JavaScript';
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
    for (const [pat, val] of Object.entries(m)) if (new RegExp(pat).test(t)) return val;
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
    if (/sin|none|no\b|solo|local/.test(t)) return 'None';
    return null;
  }

})();