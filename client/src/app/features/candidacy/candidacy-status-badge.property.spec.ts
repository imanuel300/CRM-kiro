import { TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import * as fc from 'fast-check';
import { IgdsStatusBadgeComponent } from '@igds/angular';

/**
 * Feature: igds-ui-migration, Property 8: מיפוי סטטוס מועמדות ל-status-badge
 *
 * Validates: Requirements 9.3
 *
 * For any candidacy status value, the migrated component should render an
 * igds-status-badge with the correct variant (success/warning/failure/info/neutral)
 * corresponding to that status.
 *
 * The candidacy module uses igds-status-badge in two components:
 *
 * 1. candidacy-detail: maps isActive boolean
 *    - isActive === true  → variant="success", text="פעילה"
 *    - isActive === false → variant="neutral", text="לא פעילה"
 *
 * 2. status-timeline: maps status history entries
 *    - fromStatusId (previous status) → variant="neutral"
 *    - toStatusId (current/new status) → variant="info"
 */

/**
 * Helper: maps isActive to the expected status-badge variant,
 * mirroring the logic in candidacy-detail.component.ts
 */
function getActiveVariant(isActive: boolean): 'success' | 'neutral' {
  return isActive ? 'success' : 'neutral';
}

/**
 * Helper: maps isActive to the expected status-badge text,
 * mirroring the logic in candidacy-detail.component.ts
 */
function getActiveText(isActive: boolean): string {
  return isActive ? 'פעילה' : 'לא פעילה';
}

/**
 * Test host component that mirrors candidacy-detail's status-badge usage:
 *   <igds-status-badge [variant]="isActive ? 'success' : 'neutral'" [text]="isActive ? 'פעילה' : 'לא פעילה'">
 */
@Component({
  selector: 'test-candidacy-active-badge',
  template: `
    <igds-status-badge
      [variant]="isActive ? 'success' : 'neutral'"
      [text]="isActive ? 'פעילה' : 'לא פעילה'">
    </igds-status-badge>
  `,
})
class TestCandidacyActiveBadgeComponent {
  @Input() isActive = false;
}

/**
 * Test host component that mirrors status-timeline's status-badge usage:
 *   fromStatusId → variant="neutral"
 *   toStatusId   → variant="info"
 */
@Component({
  selector: 'test-timeline-status-badge',
  template: `
    <igds-status-badge
      *ngIf="fromStatusId !== null"
      variant="neutral"
      [text]="'סטטוס ' + fromStatusId">
    </igds-status-badge>
    <igds-status-badge
      variant="info"
      [text]="'סטטוס ' + toStatusId">
    </igds-status-badge>
  `,
})
class TestTimelineStatusBadgeComponent {
  @Input() fromStatusId: number | null = null;
  @Input() toStatusId = 0;
}

describe('Feature: igds-ui-migration, Property 8: מיפוי סטטוס מועמדות ל-status-badge', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [
        IgdsStatusBadgeComponent,
        TestCandidacyActiveBadgeComponent,
        TestTimelineStatusBadgeComponent,
      ],
      imports: [CommonModule],
    });
  });

  describe('candidacy-detail: isActive → status-badge variant mapping', () => {
    it('for any boolean isActive, renders the correct variant and text', (done) => {
      fc.assert(
        fc.property(fc.boolean(), (isActive) => {
          const fixture = TestBed.createComponent(TestCandidacyActiveBadgeComponent);
          fixture.componentInstance.isActive = isActive;
          fixture.detectChanges();

          const badgeEl = fixture.nativeElement.querySelector('igds-status-badge .igds-badge');
          if (!badgeEl) {
            throw new Error('igds-status-badge element not found');
          }

          const expectedVariant = getActiveVariant(isActive);
          const expectedClass = `igds-badge--${expectedVariant}`;
          if (!badgeEl.classList.contains(expectedClass)) {
            throw new Error(
              `isActive=${isActive}: expected class "${expectedClass}" but found classes "${badgeEl.className}"`
            );
          }

          const expectedText = getActiveText(isActive);
          const actualText = badgeEl.textContent?.trim();
          if (actualText !== expectedText) {
            throw new Error(
              `isActive=${isActive}: expected text "${expectedText}" but got "${actualText}"`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });

    it('active candidacy always maps to success variant', (done) => {
      fc.assert(
        fc.property(fc.constant(true), (isActive) => {
          const fixture = TestBed.createComponent(TestCandidacyActiveBadgeComponent);
          fixture.componentInstance.isActive = isActive;
          fixture.detectChanges();

          const badgeEl = fixture.nativeElement.querySelector('igds-status-badge .igds-badge');
          if (!badgeEl) throw new Error('Badge element not found');

          if (!badgeEl.classList.contains('igds-badge--success')) {
            throw new Error(
              `Active candidacy should have success variant, got classes: "${badgeEl.className}"`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });

    it('inactive candidacy always maps to neutral variant', (done) => {
      fc.assert(
        fc.property(fc.constant(false), (isActive) => {
          const fixture = TestBed.createComponent(TestCandidacyActiveBadgeComponent);
          fixture.componentInstance.isActive = isActive;
          fixture.detectChanges();

          const badgeEl = fixture.nativeElement.querySelector('igds-status-badge .igds-badge');
          if (!badgeEl) throw new Error('Badge element not found');

          if (!badgeEl.classList.contains('igds-badge--neutral')) {
            throw new Error(
              `Inactive candidacy should have neutral variant, got classes: "${badgeEl.className}"`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('status-timeline: status history → status-badge variant mapping', () => {
    it('for any toStatusId, renders info variant with correct text', (done) => {
      fc.assert(
        fc.property(
          fc.integer({ min: 1, max: 1000 }),
          (toStatusId) => {
            const fixture = TestBed.createComponent(TestTimelineStatusBadgeComponent);
            fixture.componentInstance.fromStatusId = null;
            fixture.componentInstance.toStatusId = toStatusId;
            fixture.detectChanges();

            const badges = fixture.nativeElement.querySelectorAll('igds-status-badge .igds-badge');
            // With fromStatusId=null, only the toStatus badge should render
            if (badges.length !== 1) {
              throw new Error(`Expected 1 badge (toStatus only), found ${badges.length}`);
            }

            const toBadge = badges[0];
            if (!toBadge.classList.contains('igds-badge--info')) {
              throw new Error(
                `toStatusId=${toStatusId}: expected info variant but got classes "${toBadge.className}"`
              );
            }

            const expectedText = `סטטוס ${toStatusId}`;
            const actualText = toBadge.textContent?.trim();
            if (actualText !== expectedText) {
              throw new Error(
                `toStatusId=${toStatusId}: expected text "${expectedText}" but got "${actualText}"`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('for any fromStatusId, renders neutral variant with correct text', (done) => {
      fc.assert(
        fc.property(
          fc.integer({ min: 1, max: 1000 }),
          fc.integer({ min: 1, max: 1000 }),
          (fromStatusId, toStatusId) => {
            const fixture = TestBed.createComponent(TestTimelineStatusBadgeComponent);
            fixture.componentInstance.fromStatusId = fromStatusId;
            fixture.componentInstance.toStatusId = toStatusId;
            fixture.detectChanges();

            const badges = fixture.nativeElement.querySelectorAll('igds-status-badge .igds-badge');
            // With fromStatusId set, both badges should render
            if (badges.length !== 2) {
              throw new Error(`Expected 2 badges (from + to), found ${badges.length}`);
            }

            // First badge: fromStatus → neutral
            const fromBadge = badges[0];
            if (!fromBadge.classList.contains('igds-badge--neutral')) {
              throw new Error(
                `fromStatusId=${fromStatusId}: expected neutral variant but got classes "${fromBadge.className}"`
              );
            }

            const expectedFromText = `סטטוס ${fromStatusId}`;
            const actualFromText = fromBadge.textContent?.trim();
            if (actualFromText !== expectedFromText) {
              throw new Error(
                `fromStatusId=${fromStatusId}: expected text "${expectedFromText}" but got "${actualFromText}"`
              );
            }

            // Second badge: toStatus → info
            const toBadge = badges[1];
            if (!toBadge.classList.contains('igds-badge--info')) {
              throw new Error(
                `toStatusId=${toStatusId}: expected info variant but got classes "${toBadge.className}"`
              );
            }

            const expectedToText = `סטטוס ${toStatusId}`;
            const actualToText = toBadge.textContent?.trim();
            if (actualToText !== expectedToText) {
              throw new Error(
                `toStatusId=${toStatusId}: expected text "${expectedToText}" but got "${actualToText}"`
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

  describe('igds-status-badge: variant rendering correctness', () => {
    const allVariants: Array<'success' | 'warning' | 'failure' | 'info' | 'neutral'> =
      ['success', 'warning', 'failure', 'info', 'neutral'];

    it('for any variant and text, renders the correct CSS class and text content', (done) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...allVariants),
          fc.string({ minLength: 1, maxLength: 50 }).filter(s => s.trim().length > 0),
          (variant, text) => {
            const fixture = TestBed.createComponent(IgdsStatusBadgeComponent);
            fixture.componentInstance.variant = variant;
            fixture.componentInstance.text = text;
            fixture.detectChanges();

            const badgeEl = fixture.nativeElement.querySelector('.igds-badge');
            if (!badgeEl) {
              throw new Error('Badge element not found');
            }

            const expectedClass = `igds-badge--${variant}`;
            if (!badgeEl.classList.contains(expectedClass)) {
              throw new Error(
                `variant="${variant}": expected class "${expectedClass}" but got "${badgeEl.className}"`
              );
            }

            // Verify the text is rendered
            const actualText = badgeEl.textContent?.trim();
            if (actualText !== text.trim()) {
              throw new Error(
                `Expected text "${text.trim()}" but got "${actualText}"`
              );
            }

            // Verify role="status" for accessibility
            if (badgeEl.getAttribute('role') !== 'status') {
              throw new Error(
                `Expected role="status" but got "${badgeEl.getAttribute('role')}"`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('each variant produces a unique CSS class (no variant collision)', (done) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...allVariants),
          (variant) => {
            const fixture = TestBed.createComponent(IgdsStatusBadgeComponent);
            fixture.componentInstance.variant = variant;
            fixture.componentInstance.text = 'test';
            fixture.detectChanges();

            const badgeEl = fixture.nativeElement.querySelector('.igds-badge');
            if (!badgeEl) throw new Error('Badge element not found');

            // Should have exactly one variant class
            const variantClasses = allVariants
              .map(v => `igds-badge--${v}`)
              .filter(cls => badgeEl.classList.contains(cls));

            if (variantClasses.length !== 1) {
              throw new Error(
                `Expected exactly 1 variant class, found ${variantClasses.length}: ${variantClasses.join(', ')}`
              );
            }

            if (variantClasses[0] !== `igds-badge--${variant}`) {
              throw new Error(
                `Expected class "igds-badge--${variant}" but found "${variantClasses[0]}"`
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
