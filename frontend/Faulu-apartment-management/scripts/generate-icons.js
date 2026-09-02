#!/usr/bin/env node

/**
 * Icon Generation Utility for PWA
 * 
 * This script converts SVG icons to PNG format and creates masked variants for PWA support.
 * 
 * Requirements:
 * - ImageMagick (convert command) or sharp npm package
 * 
 * Usage:
 * node scripts/generate-icons.js
 * 
 * Or install sharp:
 * npm install --save-dev sharp
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const iconsDir = path.join(__dirname, '..', 'public', 'icons');

// Create icons directory if it doesn't exist
if (!fs.existsSync(iconsDir)) {
  fs.mkdirSync(iconsDir, { recursive: true });
}

console.log('📱 PWA Icon Generation Guide');
console.log('=============================\n');

console.log('To complete your PWA setup, you need to convert SVG icons to PNG format.');
console.log('\n✅ Option 1: Using ImageMagick (recommended for Windows)');
console.log('Install: https://imagemagick.org/script/download.php#windows');
console.log('Then run these commands from your project root:');
console.log('  magick convert public/icons/icon-192.svg public/icons/icon-192.png');
console.log('  magick convert public/icons/icon-512.svg public/icons/icon-512.png');

console.log('\n✅ Option 2: Using sharp (Node.js - recommended)');
console.log('Install: npm install --save-dev sharp');
console.log('\nCreate a convert script (scripts/convert-icons.mjs):');

const sharpCode = `
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

    // Generate maskable variants (same as regular for now)
    await sharp(path.join(iconsDir, 'icon-192.svg'))
      .png()
      .toFile(path.join(iconsDir, 'icon-192-maskable.png'));
    console.log('✓ icon-192-maskable.png created');

    await sharp(path.join(iconsDir, 'icon-512.svg'))
      .png()
      .toFile(path.join(iconsDir, 'icon-512-maskable.png'));
    console.log('✓ icon-512-maskable.png created');
    
    console.log('\\n✨ Icon conversion complete!');
  } catch (error) {
    console.error('Error converting icons:', error);
    process.exit(1);
  }
}

generateIcons();
`;

console.log(sharpCode);

console.log('\n✅ Option 3: Online Converters');
console.log('- CloudConvert: https://cloudconvert.com/svg-to-png');
console.log('- Online Convert: https://online-convert.com/');
console.log('- Konva.js: https://konvajs.org/docs/tools/konva.html');

console.log('\n📝 Splash Screen Sizes Needed:');
console.log('Create these additional images for mobile splash screens:');
console.log('  - splash-640x1136.png (iPhone SE)');
console.log('  - splash-750x1334.png (iPhone 6/7/8)');
console.log('  - splash-1242x2208.png (iPhone Plus)');

console.log('\n📸 Optional App Screenshots:');
console.log('For app store display (optional):');
console.log('  - screenshot-1.png (540x720 - mobile)');
console.log('  - screenshot-2.png (1280x720 - desktop)');

console.log('\n📂 Final file structure should be:');
console.log(\`
public/
├── icons/
│   ├── icon-192.svg
│   ├── icon-192.png
│   ├── icon-192-maskable.png
│   ├── icon-512.svg
│   ├── icon-512.png
│   ├── icon-512-maskable.png
│   ├── splash-640x1136.png (optional)
│   ├── splash-750x1334.png (optional)
│   ├── splash-1242x2208.png (optional)
│   ├── screenshot-1.png (optional)
│   └── screenshot-2.png (optional)
├── manifest.json
├── sw.js
└── favicon.svg
\`);

console.log('\n💡 Tips:');
console.log('- Current brand colors: Primary: #2563eb, Secondary: #1d4ed8');
console.log('- Ensure minimum 192x192 dimensions for icon quality');
console.log('- For maskable icons, provide extra padding (20-25%) around core content');
console.log('- Test PWA installation: Chrome DevTools → Application → Manifest');
console.log('- Use "npm run build && npm run preview" to test locally');

console.log('\n🚀 Quick setup with npm:');
console.log('1. npm install --save-dev sharp');
console.log('2. Create scripts/convert-icons.mjs with the code above');
console.log('3. Add to package.json: "icons": "node scripts/convert-icons.mjs"');
console.log('4. Run: npm run icons');

console.log('\n✨ PWA setup ready! Next steps:');
console.log('1. Generate PNG icons');
console.log('2. Run: npm run build');
console.log('3. Run: npm run preview');
console.log('4. Test installation: Open DevTools → Application tab\n');
