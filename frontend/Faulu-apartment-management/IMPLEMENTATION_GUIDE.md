# Color Palette Implementation Guide

## Quick Start

The Faulu Apartment Management app now uses a professional blue gradient color palette consistently across all components.

### What Changed?

1. ✅ **Primary brand color** updated to `#83badb` (medium blue)
2. ✅ **9-level color scale** added for consistent styling
3. ✅ **CSS variables** automatically applied to all components
4. ✅ **Utility classes** available for quick color application
5. ✅ **Color guide** documented for reference

---

## How to Use

### Method 1: CSS Variables (Recommended)

Use CSS variables for consistent, maintainable styling:

```css
/* In your .css file */
.my-component {
  background-color: var(--color-blue-50);
  border: 1px solid var(--color-blue-100);
  color: var(--color-text-primary);
}

.my-component:hover {
  background-color: var(--color-blue-100);
  border-color: var(--color-blue-200);
}
```

### Method 2: Utility Classes

Use pre-built utility classes for rapid development:

```jsx
// Background colors
<div className="bg-blue-50">Light blue background</div>
<div className="bg-blue-400">Medium blue background</div>

// Text colors
<div className="text-blue-900">Dark blue text</div>

// Borders
<div className="border-blue-200">Light blue border</div>

// Combinations
<div className="bg-blue-400-text-white">Blue button-like element</div>

// Gradients
<div className="gradient-blue-400-600">Blue gradient</div>
```

### Method 3: Custom Components

```jsx
// React component using CSS classes
function PropertyCard({ title, value }) {
  return (
    <div className="card card-blue-subtle card-blue-highlight">
      <div className="card-body">
        <h3 className="text-blue-900">{title}</h3>
        <p className="text-blue-700">{value}</p>
      </div>
    </div>
  );
}
```

---

## Color Application Examples

### Dashboard Cards

**Option 1: CSS Variables**
```css
.summary-card.properties {
  border-left-color: var(--color-primary);
  background: rgba(131, 186, 219, 0.05);
}
```

**Option 2: Utility Classes**
```jsx
<div className="summary-card properties card-blue-highlight">
  {/* content */}
</div>
```

### Buttons

**Primary Button**
```css
.btn-primary {
  background-color: var(--color-primary);
  color: white;
}

.btn-primary:hover {
  background-color: var(--color-primary-hover);
}
```

**Or with utility class:**
```jsx
<button className="btn btn-blue-primary">Save</button>
<button className="btn btn-blue-outline">Cancel</button>
```

### Form Inputs

**CSS Variable approach:**
```css
input:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-blue-50);
}
```

**Or with utility:**
```jsx
<input className="input-focus-blue" />
```

---

## Color Levels Explained

| Level | Shade | Best For |
|-------|-------|----------|
| **50** | Lightest | Page backgrounds, subtle overlays |
| **100** | Light | Card backgrounds, hover states |
| **200** | Light-Med | Borders, input borders, disabled states |
| **300** | Medium | Secondary buttons, dividers |
| **400** | Main Brand | Primary buttons, active states, links |
| **500** | Medium-Dark | Hover states for primary elements |
| **600** | Dark | Active/pressed states |
| **700** | Darker | Disabled text, strong borders |
| **800** | Very Dark | Text emphasis, strong contrast |
| **900** | Darkest | Headers, main text color |

---

## Responsive & State Examples

### Active State
```jsx
<div style={{ 
  borderLeftColor: 'var(--color-primary)',
  backgroundColor: 'var(--color-blue-50)'
}}>
  Active item
</div>
```

### Hover State
```css
.nav-item:hover {
  background-color: var(--color-blue-100);
  color: var(--color-primary);
}
```

### Disabled State
```css
.input:disabled {
  background-color: var(--color-blue-50);
  color: var(--color-blue-300);
  border-color: var(--color-blue-200);
}
```

### Focus State
```css
button:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}
```

---

## Component-Specific Guidelines

### Navigation Items
- Default: Gray text on white background
- Hover: `--color-blue-100` background
- Active: `--color-primary` color or bottom border

### Cards
- Background: White or `--color-blue-50`
- Border: `--color-blue-100` or `--color-blue-200`
- Left accent: `--color-primary` (4px border)

### Buttons
- Primary: `--color-blue-400` with white text
- Secondary: `--color-blue-100` with `--color-blue-900` text
- Outline: Transparent with `--color-blue-400` border

### Modals
- Background: White
- Header border: `--color-blue-100`
- Action buttons: Follow button guidelines

### Form Elements
- Border: `--color-border-default` (gray)
- Focus border: `--color-primary`
- Focus shadow: `rgba(131, 186, 219, 0.3)`

---

## Testing Your Changes

After applying the color palette, verify:

1. ✅ All buttons render with blue gradient
2. ✅ Input fields have blue focus state
3. ✅ Cards have consistent blue accents
4. ✅ Navigation highlights in blue
5. ✅ Hover states show lighter blue
6. ✅ Disabled elements are grayed appropriately
7. ✅ Text contrast meets WCAG AA standards
8. ✅ Mobile layout maintains color consistency

---

## Troubleshooting

### Colors not applying?
- Ensure `color-utilities.css` is imported in `main.jsx` ✓
- Check that CSS class names are spelled correctly
- Verify variable names use double hyphens: `--color-blue-400`

### Bootstrap conflicts?
- Bootstrap is imported before custom CSS
- Override Bootstrap classes with specificity if needed:
  ```css
  .btn-primary {
    background-color: var(--color-primary) !important;
  }
  ```

### Need to update colors?
- Edit `src/variables.css` - all components will update automatically
- Only change `--color-primary` and `--color-primary-hover` if needed

---

## Color Palette Reference

```
Primary Blue Palette (Faulu Professional)
=========================================

Lightest  →  #e6f1f7  (--color-blue-50)
           ↓
           #cde3f0  (--color-blue-100)
           #b4d5e9  (--color-blue-200)
           #9cc8e2  (--color-blue-300)
Main    → #83badb  (--color-blue-400) ← PRIMARY BRAND
           #6aadce  (--color-blue-500)
           #5a9cb8  (--color-blue-600)
           #4a8ba2  (--color-blue-700)
           #3a7a8c  (--color-blue-800)
Darkest → #2a6976  (--color-blue-900)
```

---

## Questions?

Refer to `COLOR_PALETTE.md` for detailed color specifications and `src/color-utilities.css` for all available utility classes.
