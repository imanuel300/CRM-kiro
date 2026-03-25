import * as fc from 'fast-check';

/**
 * Feature: igds-ui-migration, Property 10: שימוש בלעדי ב-Design Tokens
 *
 * Validates: Requirements 10.3, 10.5
 *
 * For any migrated component's stylesheet, no hardcoded color values
 * (hex #xxx, rgb(), rgba() except in token definitions), hardcoded font-family
 * strings, or hardcoded spacing pixel values should appear. All such values
 * should reference IGDS CSS custom properties (var(--igds-*)).
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
 * Patterns that detect hardcoded color values.
 * Matches hex colors (#xxx, #xxxx, #xxxxxx, #xxxxxxxx), rgb(), rgba(), hsl(), hsla().
 */
const HARDCODED_COLOR_PATTERNS: Array<{
  pattern: RegExp;
  description: string;
}> = [
  {
    pattern: /#[0-9a-fA-F]{3,8}\b/g,
    description: 'hardcoded hex color',
  },
  {
    pattern: /(?<![a-zA-Z-])rgb\s*\(/g,
    description: 'hardcoded rgb() color',
  },
  {
    pattern: /(?<![a-zA-Z-])hsl\s*\(/g,
    description: 'hardcoded hsl() color',
  },
  {
    pattern: /(?<![a-zA-Z-])hsla\s*\(/g,
    description: 'hardcoded hsla() color',
  },
];

