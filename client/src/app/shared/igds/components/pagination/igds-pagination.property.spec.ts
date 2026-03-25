import { TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import * as fc from 'fast-check';
import { IgdsPaginationComponent } from '@igds/angular';

/**
 * Feature: igds-ui-migration, Property 5: חישוב עימוד נכון
 *
 * Validates: Requirements 6.6
 *
 * For any positive totalItems and pageSize from {10, 25, 50}, igds-pagination
 * should calculate totalPages = ceil(totalItems / pageSize), and for any page
 * number p where 1 ≤ p ≤ totalPages, navigating to page p should emit pageChange
 * with value p. Navigation to pages outside this range should be prevented.
 */
describe('Feature: igds-ui-migration, Property 5: חישוב עימוד נכון', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [IgdsPaginationComponent],
      imports: [CommonModule],
    });
  });

  /** Arbitrary for totalItems: positive integer 1–1000 */
  const totalItemsArb = fc.integer({ min: 1, max: 1000 });

  /** Arbitrary for pageSize: one of {10, 25, 50} */
  const pageSizeArb = fc.constantFrom(10, 25, 50);

  describe('totalPages calculation', () => {
    it('totalPages equals ceil(totalItems / pageSize) for any positive totalItems and pageSize', (done) => {
      fc.assert(
        fc.property(totalItemsArb, pageSizeArb, (totalItems, pageSize) => {
          const fixture = TestBed.createComponent(IgdsPaginationComponent);
          const component = fixture.componentInstance;
          component.totalItems = totalItems;
          component.pageSize = pageSize;

          const expected = Math.ceil(totalItems / pageSize);
          if (component.totalPages !== expected) {
            throw new Error(
              `totalItems=${totalItems}, pageSize=${pageSize}: ` +
              `expected totalPages=${expected}, got ${component.totalPages}`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('valid page navigation emits pageChange', () => {
    it('navigating to any valid page p (1 ≤ p ≤ totalPages) emits pageChange with value p', (done) => {
      fc.assert(
        fc.property(totalItemsArb, pageSizeArb, (totalItems, pageSize) => {
          const totalPages = Math.ceil(totalItems / pageSize);

          // Pick a random valid target page within range
          const targetPage = fc.sample(fc.integer({ min: 1, max: totalPages }), 1)[0];

          const fixture = TestBed.createComponent(IgdsPaginationComponent);
          const component = fixture.componentInstance;
          component.totalItems = totalItems;
          component.pageSize = pageSize;
          // Set currentPage to something different so goTo doesn't skip
          component.currentPage = targetPage === 1 ? (totalPages > 1 ? 2 : 1) : 1;
          fixture.detectChanges();

          const emitted: number[] = [];
          component.pageChange.subscribe((p: number) => emitted.push(p));

          component.goTo(targetPage);

          // If currentPage was already targetPage, goTo skips — that's valid
          if (component.currentPage === targetPage && emitted.length === 0) {
            // goTo correctly skipped because we were already on that page
            fixture.destroy();
            return;
          }

          if (emitted.length !== 1) {
            throw new Error(
              `totalItems=${totalItems}, pageSize=${pageSize}, targetPage=${targetPage}: ` +
              `expected 1 pageChange event, got ${emitted.length}`
            );
          }
          if (emitted[0] !== targetPage) {
            throw new Error(
              `totalItems=${totalItems}, pageSize=${pageSize}: ` +
              `expected pageChange to emit ${targetPage}, got ${emitted[0]}`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('out-of-range navigation is prevented', () => {
    it('navigating to page 0 or below does not emit pageChange', (done) => {
      fc.assert(
        fc.property(totalItemsArb, pageSizeArb, (totalItems, pageSize) => {
          const fixture = TestBed.createComponent(IgdsPaginationComponent);
          const component = fixture.componentInstance;
          component.totalItems = totalItems;
          component.pageSize = pageSize;
          component.currentPage = 1;
          fixture.detectChanges();

          const emitted: number[] = [];
          component.pageChange.subscribe((p: number) => emitted.push(p));

          component.goTo(0);
          component.goTo(-1);
          component.goTo(-100);

          if (emitted.length !== 0) {
            throw new Error(
              `totalItems=${totalItems}, pageSize=${pageSize}: ` +
              `navigation to pages ≤ 0 should not emit, but got ${emitted.length} events: [${emitted}]`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });

    it('navigating beyond totalPages does not emit pageChange', (done) => {
      fc.assert(
        fc.property(totalItemsArb, pageSizeArb, (totalItems, pageSize) => {
          const totalPages = Math.ceil(totalItems / pageSize);

          const fixture = TestBed.createComponent(IgdsPaginationComponent);
          const component = fixture.componentInstance;
          component.totalItems = totalItems;
          component.pageSize = pageSize;
          component.currentPage = 1;
          fixture.detectChanges();

          const emitted: number[] = [];
          component.pageChange.subscribe((p: number) => emitted.push(p));

          component.goTo(totalPages + 1);
          component.goTo(totalPages + 100);
          component.goTo(9999);

          if (emitted.length !== 0) {
            throw new Error(
              `totalItems=${totalItems}, pageSize=${pageSize}, totalPages=${totalPages}: ` +
              `navigation beyond totalPages should not emit, but got ${emitted.length} events: [${emitted}]`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });

    it('navigating to current page does not emit pageChange', (done) => {
      fc.assert(
        fc.property(totalItemsArb, pageSizeArb, (totalItems, pageSize) => {
          const fixture = TestBed.createComponent(IgdsPaginationComponent);
          const component = fixture.componentInstance;
          component.totalItems = totalItems;
          component.pageSize = pageSize;
          component.currentPage = 1;
          fixture.detectChanges();

          const emitted: number[] = [];
          component.pageChange.subscribe((p: number) => emitted.push(p));

          component.goTo(1); // already on page 1

          if (emitted.length !== 0) {
            throw new Error(
              `Navigating to current page should not emit, but got ${emitted.length} events`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('sequential page navigation', () => {
    it('navigating through all pages sequentially emits correct page numbers', (done) => {
      fc.assert(
        fc.property(
          fc.integer({ min: 1, max: 200 }),
          pageSizeArb,
          (totalItems, pageSize) => {
            const totalPages = Math.ceil(totalItems / pageSize);

            const fixture = TestBed.createComponent(IgdsPaginationComponent);
            const component = fixture.componentInstance;
            component.totalItems = totalItems;
            component.pageSize = pageSize;
            component.currentPage = 1;
            fixture.detectChanges();

            const emitted: number[] = [];
            component.pageChange.subscribe((p: number) => emitted.push(p));

            // Navigate forward through all pages
            for (let p = 2; p <= totalPages; p++) {
              component.goTo(p);
            }

            const expectedCount = totalPages - 1; // pages 2..totalPages
            if (emitted.length !== expectedCount) {
              throw new Error(
                `totalItems=${totalItems}, pageSize=${pageSize}, totalPages=${totalPages}: ` +
                `expected ${expectedCount} events, got ${emitted.length}`
              );
            }

            for (let i = 0; i < emitted.length; i++) {
              const expectedPage = i + 2;
              if (emitted[i] !== expectedPage) {
                throw new Error(
                  `Event ${i}: expected page ${expectedPage}, got ${emitted[i]}`
                );
              }
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
