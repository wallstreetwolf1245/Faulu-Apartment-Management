#!/usr/bin/env node

/**
 * PWA Icon Converter - Converts SVG icons to PNG format
 * Requires: sharp (npm install --save-dev sharp)
 * Usage: node scripts/convert-icons.mjs
 */

import sharp from 'sharp';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const iconsDir = path.join(__dirname, '..', 'public', 'icons');

async function generateIcons() {
  try {
    console.log('🎨 Converting SVG icons to PNG...\n');
    
    const conversions = [
      { input: 'icon-192.svg', output: 'icon-192.png', size: 192 },
      { input: 'icon-512.svg', output: 'icon-512.png', size: 512 },
    ];

    for (const { input, output, size } of conversions) {
      const inputPath = path.join(iconsDir, input);
      const outputPath = path.join(iconsDir, output);
      
      if (!fs.existsSync(inputPath)) {
        console.warn(`⚠️  ${input} not found, skipping...`);
        continue;
      }

      await sharp(inputPath)
        .png()
        .toFile(outputPath);
      
      console.log(`✓ ${output} created (${size}x${size})`);
    }

    // Create maskable variants
    console.log('\nCreating maskable variants...\n');
    
    const maskableConversions = [
      { input: 'icon-192.svg', output: 'icon-192-maskable.png' },
      { input: 'icon-512.svg', output: 'icon-512-maskable.png' },
    ];

    for (const { input, output } of maskableConversions) {
      const inputPath = path.join(iconsDir, input);
      const outputPath = path.join(iconsDir, output);
      
      if (!fs.existsSync(inputPath)) {
        continue;
      }

      await sharp(inputPath)
        .png()
        .toFile(outputPath);
      
      console.log(`✓ ${output} created (maskable)`);
    }
    
    console.log('\n✨ Icon conversion complete!\n');
    console.log('📂 Generated files:');
    fs.readdirSync(iconsDir)
      .filter(f => f.endsWith('.png'))
      .forEach(f => console.log(`  - ${f}`));
    
    console.log('\n🎉 PWA icons are ready!');
    console.log('   Next: npm run build && npm run preview\n');
  } catch (error) {
    console.error('❌ Error converting icons:', error.message);
    console.error('\n💡 Make sure sharp is installed: npm install --save-dev sharp');
    process.exit(1);
  }
}

generateIcons();
