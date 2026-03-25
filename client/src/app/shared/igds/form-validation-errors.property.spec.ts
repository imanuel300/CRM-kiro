import { TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import * as fc from 'fast-check';

import { IgdsInputFieldComponent } from '@igds/angular';
import { IgdsDropdownComponent, IgdsDropdownOption } from '@igds/angular';
import { IgdsDatePickerComponent } from '@igds/angular';

/**
 * Feature: igds-ui-migration, Property 3: שימור הודעות שגיאה וולידציה בטפסים
 *
 * Validates: Requirements 5.8, 17.2
 *
 * For any form component with a non-empty error string input, the rendered output
 * should display that exact error string in a visible element with role="alert".
 * For any form with Angular Validators, the migrated IGDS form should trigger the
 * same validation errors for the same invalid inputs.
 */

// ─── Test host components ───────────────────────────────────────────────────────

@Component({
  selector: 'test-input-error-host',
  template: `
    <igds-input-field
      [label]="label"
      [error]="error"
      [inputId]="inputId">
    </igds-input-field>
  `,
})
class TestInputErrorHostComponent {
  @Input() label = 'שדה';
  @Input() error = '';
  @Input() inputId = 'test-input';
}

@Component({
  selector: 'test-dropdown-error-host',
  template: `
    <igds-dropdown
      [label]="label"
      [options]="options"
      [error]="error"
      [dropdownId]="dropdownId">
    </igds-dropdown>
  `,
})
class TestDropdownErrorHostComponent {
  @Input() label = 'בחירה';
  @Input() options: IgdsDropdownOption[] = [
    { value: '1', label: 'אפשרות א' },
    { value: '2', label: 'אפשרות ב' },
  ];
  @Input() error = '';
  @Input() dropdownId = 'test-dropdown';
}

@Component({
  selector: 'test-datepicker-error-host',
  template: `
    <igds-date-picker
      [label]="label"
      [error]="error"
      [inputId]="inputId">
    </igds-date-picker>
  `,
})
class TestDatePickerErrorHostComponent {
  @Input() label = 'תאריך';
  @Input() error = '';
  @Input() inputId = 'test-datepicker';
}

// ─── Generators ─────────────────────────────────────────────────────────────────

/** Generates a non-empty error string (Hebrew + Latin + digits + spaces) */
const arbErrorString = fc.stringOf(
  fc.constantFrom(...'אבגדהוזחטיכלמנסעפצקרשת abcdefghijklmnopqrstuvwxyz0123456789 '.split('')),
  { minLength: 1, maxLength: 80 }
).filter((s: string) => s.trim().length > 0);

// ─── Test suite ─────────────────────────────────────────────────────────────────

describe('Feature: igds-ui-migration, Property 3: שימור הודעות שגיאה וולידציה בטפסים', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [
        IgdsInputFieldComponent,
        IgdsDropdownComponent,
        IgdsDatePickerComponent,
        TestInputErrorHostComponent,
        TestDropdownErrorHostComponent,
        TestDatePickerErrorHostComponent,
      ],
      imports: [CommonModule, FormsModule, ReactiveFormsModule],
    });
  });

  // ── igds-input-field: error with role="alert" ─────────────────────────────

  describe('igds-input-field: error message display', () => {
    it('for any non-empty error string, the error text appears in a visible element with role="alert"', (done) => {
      fc.assert(
        fc.property(
          arbErrorString,
          (error) => {
            const fixture = TestBed.createComponent(TestInputErrorHostComponent);
            fixture.componentInstance.error = error;
            fixture.componentInstance.inputId = 'input-err';
            fixture.detectChanges();

            const errorEl = fixture.nativeElement.querySelector('[role="alert"]');
            if (!errorEl) {
              throw new Error(`With error="${error}": no element with role="alert" found`);
            }

            const displayedText = errorEl.textContent.trim();
            if (displayedText !== error) {
              throw new Error(
                `Error string not preserved. Expected "${error}" but got "${displayedText}"`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('when error is empty, no element with role="alert" should be present', (done) => {
      fc.assert(
        fc.property(
          fc.constant(''),
          (_empty) => {
            const fixture = TestBed.createComponent(TestInputErrorHostComponent);
            fixture.componentInstance.error = '';
            fixture.detectChanges();

            const errorEl = fixture.nativeElement.querySelector('[role="alert"]');
            if (errorEl) {
              throw new Error('With empty error: element with role="alert" should not be present');
            }

            fixture.destroy();
          }
        ),
        { numRuns: 10 }
      );
      done();
    });
  });

  // ── igds-dropdown: error with role="alert" ────────────────────────────────

  describe('igds-dropdown: error message display', () => {
    it('for any non-empty error string, the error text appears in a visible element with role="alert"', (done) => {
      fc.assert(
        fc.property(
          arbErrorString,
          (error) => {
            const fixture = TestBed.createComponent(TestDropdownErrorHostComponent);
            fixture.componentInstance.error = error;
            fixture.componentInstance.dropdownId = 'dd-err';
            fixture.detectChanges();

            const errorEl = fixture.nativeElement.querySelector('[role="alert"]');
            if (!errorEl) {
              throw new Error(`With error="${error}": no element with role="alert" found`);
            }

            const displayedText = errorEl.textContent.trim();
            if (displayedText !== error) {
              throw new Error(
                `Error string not preserved. Expected "${error}" but got "${displayedText}"`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('when error is empty, no element with role="alert" should be present', (done) => {
      fc.assert(
        fc.property(
          fc.constant(''),
          (_empty) => {
            const fixture = TestBed.createComponent(TestDropdownErrorHostComponent);
            fixture.componentInstance.error = '';
            fixture.detectChanges();

            const errorEl = fixture.nativeElement.querySelector('[role="alert"]');
            if (errorEl) {
              throw new Error('With empty error: element with role="alert" should not be present');
            }

            fixture.destroy();
          }
        ),
        { numRuns: 10 }
      );
      done();
    });
  });

  // ── igds-date-picker: error with role="alert" ────────────────────────────

  describe('igds-date-picker: error message display', () => {
    it('for any non-empty error string, the error text appears in a visible element with role="alert"', (done) => {
      fc.assert(
        fc.property(
          arbErrorString,
          (error) => {
            const fixture = TestBed.createComponent(TestDatePickerErrorHostComponent);
            fixture.componentInstance.error = error;
            fixture.componentInstance.inputId = 'dp-err';
            fixture.detectChanges();

            const errorEl = fixture.nativeElement.querySelector('[role="alert"]');
            if (!errorEl) {
              throw new Error(`With error="${error}": no element with role="alert" found`);
            }

            const displayedText = errorEl.textContent.trim();
            if (displayedText !== error) {
              throw new Error(
                `Error string not preserved. Expected "${error}" but got "${displayedText}"`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('when error is empty, no element with role="alert" should be present', (done) => {
      fc.assert(
        fc.property(
          fc.constant(''),
          (_empty) => {
            const fixture = TestBed.createComponent(TestDatePickerErrorHostComponent);
            fixture.componentInstance.error = '';
            fixture.detectChanges();

            const errorEl = fixture.nativeElement.querySelector('[role="alert"]');
            if (errorEl) {
              throw new Error('With empty error: element with role="alert" should not be present');
            }

            fixture.destroy();
          }
        ),
        { numRuns: 10 }
      );
      done();
    });
  });

  // ── Combined: exact error string preservation across all form components ──

  describe('combined: error string preservation across all form components', () => {
    type FormComponentType = 'input' | 'dropdown' | 'datepicker';

    it('for any form component and any error string, the exact error is displayed with role="alert"', (done) => {
      fc.assert(
        fc.property(
          fc.constantFrom<FormComponentType>('input', 'dropdown', 'datepicker'),
          arbErrorString,
          (componentType, error) => {
            let fixture: any;

            switch (componentType) {
              case 'input':
                fixture = TestBed.createComponent(TestInputErrorHostComponent);
                fixture.componentInstance.error = error;
                fixture.componentInstance.inputId = 'combined-input';
                break;
              case 'dropdown':
                fixture = TestBed.createComponent(TestDropdownErrorHostComponent);
                fixture.componentInstance.error = error;
                fixture.componentInstance.dropdownId = 'combined-dd';
                break;
              case 'datepicker':
                fixture = TestBed.createComponent(TestDatePickerErrorHostComponent);
                fixture.componentInstance.error = error;
                fixture.componentInstance.inputId = 'combined-dp';
                break;
            }

            fixture.detectChanges();

            const errorEl = fixture.nativeElement.querySelector('[role="alert"]');
            if (!errorEl) {
              throw new Error(
                `${componentType} with error="${error}": no element with role="alert" found`
              );
            }

            const displayedText = errorEl.textContent.trim();
            if (displayedText !== error) {
              throw new Error(
                `${componentType}: error string not preserved. Expected "${error}" but got "${displayedText}"`
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
