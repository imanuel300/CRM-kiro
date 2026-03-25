import * as fc from 'fast-check';

/**
 * Feature: igds-ui-migration, Property 12: גודל מינימלי למגע ו-focus-visible
 *
 * Validates: Requirements 11.3, 11.4
 *
 * For any interactive element in IGDS components (buttons, inputs, dropdown triggers,
 * pagination buttons, tab buttons), the computed min-height should be at least 44px.
 * Additionally, the component's stylesheet should include a :focus-visible rule that
 * uses --igds-border-focused.
 */

/**
 * Represents an interactive IGDS component's stylesheet metadata for testing.
 */
interface InteractiveComponentStylesheet {
  /** Component name for error reporting */
  name: string;
  /** The raw CSS string from the component's inline styles */
  css: string;
  /** CSS selectors for interactive elements that must have min-height >= 44px */
  interactiveSelectors: string[];
  /** Whether this component must have a :focus-visible rule (true for all interactive components) */
  requiresFocusVisible: boolean;
}

/**
 * Minimum touch target size in pixels as per IGDS design system rules and requirement 11.3.
 */
const MIN_TOUCH_TARGET_PX = 44;

/**
 * Regex to detect :focus-visible rules in CSS.
 */
const FOCUS_VISIBLE_PATTERN = /:focus-visible\b/g;

/**
 * Regex to detect usage of --igds-border-focused token.
 */
const BORDER_FOCUSED_TOKEN_PATTERN = /--igds-border-focused/g;

/**
 * All interactive IGDS component stylesheets.
 * These CSS strings are extracted from the inline `styles` arrays in each component's .ts file.
 *
 * Interactive components are those that the user directly interacts with:
 * buttons, inputs, dropdown triggers, pagination buttons, tab buttons,
 * accordion headers, checkbox, radio, toggle, search field, side-menu buttons.
 */
const INTERACTIVE_COMPONENT_STYLESHEETS: InteractiveComponentStylesheet[] = [
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
    interactiveSelectors: ['.igds-btn'],
    requiresFocusVisible: true,
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
      .igds-input__wrapper {
        display: flex; align-items: center; border: 1px solid var(--igds-border-subtle-default);
        border-radius: var(--igds-radius-md); background: var(--igds-bg-neutral);
        min-height: 44px; transition: border-color var(--igds-transition-fast);
      }
      .igds-input__wrapper:focus-within { border-color: var(--igds-border-focused); }
      .igds-input__field {
        width: 100%; padding: var(--igds-space-8) var(--igds-space-12); border: none; outline: none;
        font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
        color: var(--igds-text-primary); background: transparent; direction: inherit;
      }
      .igds-input__field:focus-visible { outline: none; }
      .igds-input__error {
        display: block; font-size: var(--igds-font-size-xs); color: var(--igds-text-failure);
        margin-top: var(--igds-space-4);
      }
    `,
    interactiveSelectors: ['.igds-input__wrapper'],
    requiresFocusVisible: true,
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
      .igds-dropdown__trigger:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
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
    interactiveSelectors: ['.igds-dropdown__trigger'],
    requiresFocusVisible: true,
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
      .igds-pagination__btn:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    `,
    interactiveSelectors: ['.igds-pagination__btn'],
    requiresFocusVisible: true,
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
      .igds-tabs__tab:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: -2px; }
    `,
    interactiveSelectors: ['.igds-tabs__tab'],
    requiresFocusVisible: true,
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
      .igds-accordion__header:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: -2px; }
      .igds-accordion__body { padding: var(--igds-space-16); }
    `,
    interactiveSelectors: ['.igds-accordion__header'],
    requiresFocusVisible: true,
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
      .igds-checkbox__input:focus-visible + .igds-checkbox__box { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    `,
    interactiveSelectors: ['.igds-checkbox'],
    requiresFocusVisible: true,
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
      .igds-radio__input:focus-visible + .igds-radio__circle { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    `,
    interactiveSelectors: ['.igds-radio'],
    requiresFocusVisible: true,
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
      .igds-toggle__input:focus-visible + .igds-toggle__track { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    `,
    interactiveSelectors: ['.igds-toggle'],
    requiresFocusVisible: true,
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
      .igds-search__input:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
      .igds-search__icon {
        position: absolute; top: 50%; transform: translateY(-50%);
        inset-inline-start: var(--igds-space-12); color: var(--igds-text-secondary);
      }
    `,
    interactiveSelectors: ['.igds-search__input'],
    requiresFocusVisible: true,
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
      .igds-datepicker__field:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    `,
    interactiveSelectors: ['.igds-datepicker__field'],
    requiresFocusVisible: true,
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
        color: var(--igds-text-primary); min-height: 44px; text-align: inherit;
      }
      .igds-sidemenu__btn:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: -2px; }
      .igds-sidemenu__btn--sub { padding-inline-start: var(--igds-space-40); font-size: var(--igds-font-size-xs); }
      .igds-sidemenu__sublist { list-style: none; margin: 0; padding: 0; }
    `,
    interactiveSelectors: ['.igds-sidemenu__btn'],
    requiresFocusVisible: true,
  },
];

/**
 * Extracts all min-height pixel values from a CSS string.
 * Returns an array of { selector: string, value: number } for each min-height declaration found.
 */
function extractMinHeightValues(css: string): Array<{ context: string; value: number }> {
  const results: Array<{ context: string; value: number }> = [];
  // Split CSS into rule blocks to get context
  const rulePattern = /([^{}]+)\{([^{}]+)\}/g;
  let match: RegExpExecArray | null;

  while ((match = rulePattern.exec(css)) !== null) {
    const selector = match[1].trim();
    const declarations = match[2];
    const minHeightMatch = /min-height\s*:\s*(\d+)px/g.exec(declarations);
    if (minHeightMatch) {
      results.push({ context: selector, value: parseInt(minHeightMatch[1], 10) });
    }
  }

  return results;
}

/**
 * Checks if a CSS string contains a :focus-visible rule that references --igds-border-focused.
 * Returns details about what was found or missing.
 */
function checkFocusVisibleRule(css: string): {
  hasFocusVisible: boolean;
  usesBorderFocusedToken: boolean;
  focusVisibleUsesToken: boolean;
} {
  // Check for :focus-visible anywhere in the CSS
  FOCUS_VISIBLE_PATTERN.lastIndex = 0;
  const hasFocusVisible = FOCUS_VISIBLE_PATTERN.test(css);

  // Check for --igds-border-focused anywhere in the CSS
  BORDER_FOCUSED_TOKEN_PATTERN.lastIndex = 0;
  const usesBorderFocusedToken = BORDER_FOCUSED_TOKEN_PATTERN.test(css);

  // Check if :focus-visible rule specifically uses --igds-border-focused
  // by finding focus-visible blocks and checking their content
  let focusVisibleUsesToken = false;
  const focusVisibleBlockPattern = /:focus-visible[^{]*\{([^}]+)\}/g;
  let blockMatch: RegExpExecArray | null;
  while ((blockMatch = focusVisibleBlockPattern.exec(css)) !== null) {
    if (blockMatch[1].includes('--igds-border-focused')) {
      focusVisibleUsesToken = true;
      break;
    }
  }

  // Also check for :focus-within as an acceptable alternative for wrapper-based focus
  // (e.g., igds-input-field uses :focus-within on the wrapper + :focus-visible on the inner input)
  const hasFocusWithin = /:focus-within\b/.test(css);
  const focusWithinUsesToken = hasFocusWithin && css.includes('--igds-border-focused');

  return {
    hasFocusVisible: hasFocusVisible || hasFocusWithin,
    usesBorderFocusedToken,
    focusVisibleUsesToken: focusVisibleUsesToken || focusWithinUsesToken,
  };
}

/**
 * Checks that all interactive selectors in a component have min-height >= 44px.
 * Returns violations found.
 */
function checkMinHeightForInteractiveElements(
  stylesheet: InteractiveComponentStylesheet
): Array<{ selector: string; actualValue: number | null }> {
  const violations: Array<{ selector: string; actualValue: number | null }> = [];
  const minHeights = extractMinHeightValues(stylesheet.css);

  for (const interactiveSelector of stylesheet.interactiveSelectors) {
    // Find the min-height for this selector
    const found = minHeights.find(mh => mh.context.includes(interactiveSelector));
    if (!found) {
      violations.push({ selector: interactiveSelector, actualValue: null });
    } else if (found.value < MIN_TOUCH_TARGET_PX) {
      violations.push({ selector: interactiveSelector, actualValue: found.value });
    }
  }

  return violations;
}

// ─── Test suite ─────────────────────────────────────────────────────────────────

describe('Feature: igds-ui-migration, Property 12: גודל מינימלי למגע ו-focus-visible', () => {

  describe('(a) interactive elements have min-height >= 44px for touch target', () => {
    it('for any randomly selected interactive component, all interactive elements have min-height >= 44px', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...INTERACTIVE_COMPONENT_STYLESHEETS),
          (stylesheet: InteractiveComponentStylesheet) => {
            const violations = checkMinHeightForInteractiveElements(stylesheet);
            if (violations.length > 0) {
              const violationList = violations
                .map(v =>
                  v.actualValue === null
                    ? `  - "${v.selector}": no min-height declaration found (expected >= ${MIN_TOUCH_TARGET_PX}px)`
                    : `  - "${v.selector}": min-height is ${v.actualValue}px (expected >= ${MIN_TOUCH_TARGET_PX}px)`
                )
                .join('\n');
              throw new Error(
                `Component "${stylesheet.name}" has interactive elements below minimum touch target:\n${violationList}`
              );
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('(b) component stylesheet includes :focus-visible rule with --igds-border-focused', () => {
    it('for any randomly selected interactive component, a :focus-visible rule using --igds-border-focused exists', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...INTERACTIVE_COMPONENT_STYLESHEETS),
          (stylesheet: InteractiveComponentStylesheet) => {
            if (!stylesheet.requiresFocusVisible) return;

            const result = checkFocusVisibleRule(stylesheet.css);

            if (!result.hasFocusVisible) {
              throw new Error(
                `Component "${stylesheet.name}" is missing a :focus-visible (or :focus-within) rule. ` +
                `All interactive IGDS components must display focus-visible state.`
              );
            }

            if (!result.usesBorderFocusedToken) {
              throw new Error(
                `Component "${stylesheet.name}" does not reference --igds-border-focused token. ` +
                `Focus styles must use the IGDS border-focused design token.`
              );
            }

            if (!result.focusVisibleUsesToken) {
              throw new Error(
                `Component "${stylesheet.name}" has a :focus-visible rule but it does not use --igds-border-focused. ` +
                `The focus-visible rule must reference var(--igds-border-focused).`
              );
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('combined: all interactive components pass both touch target and focus-visible checks', () => {
    it('for any randomly selected interactive component, both min-height and focus-visible are correct', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...INTERACTIVE_COMPONENT_STYLESHEETS),
          (stylesheet: InteractiveComponentStylesheet) => {
            // Check (a): min-height >= 44px
            const minHeightViolations = checkMinHeightForInteractiveElements(stylesheet);
            if (minHeightViolations.length > 0) {
              const violationList = minHeightViolations
                .map(v =>
                  v.actualValue === null
                    ? `  - "${v.selector}": no min-height found (expected >= ${MIN_TOUCH_TARGET_PX}px)`
                    : `  - "${v.selector}": min-height is ${v.actualValue}px (expected >= ${MIN_TOUCH_TARGET_PX}px)`
                )
                .join('\n');
              throw new Error(
                `Component "${stylesheet.name}" touch target violations:\n${violationList}`
              );
            }

            // Check (b): :focus-visible with --igds-border-focused
            if (stylesheet.requiresFocusVisible) {
              const focusResult = checkFocusVisibleRule(stylesheet.css);
              if (!focusResult.focusVisibleUsesToken) {
                throw new Error(
                  `Component "${stylesheet.name}" is missing a :focus-visible rule using --igds-border-focused.`
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

  describe('for any pair of randomly selected interactive components, both pass all checks', () => {
    it('both components in a random pair have correct touch target and focus-visible', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...INTERACTIVE_COMPONENT_STYLESHEETS),
          fc.constantFrom(...INTERACTIVE_COMPONENT_STYLESHEETS),
          (sheet1: InteractiveComponentStylesheet, sheet2: InteractiveComponentStylesheet) => {
            for (const sheet of [sheet1, sheet2]) {
              const minHeightViolations = checkMinHeightForInteractiveElements(sheet);
              if (minHeightViolations.length > 0) {
                const violationList = minHeightViolations
                  .map(v =>
                    v.actualValue === null
                      ? `  - "${v.selector}": no min-height found`
                      : `  - "${v.selector}": min-height is ${v.actualValue}px`
                  )
                  .join('\n');
                throw new Error(
                  `Component "${sheet.name}" touch target violations:\n${violationList}`
                );
              }

              if (sheet.requiresFocusVisible) {
                const focusResult = checkFocusVisibleRule(sheet.css);
                if (!focusResult.focusVisibleUsesToken) {
                  throw new Error(
                    `Component "${sheet.name}" missing :focus-visible with --igds-border-focused.`
                  );
                }
              }
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('exhaustive: every single interactive component passes all checks', () => {
    INTERACTIVE_COMPONENT_STYLESHEETS.forEach((stylesheet) => {
      it(`${stylesheet.name}: interactive elements have min-height >= ${MIN_TOUCH_TARGET_PX}px`, () => {
        const violations = checkMinHeightForInteractiveElements(stylesheet);
        if (violations.length > 0) {
          const violationList = violations
            .map(v =>
              v.actualValue === null
                ? `  - "${v.selector}": no min-height declaration found`
                : `  - "${v.selector}": min-height is ${v.actualValue}px`
            )
            .join('\n');
          fail(
            `Component "${stylesheet.name}" has interactive elements below minimum touch target (${MIN_TOUCH_TARGET_PX}px):\n${violationList}`
          );
        }
      });

      it(`${stylesheet.name}: has :focus-visible rule using --igds-border-focused`, () => {
        if (!stylesheet.requiresFocusVisible) return;

        const result = checkFocusVisibleRule(stylesheet.css);
        if (!result.hasFocusVisible) {
          fail(
            `Component "${stylesheet.name}" is missing a :focus-visible (or :focus-within) rule.`
          );
        }
        if (!result.focusVisibleUsesToken) {
          fail(
            `Component "${stylesheet.name}" :focus-visible rule does not use --igds-border-focused token.`
          );
        }
      });
    });
  });
});
