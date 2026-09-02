# Progressive Web App (PWA) Setup Guide

Your **Faulu Apartment Management** application is now configured as a Progressive Web App! 🎉

## What is a PWA?

A Progressive Web App combines the best features of web and mobile apps:
- **Installable** - Add to home screen like a native app
- **Offline Capable** - Works without internet connection
- **Fast** - Service worker caching for quick load times
- **Responsive** - Works on any device
- **Secure** - Served over HTTPS in production

## Current Setup

✅ **Already Configured:**
- Service Worker (`public/sw.js`) - Handles caching and offline support
- Web App Manifest (`public/manifest.json`) - App metadata and configuration
- PWA Meta Tags - Added to `index.html` for mobile installation
- Cache Strategies:
  - **Static Assets**: Cache-first (fast loading with background updates)
  - **API Calls**: Network-first (fresh data when online, cached fallback)

## Next Steps: Icon Generation

Before testing the PWA, you need to convert SVG icons to PNG format.

### Step 1: Install Sharp (Recommended)

Sharp is a fast Node.js image library perfect for PWA icon conversion.

```bash
npm install --save-dev sharp
```

### Step 2: Create Icon Conversion Script

Create `scripts/convert-icons.mjs`:

```javascript
import sharp from 'sharp';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const iconsDir = path.join(__dirname, '..', 'public', 'icons');

async function generateIcons() {
  try {
    console.log('Converting SVG icons to PNG...');
    
    // Generate 192x192 icons
    await sharp(path.join(iconsDir, 'icon-192.svg'))
      .png()
      .toFile(path.join(iconsDir, 'icon-192.png'));
    console.log('✓ icon-192.png created');

    // Generate 512x512 icons
    await sharp(path.join(iconsDir, 'icon-512.svg'))
      .png()
      .toFile(path.join(iconsDir, 'icon-512.png'));
    console.log('✓ icon-512.png created');

    // Generate maskable variants
    await sharp(path.join(iconsDir, 'icon-192.svg'))
      .png()
      .toFile(path.join(iconsDir, 'icon-192-maskable.png'));
    console.log('✓ icon-192-maskable.png created');

    await sharp(path.join(iconsDir, 'icon-512.svg'))
      .png()
      .toFile(path.join(iconsDir, 'icon-512-maskable.png'));
    console.log('✓ icon-512-maskable.png created');
    
    console.log('\n✨ Icon conversion complete!');
  } catch (error) {
    console.error('Error converting icons:', error);
    process.exit(1);
  }
}

generateIcons();
```

### Step 3: Add NPM Script

Add this to `package.json` scripts:

```json
{
  "scripts": {
    "icons": "node scripts/convert-icons.mjs"
  }
}
```

### Step 4: Generate Icons

```bash
npm run icons
```

This creates:
- `public/icons/icon-192.png`
- `public/icons/icon-192-maskable.png`
- `public/icons/icon-512.png`
- `public/icons/icon-512-maskable.png`

## Testing Your PWA

### Build for Production

```bash
npm run build
npm run preview
```

### Test Installation (Chrome/Edge)

1. Open the app in your browser
2. Look for the "Install" button in the address bar
3. Or: DevTools → Application tab → Manifest section → "Install" button

### Check PWA Status

**Chrome/Edge DevTools:**
1. Open DevTools (F12)
2. Go to "Application" tab
3. Check under "Manifest" - should see all your app info
4. Check "Service Workers" - should show registered worker

**Lighthouse Audit:**
1. DevTools → Lighthouse tab
2. Run PWA audit
3. Should achieve 90+ PWA score

## Service Worker Features

### Current Caching Strategies

```
Static Assets (HTML, CSS, JS, Images)
├─ Cache first: Serve from cache, update in background
├─ Fallback: Serve offline page if not cached
└─ Works: Fast loading, reliable offline experience

API Calls (/api/*)
├─ Network first: Try to fetch fresh data
├─ Fallback: Serve from API cache if offline
└─ Works: Always latest data when online, functional offline
```

### Offline Handling

When offline, the app will:
- ✅ Load all cached pages and assets instantly
- ✅ Show cached API data from recent requests
- ✅ Display helpful "Offline" messages for missing resources
- ⏳ Queue actions for sync when back online (future feature)

## Customization

### Change App Colors

Edit `public/manifest.json`:

```json
{
  "theme_color": "#2563eb",      // URL bar color on Android
  "background_color": "#ffffff"   // Splash screen background
}
```

Edit `index.html`:
```html
<meta name="theme-color" content="#2563eb" />
```

### Change App Name

Edit `public/manifest.json`:

```json
{
  "name": "Your App Name",
  "short_name": "Short Name",
  "description": "Your app description"
}
```

### Add App Shortcuts

Edit `public/manifest.json` → `shortcuts` array. Example:

```json
{
  "shortcuts": [
    {
      "name": "View Dashboard",
      "short_name": "Dashboard",
      "url": "/dashboard",
      "icons": [{"src": "/icons/icon-192.png", "sizes": "192x192"}]
    }
  ]
}
```

## Mobile Splash Screens (iOS)

For iOS, create splash screen images and add to `index.html`:

**Image Sizes Needed:**
- 640x1136 (iPhone SE)
- 750x1334 (iPhone 6/7/8)
- 1242x2208 (iPhone Plus)

**HTML Links (already in index.html):**
```html
<link rel="apple-touch-startup-image" href="/icons/splash-640x1136.png" 
  media="(device-width: 320px) and (device-height: 568px) and (-webkit-device-pixel-ratio: 2)" />
```

## Background Sync (Future)

The service worker supports background sync. When online again, pending actions can be synced:

```javascript
// Listen for sync completion
window.addEventListener('pwa-sync-complete', (event) => {
  console.log('Data synced:', event.detail);
});

// Request sync
if ('serviceWorker' in navigator && 'SyncManager' in window) {
  navigator.serviceWorker.ready.then((registration) => {
    registration.sync.register('sync-data');
  });
}
```

## Push Notifications (Future)

The service worker handles push notifications. To enable:

```javascript
// Request notification permission
Notification.requestPermission().then((permission) => {
  if (permission === 'granted') {
    // User granted permission
  }
});
```

## Deployment Checklist

Before deploying to production:

- [ ] Convert SVG icons to PNG (`npm run icons`)
- [ ] Run `npm run build` and verify no errors
- [ ] Test PWA installation works
- [ ] Verify service worker registration in DevTools
- [ ] Run Lighthouse PWA audit (aim for 90+)
- [ ] Test offline functionality
- [ ] Verify all API caching works
- [ ] Deploy with HTTPS (required for PWA)
- [ ] Add security headers (CSP, etc.)

## Troubleshooting

### "Install button not showing"
- Ensure HTTPS (localhost works for testing)
- Check manifest.json is valid (DevTools → Application → Manifest)
- Verify at least 192x192 icon exists
- Check browser console for errors

### "Service Worker not registering"
- Check browser console for errors
- Verify `public/sw.js` exists
- Ensure no syntax errors in `sw.js`
- Clear site data and reload (DevTools → Application → Storage → Clear)

### "Offline page showing for online resources"
- Check network tab in DevTools
- Verify API routes are `/api/*` prefix
- Check service worker caching strategy in `sw.js`

### "Icons showing as broken"
- Verify PNG files exist in `public/icons/`
- Check image paths in `manifest.json`
- Ensure images are valid PNG format
- Clear cache: DevTools → Application → Storage → Clear

## File Structure

```
project/
├── public/
│   ├── manifest.json          # PWA configuration
│   ├── sw.js                  # Service Worker
│   ├── favicon.svg            # Browser tab icon
│   └── icons/                 # App icons
│       ├── icon-192.svg       # Source (convert to PNG)
│       ├── icon-192.png       # Generated PNG
│       ├── icon-192-maskable.png
│       ├── icon-512.svg       # Source (convert to PNG)
│       ├── icon-512.png       # Generated PNG
│       └── icon-512-maskable.png
├── src/
│   ├── main.jsx
│   ├── App.jsx
│   └── ...
├── index.html                 # Has PWA meta tags + SW registration
├── package.json               # Update with icons script
└── vite.config.js
```

## Resources

- [MDN: Progressive Web Apps](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps)
- [Web.dev: PWA Checklist](https://web.dev/pwa-checklist/)
- [Chrome DevTools: PWA](https://developer.chrome.com/docs/devtools/progressive-web-apps/)
- [Service Workers API](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)

## Support

For questions about PWA:
- Check browser console for errors
- Use Lighthouse audit for detailed feedback
- Verify all manifest fields in DevTools
- Test in production mode: `npm run build && npm run preview`

Happy PWA development! 🚀
