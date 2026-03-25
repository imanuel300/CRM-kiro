import { TestBed } from '@angular/core/testing';
import * as fc from 'fast-check';
import {
  IgdsModalService,
  IgdsModalRef,
} from '@igds/angular';
import { IgdsModalComponent } from '@igds/angular';
import { IgdsButtonComponent } from '@igds/angular';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from './confirm-dialog.component';
import { CommonModule } from '@angular/common';

/**
 * Feature: igds-ui-migration, Property 14: שמירה על ממשק ConfirmDialogData
 *
 * Validates: Requirements 3.5
 *
 * For any valid ConfirmDialogData object (with title, message, and optional
 * confirmText/cancelText), the migrated confirm dialog should render the title
 * and message in the modal, and display buttons with the provided text
 * (or defaults 'אישור'/'ביטול').
 */
describe('Feature: igds-ui-migration, Property 14: שמירה על ממשק ConfirmDialogData', () => {
  let service: IgdsModalService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [IgdsModalComponent, IgdsButtonComponent, ConfirmDialogComponent],
      imports: [CommonModule],
    });
    service = TestBed.inject(IgdsModalService);
  });

  afterEach(() => {
    document.querySelectorAll('body > div > igds-modal').forEach((el) => {
      el.parentElement?.remove();
    });
  });

  /** Generator for non-empty strings (title and message are required) */
  const nonEmptyString = () =>
    fc.string({ minLength: 1, maxLength: 100 }).filter((s) => s.trim().length > 0);

  /** Generator for ConfirmDialogData with all fields provided */
  const confirmDialogDataWithCustomTexts = () =>
    fc.record({
      title: nonEmptyString(),
      message: nonEmptyString(),
      confirmText: nonEmptyString(),
      cancelText: nonEmptyString(),
    });

  /** Generator for ConfirmDialogData with only required fields */
  const confirmDialogDataDefaults = () =>
    fc.record({
      title: nonEmptyString(),
      message: nonEmptyString(),
    });

  /** Generator for ConfirmDialogData with optional fields randomly present */
  const confirmDialogDataArbitrary = () =>
    fc.record(
      {
        title: nonEmptyString(),
        message: nonEmptyString(),
        confirmText: fc.option(nonEmptyString(), { nil: undefined }),
        cancelText: fc.option(nonEmptyString(), { nil: undefined }),
      },
      { requiredKeys: ['title', 'message'] }
    );

  function openConfirmDialog(data: ConfirmDialogData): { ref: IgdsModalRef<boolean>; hostEl: HTMLElement } {
    const ref = service.open<boolean>({
      title: data.title,
      component: ConfirmDialogComponent,
      data,
    });

    // The modal is appended to document.body as a child div
    const hostEl = document.body.querySelector('div > igds-modal') as HTMLElement;
    return { ref, hostEl };
  }

  it('title is rendered in the modal header for any ConfirmDialogData', (done) => {
    fc.assert(
      fc.property(confirmDialogDataArbitrary(), (data) => {
        const { ref, hostEl } = openConfirmDialog(data);

        const titleEl = hostEl?.querySelector('.igds-modal__title');
        if (!titleEl) {
          ref.close();
          throw new Error('Modal title element not found');
        }

        const renderedTitle = titleEl.textContent?.trim();
        if (renderedTitle !== data.title) {
          ref.close();
          throw new Error(
            `Expected title "${data.title}" but got "${renderedTitle}"`
          );
        }

        ref.close();
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('message is rendered in the modal body for any ConfirmDialogData', (done) => {
    fc.assert(
      fc.property(confirmDialogDataArbitrary(), (data) => {
        const { ref, hostEl } = openConfirmDialog(data);

        const messageEl = hostEl?.querySelector('.igds-confirm-dialog__message');
        if (!messageEl) {
          ref.close();
          throw new Error('Message element not found');
        }

        const renderedMessage = messageEl.textContent?.trim();
        if (renderedMessage !== data.message) {
          ref.close();
          throw new Error(
            `Expected message "${data.message}" but got "${renderedMessage}"`
          );
        }

        ref.close();
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('custom confirmText and cancelText are rendered on buttons when provided', (done) => {
    fc.assert(
      fc.property(confirmDialogDataWithCustomTexts(), (data) => {
        const { ref, hostEl } = openConfirmDialog(data);

        const buttons = hostEl?.querySelectorAll('igds-button');
        if (!buttons || buttons.length < 2) {
          ref.close();
          throw new Error(`Expected 2 buttons but found ${buttons?.length ?? 0}`);
        }

        // First button is cancel (secondary), second is confirm (primary)
        const cancelButton = buttons[0];
        const confirmButton = buttons[1];

        const cancelText = cancelButton.textContent?.trim();
        const confirmText = confirmButton.textContent?.trim();

        if (cancelText !== data.cancelText) {
          ref.close();
          throw new Error(
            `Expected cancel button text "${data.cancelText}" but got "${cancelText}"`
          );
        }

        if (confirmText !== data.confirmText) {
          ref.close();
          throw new Error(
            `Expected confirm button text "${data.confirmText}" but got "${confirmText}"`
          );
        }

        ref.close();
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('default button texts are used when confirmText/cancelText are not provided', (done) => {
    fc.assert(
      fc.property(confirmDialogDataDefaults(), (data) => {
        const { ref, hostEl } = openConfirmDialog(data);

        const buttons = hostEl?.querySelectorAll('igds-button');
        if (!buttons || buttons.length < 2) {
          ref.close();
          throw new Error(`Expected 2 buttons but found ${buttons?.length ?? 0}`);
        }

        const cancelText = buttons[0].textContent?.trim();
        const confirmText = buttons[1].textContent?.trim();

        if (cancelText !== 'ביטול') {
          ref.close();
          throw new Error(
            `Expected default cancel text "ביטול" but got "${cancelText}"`
          );
        }

        if (confirmText !== 'אישור') {
          ref.close();
          throw new Error(
            `Expected default confirm text "אישור" but got "${confirmText}"`
          );
        }

        ref.close();
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('buttons use correct variants: secondary for cancel, primary for confirm', (done) => {
    fc.assert(
      fc.property(confirmDialogDataArbitrary(), (data) => {
        const { ref, hostEl } = openConfirmDialog(data);

        const buttons = hostEl?.querySelectorAll('igds-button');
        if (!buttons || buttons.length < 2) {
          ref.close();
          throw new Error(`Expected 2 buttons but found ${buttons?.length ?? 0}`);
        }

        const cancelVariant = buttons[0].getAttribute('variant');
        const confirmVariant = buttons[1].getAttribute('variant');

        if (cancelVariant !== 'secondary') {
          ref.close();
          throw new Error(
            `Expected cancel button variant "secondary" but got "${cancelVariant}"`
          );
        }

        if (confirmVariant !== 'primary') {
          ref.close();
          throw new Error(
            `Expected confirm button variant "primary" but got "${confirmVariant}"`
          );
        }

        ref.close();
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('optional fields default correctly for any combination of provided/missing texts', (done) => {
    fc.assert(
      fc.property(confirmDialogDataArbitrary(), (data) => {
        const { ref, hostEl } = openConfirmDialog(data);

        const buttons = hostEl?.querySelectorAll('igds-button');
        if (!buttons || buttons.length < 2) {
          ref.close();
          throw new Error(`Expected 2 buttons but found ${buttons?.length ?? 0}`);
        }

        const expectedCancel = data.cancelText || 'ביטול';
        const expectedConfirm = data.confirmText || 'אישור';

        const cancelText = buttons[0].textContent?.trim();
        const confirmText = buttons[1].textContent?.trim();

        if (cancelText !== expectedCancel) {
          ref.close();
          throw new Error(
            `Expected cancel text "${expectedCancel}" but got "${cancelText}"`
          );
        }

        if (confirmText !== expectedConfirm) {
          ref.close();
          throw new Error(
            `Expected confirm text "${expectedConfirm}" but got "${confirmText}"`
          );
        }

        ref.close();
      }),
      { numRuns: 100 }
    );
    done();
  });
});
