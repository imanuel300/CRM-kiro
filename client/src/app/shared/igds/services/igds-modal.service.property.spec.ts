import { TestBed } from '@angular/core/testing';
import * as fc from 'fast-check';
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  IgdsModalService,
  IgdsModalRef,
  IGDS_MODAL_DATA,
  IGDS_MODAL_REF,
} from '@igds/angular';
import { IgdsModalComponent } from '@igds/angular';

/**
 * Dummy content component used inside modals during testing.
 * It injects IGDS_MODAL_DATA and IGDS_MODAL_REF to verify data round-trip.
 */
@Component({
  selector: 'test-modal-content',
  template: `<span class="test-data">{{ data | json }}</span>`,
})
class TestModalContentComponent {
  constructor(
    @Inject(IGDS_MODAL_DATA) public data: any,
    @Inject(IGDS_MODAL_REF) public modalRef: IgdsModalRef
  ) {}
}

/**
 * Feature: igds-ui-migration, Property 6: זרימת נתונים בדיאלוגים (round-trip)
 *
 * Validates: Requirements 8.4, 17.4
 *
 * For any modal opened via IgdsModalService.open() with input data, and closed
 * with a result value R, the afterClosed() Observable should emit exactly R.
 * If the modal is dismissed without a result, afterClosed() should emit undefined.
 */
describe('Feature: igds-ui-migration, Property 6: זרימת נתונים בדיאלוגים (round-trip)', () => {
  let service: IgdsModalService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [IgdsModalComponent, TestModalContentComponent],
      imports: [CommonModule],
    });
    service = TestBed.inject(IgdsModalService);
  });

  afterEach(() => {
    document.querySelectorAll('body > div > igds-modal').forEach(el => {
      el.parentElement?.remove();
    });
  });

  /** Arbitrary for JSON-serializable result values */
  const resultArb = fc.oneof(
    fc.string(),
    fc.integer(),
    fc.boolean(),
    fc.constant(null),
    fc.dictionary(fc.string({ minLength: 1, maxLength: 10 }), fc.string()),
    fc.array(fc.integer(), { maxLength: 5 })
  );

  /** Arbitrary for modal config data (the data passed into the modal) */
  const modalDataArb = fc.oneof(
    fc.string(),
    fc.integer(),
    fc.record({ key: fc.string(), value: fc.integer() }),
    fc.constant(undefined),
    fc.constant(null)
  );

  /** Arbitrary for non-empty title strings */
  const titleArb = fc.string({ minLength: 1, maxLength: 80 }).filter(s => s.trim().length > 0);

  it('closing with a result value R causes afterClosed() to emit exactly R', (done) => {
    fc.assert(
      fc.property(titleArb, resultArb, (title, result) => {
        const ref = service.open<any>({
          title,
          component: TestModalContentComponent,
          data: { payload: result },
        });

        let emittedValue: any = Symbol('NOT_EMITTED');
        let emitCount = 0;

        ref.afterClosed().subscribe({
          next: (val) => {
            emittedValue = val;
            emitCount++;
          },
        });

        ref.close(result);

        if (emitCount !== 1) {
          throw new Error(
            `Expected afterClosed() to emit exactly 1 value, but emitted ${emitCount}`
          );
        }

        // Deep equality check for objects/arrays
        const expected = JSON.stringify(result);
        const actual = JSON.stringify(emittedValue);
        if (expected !== actual) {
          throw new Error(
            `Expected afterClosed() to emit ${expected} but got ${actual}`
          );
        }
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('dismissing without a result causes afterClosed() to emit undefined', (done) => {
    fc.assert(
      fc.property(titleArb, modalDataArb, (title, data) => {
        const ref = service.open<any>({
          title,
          component: TestModalContentComponent,
          data,
        });

        let emittedValue: any = Symbol('NOT_EMITTED');
        let emitCount = 0;

        ref.afterClosed().subscribe({
          next: (val) => {
            emittedValue = val;
            emitCount++;
          },
        });

        // Close without providing a result
        ref.close();

        if (emitCount !== 1) {
          throw new Error(
            `Expected afterClosed() to emit exactly 1 value, but emitted ${emitCount}`
          );
        }

        if (emittedValue !== undefined) {
          throw new Error(
            `Expected afterClosed() to emit undefined but got ${JSON.stringify(emittedValue)}`
          );
        }
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('afterClosed() Observable completes after close is called', (done) => {
    fc.assert(
      fc.property(titleArb, resultArb, (title, result) => {
        const ref = service.open<any>({
          title,
          component: TestModalContentComponent,
          data: {},
        });

        let completed = false;

        ref.afterClosed().subscribe({
          complete: () => {
            completed = true;
          },
        });

        ref.close(result);

        if (!completed) {
          throw new Error('Expected afterClosed() Observable to complete after close()');
        }
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('calling close() multiple times only emits once', (done) => {
    fc.assert(
      fc.property(titleArb, resultArb, resultArb, (title, result1, result2) => {
        const ref = service.open<any>({
          title,
          component: TestModalContentComponent,
          data: {},
        });

        let emitCount = 0;
        let firstValue: any;

        ref.afterClosed().subscribe({
          next: (val) => {
            if (emitCount === 0) firstValue = val;
            emitCount++;
          },
        });

        ref.close(result1);
        ref.close(result2); // second call should be a no-op

        if (emitCount !== 1) {
          throw new Error(
            `Expected exactly 1 emission but got ${emitCount} after double close()`
          );
        }

        const expected = JSON.stringify(result1);
        const actual = JSON.stringify(firstValue);
        if (expected !== actual) {
          throw new Error(
            `Expected first result ${expected} but got ${actual}`
          );
        }
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('input data is accessible to the content component via IGDS_MODAL_DATA injection token', (done) => {
    fc.assert(
      fc.property(titleArb, modalDataArb, (title, data) => {
        const ref = service.open<any>({
          title,
          component: TestModalContentComponent,
          data,
        });

        const dataEl = document.querySelector('.test-data');
        if (!dataEl) {
          ref.close();
          throw new Error('Content component was not rendered inside the modal');
        }

        const renderedData = dataEl.textContent?.trim();
        const expectedData = JSON.stringify(data);

        // Angular's json pipe outputs similar to JSON.stringify
        // For undefined, the pipe renders nothing
        if (data !== undefined && renderedData !== expectedData) {
          ref.close();
          throw new Error(
            `Expected injected data ${expectedData} but rendered "${renderedData}"`
          );
        }

        ref.close();
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('modal renders with the provided title for any non-empty title string', (done) => {
    fc.assert(
      fc.property(titleArb, (title) => {
        const ref = service.open<any>({
          title,
          component: TestModalContentComponent,
          data: {},
        });

        const titleEl = document.querySelector('.igds-modal__title');
        if (!titleEl) {
          ref.close();
          throw new Error('Modal title element not found');
        }

        const renderedTitle = titleEl.textContent?.trim();
        if (renderedTitle !== title) {
          ref.close();
          throw new Error(
            `Expected title "${title}" but got "${renderedTitle}"`
          );
        }

        ref.close();
      }),
      { numRuns: 100 }
    );
    done();
  });
});
