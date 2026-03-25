import { TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import * as fc from 'fast-check';

import { IgdsButtonComponent } from '@igds/angular';

/**
 * Feature: igds-ui-migration, Property 15: שמירה על מאפייני כפתורים
 *
 * Validates: Requirements 4.4
 *
 * For any button with disabled, type, and/or aria-label attributes,
 * the migrated igds-button should preserve all these attributes with their original values.
 */

// ─── Test host component ────────────────────────────────────────────────────────

@Component({
  selector: 'test-button-attrs-host',
  template: `
    <igds-button
      [variant]="variant"
      [disabled]="disabled"
      [type]="type"
      [ariaLabel]="ariaLabel"
      [iconOnly]="iconOnly">
      {{ label }}
    </igds-button>
  `,
})
class TestButtonAttrsHostComponent {
  @Input() variant: 'primary' | 'secondary' | 'link' = 'primary';
  @Input() disabled = false;
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() ariaLabel = '';
  @Input() iconOnly = false;
  @Input() label = 'לחצן';
}

// ─── Generators ─────────────────────────────────────────────────────────────────

const arbVariant = fc.constantFrom<'primary' | 'secondary' | 'link'>('primary', 'secondary', 'link');
const arbType = fc.constantFrom<'button' | 'submit' | 'reset'>('button', 'submit', 'reset');
const arbAriaLabel = fc.stringOf(
  fc.constantFrom(...'אבגדהוזחטיכלמנסעפצקרשת abcdefghijklmnopqrstuvwxyz0123456789 '.split('')),
  { minLength: 1, maxLength: 30 }
);

// ─── Test suite ─────────────────────────────────────────────────────────────────

describe('Feature: igds-ui-migration, Property 15: שמירה על מאפייני כפתורים', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [
        IgdsButtonComponent,
        TestButtonAttrsHostComponent,
      ],
      imports: [CommonModule],
    });
  });

  // ── disabled attribute preservation ───────────────────────────────────────

  describe('disabled attribute preservation', () => {
    it('for any disabled state, the rendered button preserves it', (done) => {
      fc.assert(
        fc.property(
          fc.boolean(),
          arbVariant,
          (disabled, variant) => {
            const fixture = TestBed.createComponent(TestButtonAttrsHostComponent);
            fixture.componentInstance.disabled = disabled;
            fixture.componentInstance.variant = variant;
            fixture.detectChanges();

            const btn: HTMLButtonElement = fixture.nativeElement.querySelector('button.igds-btn');
            if (!btn) throw new Error('Button element not found');

            if (btn.disabled !== disabled) {
              throw new Error(
                `Expected disabled=${disabled} but got disabled=${btn.disabled}`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  // ── type attribute preservation ───────────────────────────────────────────

  describe('type attribute preservation', () => {
    it('for any type value (button/submit/reset), the rendered button preserves it', (done) => {
      fc.assert(
        fc.property(
          arbType,
          arbVariant,
          (type, variant) => {
            const fixture = TestBed.createComponent(TestButtonAttrsHostComponent);
            fixture.componentInstance.type = type;
            fixture.componentInstance.variant = variant;
            fixture.detectChanges();

            const btn: HTMLButtonElement = fixture.nativeElement.querySelector('button.igds-btn');
            if (!btn) throw new Error('Button element not found');

            if (btn.type !== type) {
              throw new Error(
                `Expected type="${type}" but got type="${btn.type}"`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  // ── aria-label attribute preservation ─────────────────────────────────────

  describe('aria-label attribute preservation', () => {
    it('for any aria-label string, the rendered button preserves it', (done) => {
      fc.assert(
        fc.property(
          arbAriaLabel,
          arbVariant,
          (ariaLabel, variant) => {
            const fixture = TestBed.createComponent(TestButtonAttrsHostComponent);
            fixture.componentInstance.ariaLabel = ariaLabel;
            fixture.componentInstance.variant = variant;
            fixture.detectChanges();

            const btn: HTMLButtonElement = fixture.nativeElement.querySelector('button.igds-btn');
            if (!btn) throw new Error('Button element not found');

            const actual = btn.getAttribute('aria-label');
            if (actual !== ariaLabel) {
              throw new Error(
                `Expected aria-label="${ariaLabel}" but got aria-label="${actual}"`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  // ── combined: all attributes preserved together ───────────────────────────

  describe('combined: all attributes preserved together', () => {
    it('for any combination of disabled, type, and aria-label, all are preserved simultaneously', (done) => {
      fc.assert(
        fc.property(
          fc.boolean(),
          arbType,
          arbAriaLabel,
          arbVariant,
          fc.boolean(),
          (disabled, type, ariaLabel, variant, iconOnly) => {
            const fixture = TestBed.createComponent(TestButtonAttrsHostComponent);
            fixture.componentInstance.disabled = disabled;
            fixture.componentInstance.type = type;
            fixture.componentInstance.ariaLabel = ariaLabel;
            fixture.componentInstance.variant = variant;
            fixture.componentInstance.iconOnly = iconOnly;
            fixture.detectChanges();

            const btn: HTMLButtonElement = fixture.nativeElement.querySelector('button.igds-btn');
            if (!btn) throw new Error('Button element not found');

            // Check disabled
            if (btn.disabled !== disabled) {
              throw new Error(
                `disabled: expected ${disabled} but got ${btn.disabled}`
              );
            }

            // Check type
            if (btn.type !== type) {
              throw new Error(
                `type: expected "${type}" but got "${btn.type}"`
              );
            }

            // Check aria-label
            const actual = btn.getAttribute('aria-label');
            if (actual !== ariaLabel) {
              throw new Error(
                `aria-label: expected "${ariaLabel}" but got "${actual}"`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });
});
