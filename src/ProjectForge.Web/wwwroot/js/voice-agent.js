/* ═══════════════════════════════════════════════════════════════════════════
   ProjectForge — Dashboard voice agent (ElevenLabs Conversational AI / ElevenAgents)
   The <elevenlabs-convai> widget owns STT + understanding + TTS end-to-end.
   This file only wires the one thing the agent can't do itself: creating the
   project through our authenticated backend and navigating the browser there.
   ═══════════════════════════════════════════════════════════════════════════ */
(function () {
  'use strict';

  const el = document.getElementById('pf-voice-agent');
  if (!el) return;

  // Fired by the widget right before it opens a session — this is where we
  // hand it the client tools it's allowed to call (must match the tool name
  // configured on the agent itself).
  el.addEventListener('elevenlabs-convai:call', (event) => {
    event.detail.config.clientTools = {
      crear_proyecto: async ({ architecture, framework, database, infrastructure, projectName }) => {
        try {
          const resp = await fetch('/wizard/api/voice-generate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ architecture, framework, database, infrastructure, projectName }),
          });
          const data = await resp.json();

          if (!data.success) {
            return { success: false, message: data.error || 'No se pudo crear el proyecto.' };
          }

          // Give the agent a moment to speak its closing line before we navigate away.
          setTimeout(() => { window.location.href = data.redirectUrl; }, 1200);
          return { success: true, message: 'Proyecto creado, redirigiendo al usuario ahora mismo.' };
        } catch {
          return { success: false, message: 'Hubo un error de red al crear el proyecto.' };
        }
      },
    };
  });
})();
