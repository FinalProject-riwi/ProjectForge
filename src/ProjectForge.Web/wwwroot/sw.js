// Minimal service worker — its only job is to satisfy the browser's PWA installability
// requirement (Chrome/Edge only offer "Install app" / the address-bar install icon for a site
// that has a registered service worker with a fetch handler). This app is mostly authenticated,
// server-rendered pages plus a live SignalR connection, so it deliberately does NOT cache
// anything: an offline-first strategy here would risk serving stale wizard/dashboard state or
// stale auth pages, which is worse than the app simply not working without a network.
self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));
self.addEventListener('fetch', event => event.respondWith(fetch(event.request)));
