# Faulu Apartment Management - Color Palette Guide

## 🎨 Primary Blue Palette (Professional Gradient)

The color palette is designed for professional property management with a cohesive blue gradient system.

### Color Values

| Variable | Hex | RGB | Usage |
|----------|-----|-----|-------|
| `--color-blue-50` | #e6f1f7 | (230, 241, 247) | **Lightest** - Page backgrounds, subtle overlays |
| `--color-blue-100` | #cde3f0 | (205, 227, 240) | Light - Card backgrounds, dividers |
| `--color-blue-200` | #b4d5e9 | (180, 213, 233) | Light-Medium - Input field borders, disabled states |
| `--color-blue-300` | #9cc8e2 | (156, 200, 226) | Medium - Secondary buttons, hover states |
| `--color-blue-400` | #83badb | (131, 186, 219) | **Main Brand** - Primary buttons, active states, focus indicators |
| `--color-blue-500` | #6aadce | (106, 173, 206) | Medium-Dark - Hover states for primary elements |
| `--color-blue-600` | #5a9cb8 | (90, 156, 184) | Dark - Active/pressed states |
| `--color-blue-700` | #4a8ba2 | (74, 139, 162) | Darker - Disabled buttons, borders |
| `--color-blue-800` | #3a7a8c | (58, 122, 140) | Very Dark - Text emphasis, strong borders |
| `--color-blue-900` | #2a6976 | (42, 105, 118) | **Darkest** - Headers, strong text contrast |

## 🎯 Primary Brand Colors (Active)

| Variable | Value | Usage |
|----------|-------|-------|
| `--color-primary` | #83badb | Main brand color - buttons, links, highlights |
| `--color-primary-hover` | #6aadce | Hover state for primary elements |
| `--color-primary-light` | #e6f1f7 | Light background for primary-related content |
| `--color-primary-bg` | rgba(131, 186, 219, 0.1) | Semi-transparent background overlay |

## 📊 Color Application Guide

### Dashboard & Cards
- **Page Background:** `--color-blue-50` (#e6f1f7)
- **Card Background:** `--color-blue-50` or white
- **Card Border:** `--color-blue-100` (#cde3f0)
- **Card Dividers:** `--color-blue-100` (#cde3f0)

### Buttons & CTAs
- **Primary Button:** `--color-blue-400` (#83badb)
- **Primary Hover:** `--color-blue-500` (#6aadce)
- **Primary Active:** `--color-blue-600` (#5a9cb8)
- **Secondary Button:** Use neutral gray system
- **Disabled Button:** `--color-blue-200` (#b4d5e9)

### Form Elements
- **Input Border (Default):** `--color-border-default` (gray)
- **Input Border (Focus):** `--color-primary` (#83badb)
- **Input Focus Shadow:** `rgba(131, 186, 219, 0.3)`
- **Input Background:** White
- **Placeholder Text:** Gray secondary

### Status Indicators
- **Properties Card Border:** `--color-primary` (#83badb)
- **Tenants Card Border:** `--color-success` (green)
- **Maintenance Card Border:** `--color-warning` (orange)
- **Payments Card Border:** `--color-error` (red)

### Navigation & UI
- **Active Navigation Item:** `--color-blue-400` (#83badb)
- **Hover State:** `--color-blue-100` (#cde3f0) background
- **Active Link:** `--color-blue-500` (#6aadce)

## 💡 Usage Examples

### CSS Variables in Stylesheets
```css
.my-element {
  background-color: var(--color-blue-50);
  border: 1px solid var(--color-blue-100);
  color: var(--color-text-primary);
}

.my-button {
  background-color: var(--color-primary);
  color: white;
}

.my-button:hover {
  background-color: var(--color-primary-hover);
}
```

### Gradient Accents (Optional)
```css
.gradient-accent {
  background: linear-gradient(135deg, var(--color-blue-400), var(--color-blue-500));
}

.gradient-subtle {
  background: linear-gradient(135deg, var(--color-blue-50), var(--color-blue-100));
}
```

## 🎨 Design Principles

1. **Hierarchy:** Use darker blues for important elements, lighter for secondary
2. **Consistency:** Always use CSS variables, never hardcode colors
3. **Accessibility:** Maintain sufficient contrast ratios
4. **Status:** Use status colors (success/warning/error) for outcomes, not the blue palette
5. **Whitespace:** Balance blue usage with white backgrounds and neutrals

## 📱 Responsive Considerations

The blue palette works seamlessly across:
- Desktop (full width displays)
- Tablet (medium displays)
- Mobile (touch-friendly interfaces)

Maintain the same color values across all breakpoints for consistency.

## 🔄 Future Enhancements

- Consider dark mode variants with adjusted blue shades
- Add animation effects using the gradient palette
- Create chart color scheme using the palette levels
