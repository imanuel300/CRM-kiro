import { TestBed } from '@angular/core/testing';
import * as fc from 'fast-check';
import { IgdsToastService } from '@igds/angular';
import { IgdsToastComponent } from '@igds/angular';

/**
 * Feature: igds-ui-migration, Property 7: סוגי הודעות toast
 *
 * Validates: Requirements 8.5
 *
 * For any notification type in {success, error, warning, info}, calling the
 * corresponding method on IgdsToastService should render an igds-toast with
 * the matching type variant and the provided message text.
 */
describe('Feature: igds-ui-migration, Property 7: סוגי הודעות toast', () => {
  let service: IgdsToastService;

  /**
   * Maps service method names to the expected component `type` input.
   * Note: service.error() maps to 'failure' type on the component.
   */
  const methodToType: Record<string, string> = {
    success: 'success',
    error: 'failure',
    warning: 'warning',
    info: 'info',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [IgdsToastComponent],
    });
    service = TestBed.inject(IgdsToastService);
  });

  afterEach(() => {
    // Clean up any dynamically created toast elements
    document.querySelectorAll('body > div > igds-toast').forEach(el => {
      el.parentElement?.remove();
    });
  });

  it('for any toast type and non-empty message, the rendered toast has the correct type variant CSS class', (done) => {
    fc.assert(
      fc.property(
        fc.constantFrom('success', 'error', 'warning', 'info'),
        fc.string({ minLength: 1 }),
        (method: string, message: string) => {
          // Call the corresponding service method
          (service as any)[method](message);

          const toastEl = document.querySelector('.igds-toast');
          if (!toastEl) {
            throw new Error(`Expected igds-toast element to be rendered for method "${method}"`);
          }

          const expectedType = methodToType[method];
          const expectedClass = `igds-toast--${expectedType}`;
          if (!toastEl.classList.contains(expectedClass)) {
            throw new Error(
              `Expected toast to have class "${expectedClass}" for method "${method}", ` +
              `but found classes: "${toastEl.className}"`
            );
          }

          // Cleanup for next iteration
          (service as any).cleanup();
        }
      ),
      { numRuns: 100 }
    );
    done();
  });

  it('for any toast type and non-empty message, the rendered toast displays the provided message text', (done) => {
    fc.assert(
      fc.property(
        fc.constantFrom('success', 'error', 'warning', 'info'),
        fc.string({ minLength: 1 }),
        (method: string, message: string) => {
          (service as any)[method](message);

          const messageEl = document.querySelector('.igds-toast__message');
          if (!messageEl) {
            throw new Error(`Expected toast message element to be rendered for method "${method}"`);
          }

          const renderedText = messageEl.textContent;
          if (renderedText !== message) {
            throw new Error(
              `Expected toast message "${message}" but got "${renderedText}" for method "${method}"`
            );
          }

          (service as any).cleanup();
        }
      ),
      { numRuns: 100 }
    );
    done();
  });

  it('for any toast type, the rendered toast has role="alert" for accessibility', (done) => {
    fc.assert(
      fc.property(
        fc.constantFrom('success', 'error', 'warning', 'info'),
        fc.string({ minLength: 1 }),
        (method: string, message: string) => {
          (service as any)[method](message);

          const toastEl = document.querySelector('.igds-toast');
          if (!toastEl) {
            throw new Error(`Expected igds-toast element to be rendered for method "${method}"`);
          }

          const role = toastEl.getAttribute('role');
          if (role !== 'alert') {
            throw new Error(
              `Expected toast to have role="alert" but got role="${role}" for method "${method}"`
            );
          }

          (service as any).cleanup();
        }
      ),
      { numRuns: 100 }
    );
    done();
  });

  it('empty message does not render a toast (guard in service)', () => {
    const methods = ['success', 'error', 'warning', 'info'] as const;
    for (const method of methods) {
      (service as any)[method]('');
      const toastEl = document.querySelector('.igds-toast');
      expect(toastEl).toBeNull();
    }
  });
});
