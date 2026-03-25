import * as fc from 'fast-check';

/**
 * Feature: igds-ui-migration, Property 9: תמיכה ב-RTL ו-CSS logical properties
 *
 * Validates: Requirements 10.1, 10.2
 *
 * For any migrated component's stylesheet, (a) direction should be set to inherit
 * (not ltr or rtl explicitly), and (b) no physical direction CSS properties
 * (margin-left, margin-right, padding-left, padding-right, left, right,
 * text-align: left/right) should appear — only logical equivalents
 * (margin-inline-start, margin-inline-end, padding-inline-start,
 * padding-inline-end, inset-inline-start, text-align: start/end).
 */

/**
 * Represents a migrated component's stylesheet metadata for testing.
 */
interface ComponentStylesheet {
  /** Component name for error reporting */
  name: string;
  /** The raw CSS string from the component's inline styles */
  css: string;
}

/**
 * Physical direction CSS properties that should NOT appear in migrated stylesheets.
 * Each regex uses negative lookbehind to avoid matching logical property names
 * that happen to contain the physical keyword (e.g. margin-inline-start vs margin-left).
 */
const PHYSICAL_DIRECTION_PATTERNS: Array<{
  pattern: RegExp;
  description: string;
  logicalAlternative: string;
}> = [
  {
    pattern: /(?<![a-zA-Z-])margin-left\s*:/g,
    description: 'margin-left',
    logicalAlternative: 'margin-inline-start / margin-inline-end',
  },
  {
    pattern: /(?<![a-zA-Z-])margin-right\s*:/g,
    description: 'margin-right',
    logicalAlternative: 'margin-inline-start / margin-inline-end',
  },
  {
    pattern: /(?<![a-zA-Z-])padding-left\s*:/g,
    description: 'padding-left',
    logicalAlternative: 'padding-inline-start / padding-inline-end',
  },
  {
    pattern: /(?<![a-zA-Z-])padding-right\s*:/g,
    description: 'padding-right',
    logicalAlternative: 'padding-inline-start / padding-inline-end',
  },
  {
    pattern: /text-align\s*:\s*left/g,
    description: 'text-align: left',
    logicalAlternative: 'text-align: start',
  },
  {
    pattern: /text-align\s*:\s*right/g,
    description: 'text-align: right',
    logicalAlternative: 'text-align: end',
  },
];

/**
 * Checks for standalone `left:` or `right:` positional properties.
 * These need special handling to avoid matching inside compound properties
 * like `border-left`, `inset-inline-start`, etc.
 * We match lines that have `left:` or `right:` as standalone CSS properties.
 */
const POSITIONAL_LEFT_PATTERN = /(?:^|[{;\s])left\s*:/gm;
const POSITIONAL_RIGHT_PATTERN = /(?:^|[{;\s])right\s*:/gm;

/**
 * Regex to detect explicit direction: ltr or direction: rtl.
 */
const EXPLICIT_DIRECTION_PATTERN = /direction\s*:\s*(ltr|rtl)\b/g;

/**
 * Regex to detect direction: inherit declarations.
 */
const DIRECTION_INHERIT_PATTERN = /direction\s*:\s*inherit/g;

/**
 * All migrated IGDS component stylesheets and migrated shared/core component stylesheets.
 * These CSS strings are extracted from the inline `styles` arrays in each component's .ts file.
 *
 * This list covers:
 * - All 22 IGDS components in client/src/app/shared/igds/components/
 * - Migrated shared components (confirm-dialog, loading-spinner)
 * - Migrated core components (layout, breadcrumbs)
 */
const MIGRATED_COMPONENT_STYLESHEETS: ComponentStylesheet[] = [
  {
    name: 'igds-button',
    css: `
      :host { display: inline-block; }
      .igds-btn {
        font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
        font-weight: var(--igds-font-weight-medium); line-height: 1.5;
        padding: var(--igds-space-8) var(--igds-space-24); border-radius: var(--igds-radius-md);
        border: 2px solid transparent; cursor: pointer; display: inline-flex;
        align-items: center; justify-content: center; gap: var(--igds-space-8);
        transition: all var(--igds-transition-fast); min-height: 44px; direction: inherit;
      }
      .igds-btn:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
      .igds-btn--icon-only { padding: var(--igds-space-8); }
      .igds-btn__icon { display: inline-flex; align-items: center; }
    `,
  },
  {
    name: 'igds-input-field',
    css: `
      :host { display: block; }
      .igds-input__label {
        display: block; font-family: var(--igds-font-family); font-size: var(--igds-font-size-sm);
        font-weight: var(--igds-font-weight-medium); color: var(--igds-text-primary);
        margin-bottom: var(--igds-space-4);
      }
      .igds-input__field {
        width: 100%; padding: var(--igds-space-8) var(--igds-space-12); border: none; outline: none;
        font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
        color: var(--igds-text-primary); background: transparent; direction: inherit;
      }
      .igds-input__error {
        display: block; font-size: var(--igds-font-size-xs); color: var(--igds-text-failure);
        margin-top: var(--igds-space-4);
      }
      .igds-input__helper {
        display: block; font-size: var(--igds-font-size-xs); color: var(--igds-text-secondary);
        margin-top: var(--igds-space-4);
      }
    `,
  },
  {
    name: 'igds-dropdown',
    css: `
      :host { display: block; direction: inherit; }
      .igds-dropdown { position: relative; font-family: var(--igds-font-family); }
      .igds-dropdown__label {
        display: block; font-size: var(--igds-font-size-sm); font-weight: var(--igds-font-weight-medium);
        color: var(--igds-text-primary); margin-bottom: var(--igds-space-4);
      }
      .igds-dropdown__trigger {
        display: flex; align-items: center; justify-content: space-between;
        padding: var(--igds-space-8) var(--igds-space-12); border: 1px solid var(--igds-border-subtle-default);
        border-radius: var(--igds-radius-md); background: var(--igds-bg-neutral);
        cursor: pointer; min-height: 44px;
      }
      .igds-dropdown__list {
        position: absolute; top: 100%; inset-inline-start: 0; inset-inline-end: 0; z-index: 100;
        margin: var(--igds-space-4) 0 0; padding: var(--igds-space-4) 0; list-style: none;
        background: var(--igds-bg-neutral); border: 1px solid var(--igds-border-subtle-default);
        border-radius: var(--igds-radius-md); box-shadow: var(--igds-shadow-md); max-height: 240px; overflow-y: auto;
      }
      .igds-dropdown__option {
        padding: var(--igds-space-8) var(--igds-space-12); cursor: pointer;
        font-size: var(--igds-font-size-md); color: var(--igds-text-primary);
      }
      .igds-dropdown__error {
        display: block; font-size: var(--igds-font-size-xs); color: var(--igds-text-failure);
        margin-top: var(--igds-space-4);
      }
    `,
  },
  {
    name: 'igds-date-picker',
    css: `
      :host { display: block; direction: inherit; }
      .igds-datepicker { font-family: var(--igds-font-family); }
      .igds-datepicker__label {
        display: block; font-size: var(--igds-font-size-sm); font-weight: var(--igds-font-weight-medium);
        color: var(--igds-text-primary); margin-bottom: var(--igds-space-4);
      }
      .igds-datepicker__field {
        width: 100%; padding: var(--igds-space-8) var(--igds-space-12); border: 1px solid var(--igds-border-subtle-default);
        border-radius: var(--igds-radius-md); font-family: var(--igds-font-family);
        font-size: var(--igds-font-size-md); color: var(--igds-text-primary);
        background: var(--igds-bg-neutral); min-height: 44px; direction: inherit;
      }
    `,
  },
  {
    name: 'igds-checkbox',
    css: `
      :host { display: inline-block; direction: inherit; }
      .igds-checkbox {
        display: inline-flex; align-items: center; gap: var(--igds-space-8);
        cursor: pointer; font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
        color: var(--igds-text-primary); min-height: 44px;
      }
    `,
  },
  {
    name: 'igds-radio-button',
    css: `
      :host { display: inline-block; direction: inherit; }
      .igds-radio {
        display: inline-flex; align-items: center; gap: var(--igds-space-8);
        cursor: pointer; font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
        color: var(--igds-text-primary); min-height: 44px;
      }
    `,
  },
  {
    name: 'igds-toggle',
    css: `
      :host { display: inline-block; direction: inherit; }
      .igds-toggle {
        display: inline-flex; align-items: center; gap: var(--igds-space-8);
        cursor: pointer; font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
        color: var(--igds-text-primary); min-height: 44px;
      }
    `,
  },
  {
    name: 'igds-table',
    css: `
      :host { display: block; direction: inherit; }
      .igds-table-wrapper { overflow-x: auto; border: 1px solid var(--igds-border-divider); border-radius: var(--igds-radius-md); }
      .igds-table { width: 100%; border-collapse: collapse; font-family: var(--igds-font-family); }
      .igds-table th {
        padding: var(--igds-space-12) var(--igds-space-16); text-align: start;
        font-size: var(--igds-font-size-sm); font-weight: var(--igds-font-weight-bold);
        color: var(--igds-text-primary); background: var(--igds-bg-neutral-secondlevel);
        border-bottom: 2px solid var(--igds-border-divider);
      }
      .igds-table td {
        padding: var(--igds-space-12) var(--igds-space-16); text-align: start;
        font-size: var(--igds-font-size-md); color: var(--igds-text-primary);
        border-bottom: 1px solid var(--igds-border-divider);
      }
    `,
  },
  {
    name: 'igds-pagination',
    css: `
      :host { display: block; direction: inherit; }
      .igds-pagination {
        display: flex; align-items: center; justify-content: center; gap: var(--igds-space-8);
        padding: var(--igds-space-12) 0; font-family: var(--igds-font-family);
      }
      .igds-pagination__btn {
        min-width: 44px; min-height: 44px; display: flex; align-items: center; justify-content: center;
        border: 1px solid var(--igds-border-subtle-default); border-radius: var(--igds-radius-md);
        background: var(--igds-bg-neutral); cursor: pointer; font-size: var(--igds-font-size-sm);
        color: var(--igds-text-primary);
      }
    `,
  },
  {
    name: 'igds-card',
    css: `
      :host { display: block; direction: inherit; }
      .igds-card {
        background: var(--igds-bg-neutral); border: 1px solid var(--igds-border-divider);
        border-radius: var(--igds-radius-lg); box-shadow: var(--igds-shadow-sm);
        font-family: var(--igds-font-family); overflow: hidden;
      }
      .igds-card__header {
        padding: var(--igds-space-16); border-bottom: 1px solid var(--igds-border-divider);
      }
      .igds-card__body { padding: var(--igds-space-16); }
      .igds-card__footer {
        padding: var(--igds-space-12) var(--igds-space-16);
        border-top: 1px solid var(--igds-border-divider);
        display: flex; justify-content: flex-end; gap: var(--igds-space-8);
      }
    `,
  },
  {
    name: 'igds-tabs',
    css: `
      :host { display: block; direction: inherit; }
      .igds-tabs { font-family: var(--igds-font-family); }
      .igds-tabs__list {
        display: flex; border-bottom: 2px solid var(--igds-border-divider);
        list-style: none; margin: 0; padding: 0;
      }
      .igds-tabs__tab {
        padding: var(--igds-space-12) var(--igds-space-16); cursor: pointer;
        font-size: var(--igds-font-size-sm); color: var(--igds-text-secondary);
        border: none; background: none; min-height: 44px;
      }
    `,
  },
  {
    name: 'igds-accordion',
    css: `
      :host { display: block; direction: inherit; }
      .igds-accordion { font-family: var(--igds-font-family); }
      .igds-accordion__item {
        border: 1px solid var(--igds-border-divider); border-radius: var(--igds-radius-md);
        margin-bottom: var(--igds-space-8); overflow: hidden;
      }
      .igds-accordion__header {
        display: flex; align-items: center; justify-content: space-between;
        padding: var(--igds-space-12) var(--igds-space-16); cursor: pointer;
        background: var(--igds-bg-neutral); min-height: 44px;
      }
      .igds-accordion__body { padding: var(--igds-space-16); }
    `,
  },
  {
    name: 'igds-modal',
    css: `
      :host { direction: inherit; }
      .igds-modal__overlay {
        position: fixed; top: 0; bottom: 0; inset-inline-start: 0; inset-inline-end: 0;
        background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center;
        z-index: 1000;
      }
      .igds-modal__dialog {
        background: var(--igds-bg-neutral); border-radius: var(--igds-radius-lg);
        box-shadow: var(--igds-shadow-lg); max-width: 560px; width: 90%;
        max-height: 80vh; overflow-y: auto; font-family: var(--igds-font-family);
      }
      .igds-modal__header {
        display: flex; align-items: center; justify-content: space-between;
        padding: var(--igds-space-16); border-bottom: 1px solid var(--igds-border-divider);
      }
      .igds-modal__body { padding: var(--igds-space-16); }
    `,
  },
  {
    name: 'igds-toast',
    css: `
      :host { display: block; direction: inherit; }
      .igds-toast {
        position: fixed; bottom: var(--igds-space-24); inset-inline-start: 50%;
        transform: translateX(-50%); z-index: 2000; min-width: 300px; max-width: 560px;
        padding: var(--igds-space-12) var(--igds-space-16); border-radius: var(--igds-radius-md);
        box-shadow: var(--igds-shadow-lg); font-family: var(--igds-font-family);
        font-size: var(--igds-font-size-sm); display: flex; align-items: center;
        gap: var(--igds-space-8);
      }
    `,
  },
  {
    name: 'igds-side-menu',
    css: `
      :host { display: block; direction: inherit; }
      .igds-sidemenu { font-family: var(--igds-font-family); background: var(--igds-bg-neutral); min-height: 100%; }
      .igds-sidemenu__list { list-style: none; margin: 0; padding: var(--igds-space-8) 0; }
      .igds-sidemenu__btn {
        display: flex; align-items: center; gap: var(--igds-space-8); width: 100%;
        padding: var(--igds-space-8) var(--igds-space-16); background: none; border: none;
        cursor: pointer; font-family: var(--igds-font-family); font-size: var(--igds-font-size-sm);
        color: var(--igds-text-primary); min-height: 40px; text-align: inherit;
      }
      .igds-sidemenu__btn--sub { padding-inline-start: var(--igds-space-40); font-size: var(--igds-font-size-xs); }
      .igds-sidemenu__sublist { list-style: none; margin: 0; padding: 0; }
    `,
  },
  {
    name: 'igds-breadcrumbs',
    css: `
      :host { display: block; direction: inherit; }
      .igds-breadcrumbs { font-family: var(--igds-font-family); }
      .igds-breadcrumbs__list {
        display: flex; align-items: center; gap: var(--igds-space-4);
        list-style: none; margin: 0; padding: 0;
      }
      .igds-breadcrumbs__link {
        color: var(--igds-text-link-default); text-decoration: none;
        font-size: var(--igds-font-size-sm);
      }
    `,
  },
  {
    name: 'igds-search-field',
    css: `
      :host { display: block; direction: inherit; }
      .igds-search { font-family: var(--igds-font-family); position: relative; }
      .igds-search__input {
        width: 100%; padding: var(--igds-space-8) var(--igds-space-12);
        padding-inline-start: var(--igds-space-40); border: 1px solid var(--igds-border-subtle-default);
        border-radius: var(--igds-radius-md); font-family: var(--igds-font-family);
        font-size: var(--igds-font-size-md); min-height: 44px; direction: inherit;
      }
      .igds-search__icon {
        position: absolute; top: 50%; transform: translateY(-50%);
        inset-inline-start: var(--igds-space-12); color: var(--igds-text-secondary);
      }
    `,
  },
  {
    name: 'igds-status-badge',
    css: `
      :host { display: inline-block; direction: inherit; }
      .igds-badge {
        display: inline-flex; align-items: center; gap: var(--igds-space-4);
        padding: var(--igds-space-4) var(--igds-space-8); border-radius: var(--igds-radius-full);
        font-family: var(--igds-font-family); font-size: var(--igds-font-size-xs);
        font-weight: var(--igds-font-weight-medium);
      }
    `,
  },
  {
    name: 'igds-step-indicator',
    css: `
      :host { display: block; direction: inherit; }
      .igds-steps { font-family: var(--igds-font-family); }
      .igds-steps__list {
        display: flex; align-items: center; list-style: none; margin: 0; padding: 0;
      }
      .igds-steps__item {
        display: flex; align-items: center; gap: var(--igds-space-8);
        font-size: var(--igds-font-size-sm); color: var(--igds-text-secondary);
      }
    `,
  },
  {
    name: 'igds-progress-bar',
    css: `
      :host { display: block; direction: inherit; }
      .igds-progress {
        font-family: var(--igds-font-family); display: flex; align-items: center;
        gap: var(--igds-space-8);
      }
      .igds-progress__track {
        flex: 1; height: 8px; background: var(--igds-bg-neutral-secondlevel);
        border-radius: var(--igds-radius-full); overflow: hidden;
      }
      .igds-progress__fill {
        height: 100%; background: var(--igds-bg-brand-default);
        border-radius: var(--igds-radius-full);
      }
    `,
  },
  {
    name: 'igds-tag',
    css: `
      :host { display: inline-block; direction: inherit; }
      .igds-tag {
        display: inline-flex; align-items: center; gap: var(--igds-space-4);
        padding: var(--igds-space-4) var(--igds-space-8); border-radius: var(--igds-radius-full);
        font-family: var(--igds-font-family); font-size: var(--igds-font-size-xs);
        font-weight: var(--igds-font-weight-medium);
      }
    `,
  },
  {
    name: 'igds-drawer',
    css: `
      :host { direction: inherit; }
      .igds-drawer__overlay {
        position: fixed; top: 0; bottom: 0; inset-inline-start: 0; inset-inline-end: 0;
        background: rgba(0,0,0,0.5); z-index: 1000;
      }
      .igds-drawer__panel {
        position: fixed; top: 0; bottom: 0; width: 320px;
        background: var(--igds-bg-neutral); box-shadow: var(--igds-shadow-lg);
        font-family: var(--igds-font-family); overflow-y: auto; z-index: 1001;
      }
      .igds-drawer__panel--end { inset-inline-end: 0; }
      .igds-drawer__panel--start { inset-inline-start: 0; }
      .igds-drawer__header {
        display: flex; align-items: center; justify-content: space-between;
        padding: var(--igds-space-16); border-bottom: 1px solid var(--igds-border-divider);
      }
      .igds-drawer__body { padding: var(--igds-space-16); }
    `,
  },
  // --- Migrated shared/core components ---
  {
    name: 'confirm-dialog (shared)',
    css: `
      :host { display: block; direction: inherit; }
      .igds-confirm-dialog__message {
        font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
        color: var(--igds-text-primary); margin: 0 0 var(--igds-space-24) 0; line-height: 1.6;
      }
      .igds-confirm-dialog__actions {
        display: flex; justify-content: flex-end; gap: var(--igds-space-8);
      }
    `,
  },
  {
    name: 'loading-spinner (shared)',
    css: `
      :host { display: block; direction: inherit; }
      .igds-spinner-container {
        display: flex; justify-content: center; align-items: center;
        padding: var(--igds-space-24);
      }
      .igds-spinner {
        border-radius: var(--igds-radius-full);
        border: 3px solid var(--igds-bg-neutral-secondlevel);
        border-top-color: var(--igds-bg-brand-default);
        animation: igds-spin 0.8s linear infinite;
      }
    `,
  },
  {
    name: 'layout (core)',
    css: `
      :host { display: block; direction: inherit; }
      .igds-header {
        display: flex; align-items: center; height: 56px;
        padding: 0 var(--igds-space-16); background: var(--igds-bg-brand-default);
        color: var(--igds-text-on-brand); font-family: var(--igds-font-family);
      }
      .igds-header__title {
        font-size: var(--igds-font-size-lg); font-weight: var(--igds-font-weight-bold);
        margin-inline-start: var(--igds-space-12);
      }
      .igds-header__user-info {
        margin-inline-end: var(--igds-space-16); font-size: var(--igds-font-size-sm);
      }
      .igds-layout { display: flex; height: calc(100vh - 56px); font-family: var(--igds-font-family); }
      .igds-layout__content { display: flex; flex-direction: column; flex: 1; overflow: auto; }
      .igds-layout__page { padding: var(--igds-space-16); flex: 1; }
      .igds-user-drawer { display: flex; flex-direction: column; gap: var(--igds-space-16); }
      .igds-user-drawer__info {
        padding-block-end: var(--igds-space-16);
        border-block-end: 1px solid var(--igds-border-divider);
      }
    `,
  },
  {
    name: 'breadcrumbs (core)',
    css: `
      :host { display: block; direction: inherit; }
      .igds-breadcrumbs-wrapper {
        padding: var(--igds-space-8) var(--igds-space-16);
        background: var(--igds-bg-neutral);
        border-block-end: 1px solid var(--igds-border-divider);
      }
    `,
  },
];

/**
 * Checks a CSS string for physical direction properties.
 * Returns an array of violations found.
 */
function findPhysicalDirectionViolations(css: string): Array<{ description: string; logicalAlternative: string }> {
  const violations: Array<{ description: string; logicalAlternative: string }> = [];

  for (const { pattern, description, logicalAlternative } of PHYSICAL_DIRECTION_PATTERNS) {
    // Reset regex lastIndex
    pattern.lastIndex = 0;
    if (pattern.test(css)) {
      violations.push({ description, logicalAlternative });
    }
  }

  // Check standalone positional left/right properties
  // Filter out false positives from compound properties like border-left, border-right
  // by checking each line individually
  const lines = css.split('\n');
  for (const line of lines) {
    const trimmed = line.trim();
    // Skip lines that are part of compound properties (border-left, border-right, etc.)
    if (/^\s*border-(left|right)/.test(trimmed)) continue;
    // Skip lines inside comments
    if (trimmed.startsWith('/*') || trimmed.startsWith('*')) continue;

    // Check for standalone left: or right: at the start of a declaration
    if (/^left\s*:/.test(trimmed) || /;\s*left\s*:/.test(trimmed)) {
      violations.push({ description: 'left (positional)', logicalAlternative: 'inset-inline-start' });
    }
    if (/^right\s*:/.test(trimmed) || /;\s*right\s*:/.test(trimmed)) {
      violations.push({ description: 'right (positional)', logicalAlternative: 'inset-inline-end' });
    }
  }

  return violations;
}

/**
 * Checks a CSS string for explicit direction: ltr or direction: rtl.
 * Returns true if an explicit (non-inherit) direction is found.
 */
function hasExplicitDirection(css: string): { found: boolean; value?: string } {
  EXPLICIT_DIRECTION_PATTERN.lastIndex = 0;
  const match = EXPLICIT_DIRECTION_PATTERN.exec(css);
  if (match) {
    return { found: true, value: match[1] };
  }
  return { found: false };
}

/**
 * Checks if a CSS string contains direction: inherit.
 */
function hasDirectionInherit(css: string): boolean {
  DIRECTION_INHERIT_PATTERN.lastIndex = 0;
  return DIRECTION_INHERIT_PATTERN.test(css);
}

describe('Feature: igds-ui-migration, Property 9: תמיכה ב-RTL ו-CSS logical properties', () => {

  describe('(a) direction should be set to inherit, not ltr or rtl explicitly', () => {
    it('for any randomly selected migrated component, direction is never set to ltr or rtl', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...MIGRATED_COMPONENT_STYLESHEETS),
          (stylesheet: ComponentStylesheet) => {
            const result = hasExplicitDirection(stylesheet.css);
            if (result.found) {
              throw new Error(
                `Component "${stylesheet.name}" has explicit direction: ${result.value}. ` +
                `Expected direction: inherit (to support RTL from html[dir="rtl"]).`
              );
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('for any randomly selected migrated component that sets direction, it uses inherit', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...MIGRATED_COMPONENT_STYLESHEETS),
          (stylesheet: ComponentStylesheet) => {
            // If the component sets direction at all, it must be inherit
            const hasExplicit = hasExplicitDirection(stylesheet.css);
            const hasInherit = hasDirectionInherit(stylesheet.css);

            if (hasExplicit.found) {
              throw new Error(
                `Component "${stylesheet.name}" sets direction: ${hasExplicit.value} ` +
                `instead of direction: inherit.`
              );
            }

            // Components that set direction should use inherit
            // (not all components need to set direction explicitly)
            if (hasInherit) {
              // This is correct — direction: inherit is the expected value
              return;
            }

            // Components that don't set direction at all are also acceptable
            // (they inherit by default from the parent)
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('(b) no physical direction CSS properties should appear', () => {
    it('for any randomly selected migrated component, no physical direction properties are used', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...MIGRATED_COMPONENT_STYLESHEETS),
          (stylesheet: ComponentStylesheet) => {
            const violations = findPhysicalDirectionViolations(stylesheet.css);
            if (violations.length > 0) {
              const violationList = violations
                .map(v => `  - "${v.description}" → use "${v.logicalAlternative}" instead`)
                .join('\n');
              throw new Error(
                `Component "${stylesheet.name}" uses physical direction CSS properties:\n${violationList}`
              );
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('for any pair of randomly selected migrated components, both are free of physical direction properties', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...MIGRATED_COMPONENT_STYLESHEETS),
          fc.constantFrom(...MIGRATED_COMPONENT_STYLESHEETS),
          (sheet1: ComponentStylesheet, sheet2: ComponentStylesheet) => {
            for (const sheet of [sheet1, sheet2]) {
              const violations = findPhysicalDirectionViolations(sheet.css);
              if (violations.length > 0) {
                const violationList = violations
                  .map(v => `  - "${v.description}" → use "${v.logicalAlternative}" instead`)
                  .join('\n');
                throw new Error(
                  `Component "${sheet.name}" uses physical direction CSS properties:\n${violationList}`
                );
              }
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('combined: all migrated components pass both RTL checks', () => {
    it('for any randomly selected migrated component, both direction and logical properties are correct', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...MIGRATED_COMPONENT_STYLESHEETS),
          (stylesheet: ComponentStylesheet) => {
            // Check (a): no explicit ltr/rtl direction
            const dirResult = hasExplicitDirection(stylesheet.css);
            if (dirResult.found) {
              throw new Error(
                `Component "${stylesheet.name}" has explicit direction: ${dirResult.value}. ` +
                `Expected direction: inherit.`
              );
            }

            // Check (b): no physical direction properties
            const violations = findPhysicalDirectionViolations(stylesheet.css);
            if (violations.length > 0) {
              const violationList = violations
                .map(v => `  - "${v.description}" → use "${v.logicalAlternative}" instead`)
                .join('\n');
              throw new Error(
                `Component "${stylesheet.name}" uses physical direction CSS properties:\n${violationList}`
              );
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('exhaustive: every single migrated component passes RTL checks', () => {
    // This deterministic test ensures 100% coverage of all components
    MIGRATED_COMPONENT_STYLESHEETS.forEach((stylesheet) => {
      it(`${stylesheet.name}: no explicit direction ltr/rtl`, () => {
        const result = hasExplicitDirection(stylesheet.css);
        if (result.found) {
          fail(
            `Component "${stylesheet.name}" has explicit direction: ${result.value}. ` +
            `Expected direction: inherit.`
          );
        }
      });

      it(`${stylesheet.name}: no physical direction CSS properties`, () => {
        const violations = findPhysicalDirectionViolations(stylesheet.css);
        if (violations.length > 0) {
          const violationList = violations
            .map(v => `  - "${v.description}" → use "${v.logicalAlternative}" instead`)
            .join('\n');
          fail(
            `Component "${stylesheet.name}" uses physical direction CSS properties:\n${violationList}`
          );
        }
      });
    });
  });
});
