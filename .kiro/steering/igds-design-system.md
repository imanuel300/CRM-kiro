---
inclusion: fileMatch
fileMatchPattern: "**/igds/**"
---

# IGDS - Israeli Government Design System

When working with IGDS components under `client/src/app/shared/igds/`:

## Design Tokens
- Always use CSS custom properties from `igds-tokens.scss` (e.g. `var(--igds-bg-brand-default)`)
- Never hardcode colors, spacing, or typography values
- Token naming: `--igds-{category}-{variant}` (bg, text, border, space, radius, shadow, font)

## Component Conventions
- Selector prefix: `igds-` (e.g. `igds-button`, `igds-input-field`)
- All components must support RTL via `direction: inherit` and CSS logical properties
- Minimum touch target: 44px
- Use `focus-visible` for keyboard focus styles with `--igds-border-focused`
- Form components implement `ControlValueAccessor`

## Accessibility
- All interactive elements need ARIA attributes
- Keyboard navigation support is required
- Color contrast must meet WCAG AA (4.5:1 text, 3:1 large text)

## Typography
- Primary font: Heebo (Hebrew), Assistant (fallback), Arial
- Font loaded via Google Fonts in `index.html`

## File Structure
- Components: `components/{name}/igds-{name}.component.ts`
- Directives: `directives/igds-{name}.directive.ts`
- Module: `igds.module.ts` (all components declared and exported here)
- Tokens: `igds-tokens.scss`
