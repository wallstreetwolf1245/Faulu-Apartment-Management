import sharp from 'sharp';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const iconsDir = path.join(__dirname, '..', 'public', 'icons');

async function convertIcons() {
  try {
    console.log('Converting SVG icons to PNG...\n');
    
    // Convert 192x192
    await sharp(path.join(iconsDir, 'icon-192.svg'))
      .png()
      .toFile(path.join(iconsDir, 'icon-192.png'));
    console.log('✓ icon-192.png created');

    // Convert 512x512
    await sharp(path.join(iconsDir, 'icon-512.svg'))
      .png()
      .toFile(path.join(iconsDir, 'icon-512.png'));
    console.log('✓ icon-512.png created');

    // Create maskable variants
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
    console.error('Error:', error.message);
    process.exit(1);
  }
}

convertIcons();
