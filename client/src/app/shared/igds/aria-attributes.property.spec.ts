import { TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import * as fc from 'fast-check';

import { IgdsButtonComponent } from '@igds/angular';
import { IgdsInputFieldComponent } from '@igds/angular';
import { IgdsDropdownComponent, IgdsDropdownOption } from '@igds/angular';
import { IgdsModalComponent } from '@igds/angular';
import { IgdsTableComponent, IgdsTableColumn } from '@igds/angular';
import { IgdsTabsComponent, IgdsTab } from '@igds/angular';
import { IgdsAccordionComponent } from '@igds/angular';
import { IgdsCheckboxComponent } from '@igds/angular';
import { IgdsRadioButtonComponent } from '@igds/angular';
import { IgdsToggleComponent } from '@igds/angular';
import { IgdsPaginationComponent } from '@igds/angular';

/**
 * Feature: igds-ui-migration, Property 11: תכונות ARIA ברכיבים אינטראקטיביים
 *
 * Validates: Requirements 11.2, 11.5
 *
 * For any interactive IGDS component (buttons, inputs, dropdowns, modals, tables, tabs),
 * the rendered HTML should include appropriate ARIA attributes: role where semantic HTML
 * is insufficient, aria-label or aria-labelledby for non-text elements, aria-expanded for
 * expandable elements, aria-selected for selectable items, and aria-invalid/aria-describedby
 * for form elements with errors.
 */

// ─── Test host components ───────────────────────────────────────────────────────

@Component({
  selector: 'test-button-host',
  template: `
    <igds-button [variant]="variant" [disabled]="disabled" [ariaLabel]="ariaLabel">
      {{text}}
    </igds-button>
  `,
})
class TestButtonHostComponent {
  @Input() variant: 'primary' | 'secondary' | 'link' = 'primary';
  @Input() disabled = false;
  @Input() ariaLabel = '';
  @Input() text = 'לחצן';
}

@Component({
  selector: 'test-input-host',
  template: `
    <igds-input-field
      [label]="label"
      [error]="error"
      [disabled]="disabled"
      [required]="required"
      [inputId]="inputId">
    </igds-input-field>
  `,
})
class TestInputHostComponent {
  @Input() label = '';
  @Input() error = '';
  @Input() disabled = false;
  @Input() required = false;
  @Input() inputId = 'test-input';
}

@Component({
  selector: 'test-dropdown-host',
  template: `
    <igds-dropdown
      [label]="label"
      [options]="options"
      [error]="error"
      [disabled]="disabled"
      [dropdownId]="dropdownId">
    </igds-dropdown>
  `,
})
class TestDropdownHostComponent {
  @Input() label = '';
  @Input() options: IgdsDropdownOption[] = [];
  @Input() error = '';
  @Input() disabled = false;
  @Input() dropdownId = 'test-dropdown';
}

@Component({
  selector: 'test-modal-host',
  template: `
    <igds-modal [title]="title" [visible]="visible" [closable]="closable">
      <p>תוכן מודאל</p>
    </igds-modal>
  `,
})
class TestModalHostComponent {
  @Input() title = '';
  @Input() visible = false;
  @Input() closable = true;
}

@Component({
  selector: 'test-table-host',
  template: `
    <igds-table [columns]="columns" [data]="data"
      [sortColumn]="sortColumn" [sortDirection]="sortDirection">
    </igds-table>
  `,
})
class TestTableHostComponent {
  @Input() columns: IgdsTableColumn[] = [];
  @Input() data: any[] = [];
  @Input() sortColumn = '';
  @Input() sortDirection: 'asc' | 'desc' = 'asc';
}

@Component({
  selector: 'test-tabs-host',
  template: `
    <igds-tabs [tabs]="tabs" [activeTab]="activeTab"></igds-tabs>
  `,
})
class TestTabsHostComponent {
  @Input() tabs: IgdsTab[] = [];
  @Input() activeTab = '';
}

@Component({
  selector: 'test-accordion-host',
  template: `
    <igds-accordion [title]="title" [expanded]="expanded" [disabled]="disabled">
      <p>תוכן אקורדיון</p>
    </igds-accordion>
  `,
})
class TestAccordionHostComponent {
  @Input() title = '';
  @Input() expanded = false;
  @Input() disabled = false;
}

@Component({
  selector: 'test-checkbox-host',
  template: `
    <igds-checkbox [label]="label" [checked]="checked" [disabled]="disabled">
    </igds-checkbox>
  `,
})
class TestCheckboxHostComponent {
  @Input() label = '';
  @Input() checked = false;
  @Input() disabled = false;
}

@Component({
  selector: 'test-toggle-host',
  template: `
    <igds-toggle [label]="label" [checked]="checked" [disabled]="disabled">
    </igds-toggle>
  `,
})
class TestToggleHostComponent {
  @Input() label = '';
  @Input() checked = false;
  @Input() disabled = false;
}

@Component({
  selector: 'test-pagination-host',
  template: `
    <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize" [currentPage]="currentPage">
    </igds-pagination>
  `,
})
class TestPaginationHostComponent {
  @Input() totalItems = 100;
  @Input() pageSize = 10;
  @Input() currentPage = 1;
}

// ─── Generators ─────────────────────────────────────────────────────────────────

/** Generates a non-empty Hebrew-friendly label string */
const arbLabel = fc.stringOf(
  fc.constantFrom(...'אבגדהוזחטיכלמנסעפצקרשת abcdefghijklmnopqrstuvwxyz0123456789'.split('')),
  { minLength: 1, maxLength: 30 }
);

/** Generates a dropdown option */
const arbDropdownOption: fc.Arbitrary<IgdsDropdownOption> = fc.record({
  value: fc.oneof(fc.string({ minLength: 1, maxLength: 10 }), fc.integer({ min: 1, max: 100 })),
  label: arbLabel,
});

/** Generates a non-empty list of dropdown options */
const arbDropdownOptions = fc.array(arbDropdownOption, { minLength: 1, maxLength: 10 });

/** Generates a table column definition */
const arbTableColumn: fc.Arbitrary<IgdsTableColumn> = fc.record({
  key: fc.string({ minLength: 1, maxLength: 15 }).filter((s: string) => /^[a-zA-Z]/.test(s)),
  label: arbLabel,
  sortable: fc.boolean(),
});

/** Generates a non-empty list of table columns */
const arbTableColumns = fc.array(arbTableColumn, { minLength: 1, maxLength: 6 });

/** Generates a tab definition */
const arbTab: fc.Arbitrary<IgdsTab> = fc.record({
  label: arbLabel,
  id: fc.string({ minLength: 1, maxLength: 10 }).filter((s: string) => /^[a-zA-Z]/.test(s)),
});

/** Generates a non-empty list of tabs with unique ids */
const arbTabs: fc.Arbitrary<IgdsTab[]> = fc.array(arbTab, { minLength: 1, maxLength: 6 }).map((tabs: IgdsTab[]) => {
  const seen = new Set<string>();
  return tabs.filter((t: IgdsTab) => {
    if (seen.has(t.id)) return false;
    seen.add(t.id);
    return true;
  });
}).filter((tabs: IgdsTab[]) => tabs.length > 0);

// ─── Test suite ─────────────────────────────────────────────────────────────────

describe('Feature: igds-ui-migration, Property 11: תכונות ARIA ברכיבים אינטראקטיביים', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [
        IgdsButtonComponent,
        IgdsInputFieldComponent,
        IgdsDropdownComponent,
        IgdsModalComponent,
        IgdsTableComponent,
        IgdsTabsComponent,
        IgdsAccordionComponent,
        IgdsCheckboxComponent,
        IgdsRadioButtonComponent,
        IgdsToggleComponent,
        IgdsPaginationComponent,
        TestButtonHostComponent,
        TestInputHostComponent,
        TestDropdownHostComponent,
        TestModalHostComponent,
        TestTableHostComponent,
        TestTabsHostComponent,
        TestAccordionHostComponent,
        TestCheckboxHostComponent,
        TestToggleHostComponent,
        TestPaginationHostComponent,
      ],
      imports: [CommonModule, FormsModule, ReactiveFormsModule],
    });
  });

  // ── Button: aria-label preserved ──────────────────────────────────────────

  describe('igds-button: aria-label on buttons', () => {
    it('for any aria-label string, the rendered button preserves it', (done) => {
      fc.assert(
        fc.property(
          arbLabel,
          fc.constantFrom<'primary' | 'secondary' | 'link'>('primary', 'secondary', 'link'),
          fc.boolean(),
          (ariaLabel, variant, disabled) => {
            const fixture = TestBed.createComponent(TestButtonHostComponent);
            fixture.componentInstance.ariaLabel = ariaLabel;
            fixture.componentInstance.variant = variant;
            fixture.componentInstance.disabled = disabled;
            fixture.detectChanges();

            const btn = fixture.nativeElement.querySelector('button.igds-btn');
            if (!btn) throw new Error('Button element not found');

            const actual = btn.getAttribute('aria-label');
            if (actual !== ariaLabel) {
              throw new Error(
                `Expected aria-label="${ariaLabel}" but got "${actual}"`
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

  // ── Input field: aria-invalid and aria-describedby for errors ──────────────

  describe('igds-input-field: aria-invalid and aria-describedby with errors', () => {
    it('for any error string, input has aria-invalid=true and aria-describedby pointing to error element', (done) => {
      fc.assert(
        fc.property(
          arbLabel,
          arbLabel,
          (label, error) => {
            const fixture = TestBed.createComponent(TestInputHostComponent);
            fixture.componentInstance.label = label;
            fixture.componentInstance.error = error;
            fixture.componentInstance.inputId = 'test-input-err';
            fixture.detectChanges();

            const input = fixture.nativeElement.querySelector('input.igds-input__field');
            if (!input) throw new Error('Input element not found');

            // aria-invalid should be "true" when error is present
            const ariaInvalid = input.getAttribute('aria-invalid');
            if (ariaInvalid !== 'true') {
              throw new Error(
                `With error="${error}": expected aria-invalid="true" but got "${ariaInvalid}"`
              );
            }

            // aria-describedby should reference the error element
            const describedBy = input.getAttribute('aria-describedby');
            if (describedBy !== 'test-input-err-error') {
              throw new Error(
                `With error="${error}": expected aria-describedby="test-input-err-error" but got "${describedBy}"`
              );
            }

            // The error element should exist with role="alert"
            const errorEl = fixture.nativeElement.querySelector('#test-input-err-error');
            if (!errorEl) throw new Error('Error element not found');
            if (errorEl.getAttribute('role') !== 'alert') {
              throw new Error('Error element missing role="alert"');
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('for any input without error, aria-invalid is absent', (done) => {
      fc.assert(
        fc.property(
          arbLabel,
          (label) => {
            const fixture = TestBed.createComponent(TestInputHostComponent);
            fixture.componentInstance.label = label;
            fixture.componentInstance.error = '';
            fixture.detectChanges();

            const input = fixture.nativeElement.querySelector('input.igds-input__field');
            if (!input) throw new Error('Input element not found');

            const ariaInvalid = input.getAttribute('aria-invalid');
            if (ariaInvalid !== null) {
              throw new Error(
                `Without error: expected no aria-invalid but got "${ariaInvalid}"`
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

  // ── Dropdown: role, aria-expanded, aria-selected, aria-invalid ────────────

  describe('igds-dropdown: ARIA attributes for expandable/selectable', () => {
    it('for any dropdown state, trigger has role=combobox and correct aria-expanded', (done) => {
      fc.assert(
        fc.property(
          arbLabel,
          arbDropdownOptions,
          fc.boolean(),
          (label, options, hasError) => {
            const fixture = TestBed.createComponent(TestDropdownHostComponent);
            fixture.componentInstance.label = label;
            fixture.componentInstance.options = options;
            fixture.componentInstance.error = hasError ? 'שגיאה' : '';
            fixture.componentInstance.dropdownId = 'test-dd';
            fixture.detectChanges();

            const trigger = fixture.nativeElement.querySelector('[role="combobox"]');
            if (!trigger) throw new Error('Dropdown trigger with role="combobox" not found');

            // aria-expanded should be "false" initially (closed)
            if (trigger.getAttribute('aria-expanded') !== 'false') {
              throw new Error(
                `Expected aria-expanded="false" (closed) but got "${trigger.getAttribute('aria-expanded')}"`
              );
            }

            // aria-haspopup should be "listbox"
            if (trigger.getAttribute('aria-haspopup') !== 'listbox') {
              throw new Error(
                `Expected aria-haspopup="listbox" but got "${trigger.getAttribute('aria-haspopup')}"`
              );
            }

            // When error is present, aria-invalid should be "true"
            if (hasError) {
              if (trigger.getAttribute('aria-invalid') !== 'true') {
                throw new Error('With error: expected aria-invalid="true"');
              }
              if (trigger.getAttribute('aria-describedby') !== 'test-dd-error') {
                throw new Error('With error: expected aria-describedby pointing to error element');
              }
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('when dropdown is opened, listbox role and aria-selected on options are present', (done) => {
      fc.assert(
        fc.property(
          arbDropdownOptions,
          (options) => {
            const fixture = TestBed.createComponent(TestDropdownHostComponent);
            fixture.componentInstance.options = options;
            fixture.componentInstance.dropdownId = 'test-dd-open';
            fixture.detectChanges();

            // Open the dropdown by clicking the trigger
            const trigger = fixture.nativeElement.querySelector('[role="combobox"]');
            trigger.click();
            fixture.detectChanges();

            // aria-expanded should now be "true"
            if (trigger.getAttribute('aria-expanded') !== 'true') {
              throw new Error(
                `After open: expected aria-expanded="true" but got "${trigger.getAttribute('aria-expanded')}"`
              );
            }

            // Listbox should be present
            const listbox = fixture.nativeElement.querySelector('[role="listbox"]');
            if (!listbox) throw new Error('Listbox element not found after opening dropdown');

            // Each option should have role="option" and aria-selected
            const optionEls = fixture.nativeElement.querySelectorAll('[role="option"]');
            if (optionEls.length !== options.length) {
              throw new Error(
                `Expected ${options.length} options with role="option", found ${optionEls.length}`
              );
            }

            for (let i = 0; i < optionEls.length; i++) {
              const ariaSelected = optionEls[i].getAttribute('aria-selected');
              if (ariaSelected !== 'true' && ariaSelected !== 'false') {
                throw new Error(
                  `Option ${i}: expected aria-selected to be "true" or "false", got "${ariaSelected}"`
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

  // ── Modal: role=dialog, aria-modal, aria-label ────────────────────────────

  describe('igds-modal: dialog ARIA attributes', () => {
    it('for any visible modal with title, has role=dialog, aria-modal=true, and aria-label', (done) => {
      fc.assert(
        fc.property(
          arbLabel,
          fc.boolean(),
          (title, closable) => {
            const fixture = TestBed.createComponent(TestModalHostComponent);
            fixture.componentInstance.title = title;
            fixture.componentInstance.visible = true;
            fixture.componentInstance.closable = closable;
            fixture.detectChanges();

            const dialog = fixture.nativeElement.querySelector('[role="dialog"]');
            if (!dialog) throw new Error('Dialog element with role="dialog" not found');

            if (dialog.getAttribute('aria-modal') !== 'true') {
              throw new Error(
                `Expected aria-modal="true" but got "${dialog.getAttribute('aria-modal')}"`
              );
            }

            const ariaLabel = dialog.getAttribute('aria-label');
            if (ariaLabel !== title) {
              throw new Error(
                `Expected aria-label="${title}" but got "${ariaLabel}"`
              );
            }

            // Close button should have aria-label
            if (closable) {
              const closeBtn = fixture.nativeElement.querySelector('.igds-modal__close');
              if (closeBtn) {
                const closeBtnLabel = closeBtn.getAttribute('aria-label');
                if (!closeBtnLabel) {
                  throw new Error('Close button missing aria-label');
                }
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

  // ── Table: role=grid, aria-sort on sorted columns ─────────────────────────

  describe('igds-table: grid role and aria-sort', () => {
    it('for any table with sortable columns, sorted column has correct aria-sort', (done) => {
      fc.assert(
        fc.property(
          arbTableColumns.filter((cols: IgdsTableColumn[]) => cols.some((c: IgdsTableColumn) => c.sortable)),
          fc.constantFrom<'asc' | 'desc'>('asc', 'desc'),
          (columns, direction) => {
            const sortableCol = columns.find(c => c.sortable)!;
            const fixture = TestBed.createComponent(TestTableHostComponent);
            fixture.componentInstance.columns = columns;
            fixture.componentInstance.data = [];
            fixture.componentInstance.sortColumn = sortableCol.key;
            fixture.componentInstance.sortDirection = direction;
            fixture.detectChanges();

            const table = fixture.nativeElement.querySelector('table[role="grid"]');
            if (!table) throw new Error('Table with role="grid" not found');

            // Find the sorted column header
            const ths = fixture.nativeElement.querySelectorAll('th');
            let foundSorted = false;
            for (let i = 0; i < ths.length; i++) {
              const ariaSort = ths[i].getAttribute('aria-sort');
              if (columns[i].key === sortableCol.key) {
                const expected = direction === 'asc' ? 'ascending' : 'descending';
                if (ariaSort !== expected) {
                  throw new Error(
                    `Sorted column "${sortableCol.key}": expected aria-sort="${expected}" but got "${ariaSort}"`
                  );
                }
                foundSorted = true;
              } else {
                // Non-sorted columns should not have aria-sort
                if (ariaSort !== null) {
                  throw new Error(
                    `Non-sorted column "${columns[i].key}": expected no aria-sort but got "${ariaSort}"`
                  );
                }
              }
            }

            if (!foundSorted) throw new Error('Sorted column header not found');

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  // ── Tabs: role=tablist, role=tab, aria-selected ───────────────────────────

  describe('igds-tabs: tablist and tab ARIA attributes', () => {
    it('for any set of tabs with an active tab, correct ARIA roles and aria-selected', (done) => {
      fc.assert(
        fc.property(
          arbTabs,
          (tabs) => {
            const activeTab = tabs[0].id;
            const fixture = TestBed.createComponent(TestTabsHostComponent);
            fixture.componentInstance.tabs = tabs;
            fixture.componentInstance.activeTab = activeTab;
            fixture.detectChanges();

            const tablist = fixture.nativeElement.querySelector('[role="tablist"]');
            if (!tablist) throw new Error('Element with role="tablist" not found');

            const tabEls = fixture.nativeElement.querySelectorAll('[role="tab"]');
            if (tabEls.length !== tabs.length) {
              throw new Error(
                `Expected ${tabs.length} tab elements, found ${tabEls.length}`
              );
            }

            for (let i = 0; i < tabEls.length; i++) {
              const isActive = tabs[i].id === activeTab;
              const ariaSelected = tabEls[i].getAttribute('aria-selected');
              const expected = isActive ? 'true' : 'false';
              if (ariaSelected !== expected) {
                throw new Error(
                  `Tab "${tabs[i].id}": expected aria-selected="${expected}" but got "${ariaSelected}"`
                );
              }

              // Active tab should have tabindex=0, others tabindex=-1
              const tabindex = tabEls[i].getAttribute('tabindex');
              const expectedTabindex = isActive ? '0' : '-1';
              if (tabindex !== expectedTabindex) {
                throw new Error(
                  `Tab "${tabs[i].id}": expected tabindex="${expectedTabindex}" but got "${tabindex}"`
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

  // ── Accordion: aria-expanded and aria-controls ────────────────────────────

  describe('igds-accordion: aria-expanded on expandable element', () => {
    it('for any expanded/collapsed state, header button has correct aria-expanded', (done) => {
      fc.assert(
        fc.property(
          arbLabel,
          fc.boolean(),
          fc.boolean(),
          (title, expanded, disabled) => {
            const fixture = TestBed.createComponent(TestAccordionHostComponent);
            fixture.componentInstance.title = title;
            fixture.componentInstance.expanded = expanded;
            fixture.componentInstance.disabled = disabled;
            fixture.detectChanges();

            const headerBtn = fixture.nativeElement.querySelector('.igds-accordion__header');
            if (!headerBtn) throw new Error('Accordion header button not found');

            const ariaExpanded = headerBtn.getAttribute('aria-expanded');
            const expected = expanded ? 'true' : 'false';
            if (ariaExpanded !== expected) {
              throw new Error(
                `expanded=${expanded}: expected aria-expanded="${expected}" but got "${ariaExpanded}"`
              );
            }

            // aria-controls should reference the panel id
            const ariaControls = headerBtn.getAttribute('aria-controls');
            if (!ariaControls) {
              throw new Error('Accordion header missing aria-controls');
            }

            // When expanded, the panel with that id should exist with role="region"
            if (expanded) {
              const panel = fixture.nativeElement.querySelector(`#${ariaControls}`);
              if (!panel) throw new Error(`Panel with id="${ariaControls}" not found when expanded`);
              if (panel.getAttribute('role') !== 'region') {
                throw new Error('Expanded panel missing role="region"');
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

  // ── Checkbox: aria-checked ────────────────────────────────────────────────

  describe('igds-checkbox: aria-checked attribute', () => {
    it('for any checked state, the checkbox input has correct aria-checked', (done) => {
      fc.assert(
        fc.property(
          arbLabel,
          fc.boolean(),
          fc.boolean(),
          (label, checked, disabled) => {
            const fixture = TestBed.createComponent(TestCheckboxHostComponent);
            fixture.componentInstance.label = label;
            fixture.componentInstance.checked = checked;
            fixture.componentInstance.disabled = disabled;
            fixture.detectChanges();

            const input = fixture.nativeElement.querySelector('input[type="checkbox"]');
            if (!input) throw new Error('Checkbox input not found');

            const ariaChecked = input.getAttribute('aria-checked');
            const expected = checked ? 'true' : 'false';
            if (ariaChecked !== expected) {
              throw new Error(
                `checked=${checked}: expected aria-checked="${expected}" but got "${ariaChecked}"`
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

  // ── Toggle: role=switch and aria-checked ──────────────────────────────────

  describe('igds-toggle: role=switch and aria-checked', () => {
    it('for any toggle state, input has role=switch and correct aria-checked', (done) => {
      fc.assert(
        fc.property(
          arbLabel,
          fc.boolean(),
          fc.boolean(),
          (label, checked, disabled) => {
            const fixture = TestBed.createComponent(TestToggleHostComponent);
            fixture.componentInstance.label = label;
            fixture.componentInstance.checked = checked;
            fixture.componentInstance.disabled = disabled;
            fixture.detectChanges();

            const input = fixture.nativeElement.querySelector('input[role="switch"]');
            if (!input) throw new Error('Toggle input with role="switch" not found');

            const ariaChecked = input.getAttribute('aria-checked');
            const expected = checked ? 'true' : 'false';
            if (ariaChecked !== expected) {
              throw new Error(
                `checked=${checked}: expected aria-checked="${expected}" but got "${ariaChecked}"`
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

  // ── Pagination: role=navigation, aria-label, aria-current ─────────────────

  describe('igds-pagination: navigation ARIA attributes', () => {
    it('for any pagination state, nav has role=navigation and aria-label', (done) => {
      fc.assert(
        fc.property(
          fc.integer({ min: 1, max: 500 }),
          fc.constantFrom(10, 25, 50),
          (totalItems, pageSize) => {
            const fixture = TestBed.createComponent(TestPaginationHostComponent);
            fixture.componentInstance.totalItems = totalItems;
            fixture.componentInstance.pageSize = pageSize;
            fixture.componentInstance.currentPage = 1;
            fixture.detectChanges();

            const nav = fixture.nativeElement.querySelector('nav[role="navigation"]');
            if (!nav) throw new Error('Pagination nav with role="navigation" not found');

            const ariaLabel = nav.getAttribute('aria-label');
            if (!ariaLabel) {
              throw new Error('Pagination nav missing aria-label');
            }

            // Current page button should have aria-current="page"
            const activeBtn = nav.querySelector('[aria-current="page"]');
            if (!activeBtn) {
              throw new Error('No button with aria-current="page" found for current page');
            }

            // Previous/next buttons should have aria-label
            const allBtns = nav.querySelectorAll('button');
            const prevBtn = allBtns[0];
            const nextBtn = allBtns[allBtns.length - 1];
            if (!prevBtn.getAttribute('aria-label')) {
              throw new Error('Previous page button missing aria-label');
            }
            if (!nextBtn.getAttribute('aria-label')) {
              throw new Error('Next page button missing aria-label');
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  // ── Combined: random component with random state passes ARIA checks ───────

  describe('combined: random interactive component with random state has correct ARIA', () => {
    type ComponentType = 'button' | 'input' | 'dropdown' | 'modal' | 'table' | 'tabs' | 'accordion' | 'checkbox' | 'toggle' | 'pagination';

    it('for any randomly selected interactive component type and state, ARIA attributes are correct', (done) => {
      fc.assert(
        fc.property(
          fc.constantFrom<ComponentType>('button', 'input', 'dropdown', 'modal', 'table', 'tabs', 'accordion', 'checkbox', 'toggle', 'pagination'),
          fc.boolean(),
          (componentType, stateFlag) => {
            switch (componentType) {
              case 'button': {
                const fixture = TestBed.createComponent(TestButtonHostComponent);
                fixture.componentInstance.ariaLabel = 'בדיקה';
                fixture.componentInstance.disabled = stateFlag;
                fixture.detectChanges();
                const btn = fixture.nativeElement.querySelector('button.igds-btn');
                if (!btn) throw new Error('Button not found');
                if (btn.getAttribute('aria-label') !== 'בדיקה') throw new Error('Button aria-label incorrect');
                fixture.destroy();
                break;
              }
              case 'input': {
                const fixture = TestBed.createComponent(TestInputHostComponent);
                fixture.componentInstance.error = stateFlag ? 'שגיאה' : '';
                fixture.componentInstance.inputId = 'combined-input';
                fixture.detectChanges();
                const input = fixture.nativeElement.querySelector('input');
                if (!input) throw new Error('Input not found');
                if (stateFlag && input.getAttribute('aria-invalid') !== 'true') throw new Error('Input missing aria-invalid with error');
                if (!stateFlag && input.getAttribute('aria-invalid') !== null) throw new Error('Input has aria-invalid without error');
                fixture.destroy();
                break;
              }
              case 'dropdown': {
                const fixture = TestBed.createComponent(TestDropdownHostComponent);
                fixture.componentInstance.options = [{ value: '1', label: 'אפשרות' }];
                fixture.componentInstance.error = stateFlag ? 'שגיאה' : '';
                fixture.componentInstance.dropdownId = 'combined-dd';
                fixture.detectChanges();
                const trigger = fixture.nativeElement.querySelector('[role="combobox"]');
                if (!trigger) throw new Error('Dropdown combobox not found');
                if (trigger.getAttribute('aria-expanded') !== 'false') throw new Error('Dropdown aria-expanded incorrect');
                fixture.destroy();
                break;
              }
              case 'modal': {
                const fixture = TestBed.createComponent(TestModalHostComponent);
                fixture.componentInstance.title = 'כותרת';
                fixture.componentInstance.visible = true;
                fixture.detectChanges();
                const dialog = fixture.nativeElement.querySelector('[role="dialog"]');
                if (!dialog) throw new Error('Dialog not found');
                if (dialog.getAttribute('aria-modal') !== 'true') throw new Error('Dialog missing aria-modal');
                fixture.destroy();
                break;
              }
              case 'table': {
                const fixture = TestBed.createComponent(TestTableHostComponent);
                fixture.componentInstance.columns = [{ key: 'a', label: 'עמודה', sortable: true }];
                fixture.componentInstance.sortColumn = stateFlag ? 'a' : '';
                fixture.componentInstance.sortDirection = 'asc';
                fixture.detectChanges();
                const table = fixture.nativeElement.querySelector('table[role="grid"]');
                if (!table) throw new Error('Table grid not found');
                fixture.destroy();
                break;
              }
              case 'tabs': {
                const tabs = [{ id: 't1', label: 'טאב1' }, { id: 't2', label: 'טאב2' }];
                const fixture = TestBed.createComponent(TestTabsHostComponent);
                fixture.componentInstance.tabs = tabs;
                fixture.componentInstance.activeTab = stateFlag ? 't1' : 't2';
                fixture.detectChanges();
                const tablist = fixture.nativeElement.querySelector('[role="tablist"]');
                if (!tablist) throw new Error('Tablist not found');
                fixture.destroy();
                break;
              }
              case 'accordion': {
                const fixture = TestBed.createComponent(TestAccordionHostComponent);
                fixture.componentInstance.title = 'כותרת';
                fixture.componentInstance.expanded = stateFlag;
                fixture.detectChanges();
                const header = fixture.nativeElement.querySelector('.igds-accordion__header');
                if (!header) throw new Error('Accordion header not found');
                const expected = stateFlag ? 'true' : 'false';
                if (header.getAttribute('aria-expanded') !== expected) throw new Error('Accordion aria-expanded incorrect');
                fixture.destroy();
                break;
              }
              case 'checkbox': {
                const fixture = TestBed.createComponent(TestCheckboxHostComponent);
                fixture.componentInstance.checked = stateFlag;
                fixture.detectChanges();
                const input = fixture.nativeElement.querySelector('input[type="checkbox"]');
                if (!input) throw new Error('Checkbox not found');
                const expected = stateFlag ? 'true' : 'false';
                if (input.getAttribute('aria-checked') !== expected) throw new Error('Checkbox aria-checked incorrect');
                fixture.destroy();
                break;
              }
              case 'toggle': {
                const fixture = TestBed.createComponent(TestToggleHostComponent);
                fixture.componentInstance.checked = stateFlag;
                fixture.detectChanges();
                const input = fixture.nativeElement.querySelector('input[role="switch"]');
                if (!input) throw new Error('Toggle switch not found');
                const expected = stateFlag ? 'true' : 'false';
                if (input.getAttribute('aria-checked') !== expected) throw new Error('Toggle aria-checked incorrect');
                fixture.destroy();
                break;
              }
              case 'pagination': {
                const fixture = TestBed.createComponent(TestPaginationHostComponent);
                fixture.componentInstance.totalItems = 100;
                fixture.componentInstance.pageSize = 10;
                fixture.detectChanges();
                const nav = fixture.nativeElement.querySelector('nav[role="navigation"]');
                if (!nav) throw new Error('Pagination nav not found');
                fixture.destroy();
                break;
              }
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });
});
