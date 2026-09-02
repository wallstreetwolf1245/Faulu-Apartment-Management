const CACHE_NAME = 'faulu-v1';
const RUNTIME_CACHE = 'faulu-runtime-v1';
const ASSETS_TO_CACHE = [
  '/',
  '/index.html',
  '/favicon.svg',
  '/manifest.json',
];

const API_CACHE = 'faulu-api-v1';
const API_ROUTES = ['/api'];
const API_CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

// Install event - cache core assets
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      return cache.addAll(ASSETS_TO_CACHE).catch((err) => {
        console.warn('Failed to cache assets on install:', err);
      });
    }).then(() => {
      self.skipWaiting();
    })
  );
});

// Activate event - cleanup old caches
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames
          .filter((cacheName) => {
            return cacheName !== CACHE_NAME && 
                   cacheName !== RUNTIME_CACHE && 
                   cacheName !== API_CACHE;
          })
          .map((cacheName) => caches.delete(cacheName))
      );
    }).then(() => {
      self.clients.claim();
    })
  );
});

// Fetch event - serve from cache, fall back to network
self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  // Skip non-GET requests and extensions
  if (request.method !== 'GET') {
    return;
  }

  // Handle API requests with network-first, cache as fallback
  if (API_ROUTES.some((route) => url.pathname.startsWith(route))) {
    return event.respondWith(
      fetch(request)
        .then((response) => {
          if (!response || response.status !== 200 || response.type === 'error') {
            return response;
          }

          const responseToCache = response.clone();
          caches.open(API_CACHE).then((cache) => {
            cache.put(request, responseToCache);
          });

          return response;
        })
        .catch(() => {
          return caches.match(request).then((response) => {
            return response || new Response('Offline - Resource not cached', {
              status: 503,
              statusText: 'Service Unavailable',
            });
          });
        })
    );
  }

  // Handle static assets with cache-first strategy
  event.respondWith(
    caches.match(request).then((cachedResponse) => {
      if (cachedResponse) {
        // Update cache in background
        fetch(request)
          .then((response) => {
            if (response && response.status === 200) {
              const responseToCache = response.clone();
              caches.open(RUNTIME_CACHE).then((cache) => {
                cache.put(request, responseToCache);
              });
            }
          })
          .catch(() => {});

        return cachedResponse;
      }

      return fetch(request).then((response) => {
        if (!response || response.status !== 200 || response.type === 'error') {
          return response;
        }

        const responseToCache = response.clone();
        caches.open(RUNTIME_CACHE).then((cache) => {
          cache.put(request, responseToCache);
        });

        return response;
      }).catch(() => {
        return new Response('Offline - Page not available', {
          status: 503,
          statusText: 'Service Unavailable',
        });
      });
    })
  );
});

// Handle background sync for offline actions
self.addEventListener('sync', (event) => {
  if (event.tag === 'sync-data') {
    event.waitUntil(syncData());
  }
});

// Function to sync data when online
async function syncData() {
  try {
    const response = await fetch('/api/sync');
    if (response.ok) {
      const clients = await self.clients.matchAll();
      clients.forEach((client) => {
        client.postMessage({ type: 'SYNC_COMPLETE', data: 'Data synced' });
      });
    }
  } catch (error) {
    console.error('Sync failed:', error);
  }
}

// Handle push notifications (for future use)
self.addEventListener('push', (event) => {
  const options = {
    body: event.data ? event.data.text() : 'New notification',
    icon: '/icons/icon-192.png',
    badge: '/icons/icon-192.png',
    tag: 'notification',
  };

  event.waitUntil(
    self.registration.showNotification('Faulu Apartment Management', options)
  );
});

// Handle notification clicks
self.addEventListener('notificationclick', (event) => {
  event.notification.close();

  event.waitUntil(
    clients.matchAll({ type: 'window' }).then((clientList) => {
      for (let client of clientList) {
        if (client.url === '/' && 'focus' in client) {
          return client.focus();
        }
      }
      if (clients.openWindow) {
        return clients.openWindow('/');
      }
    })
  );
});
