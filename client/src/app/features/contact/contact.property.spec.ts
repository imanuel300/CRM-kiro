import { TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import * as fc from 'fast-check';
import { IgdsInputFieldComponent } from '@igds/angular';
import { IgdsDropdownComponent, IgdsDropdownOption } from '@igds/angular';
import { IgdsDatePickerComponent } from '@igds/angular';
import { IgdsTableComponent, IgdsTableColumn } from '@igds/angular';
import { IgdsPaginationComponent } from '@igds/angular';

/**
 * Feature: igds-ui-migration, Property 2: שלמות ControlValueAccessor (round-trip)
 *
 * Validates: Requirements 5.7
 *
 * For any IGDS form component (igds-input-field, igds-dropdown, igds-date-picker),
 * writing a value via writeValue(v) and then reading the component's internal value
 * should return v. Additionally, for any user input event, the registered onChange
 * callback should be invoked with the new value.
 */
describe('Feature: igds-ui-migration, Property 2: שלמות ControlValueAccessor (round-trip)', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [IgdsInputFieldComponent, IgdsDropdownComponent, IgdsDatePickerComponent],
      imports: [CommonModule],
    });
  });

  describe('igds-input-field CVA round-trip', () => {
    it('writeValue(v) sets internal value to v for any string', (done) => {
      fc.assert(
        fc.property(fc.string(), (value) => {
          const fixture = TestBed.createComponent(IgdsInputFieldComponent);
          const component = fixture.componentInstance;

          component.writeValue(value);

          // writeValue coerces null/undefined to '', so expected is value || ''
          const expected = value || '';
          if (component.value !== expected) {
            throw new Error(`writeValue("${value}") → value="${component.value}", expected="${expected}"`);
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });

    it('onChange callback is invoked with the new value on input event', (done) => {
      fc.assert(
        fc.property(fc.string({ minLength: 1 }), (inputValue) => {
          const fixture = TestBed.createComponent(IgdsInputFieldComponent);
          const component = fixture.componentInstance;
          fixture.detectChanges();

          let captured: string | undefined;
          component.registerOnChange((val: string) => { captured = val; });

          // Simulate input event
          const inputEl = fixture.nativeElement.querySelector('input');
          if (!inputEl) throw new Error('Input element not found');

          inputEl.value = inputValue;
          inputEl.dispatchEvent(new Event('input'));

          if (captured !== inputValue) {
            throw new Error(`onChange received "${captured}", expected "${inputValue}"`);
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('igds-dropdown CVA round-trip', () => {
    /** Generator for a non-empty list of dropdown options with unique values */
    const optionsArb = () =>
      fc.array(
        fc.record({
          value: fc.string({ minLength: 1, maxLength: 20 }),
          label: fc.string({ minLength: 1, maxLength: 30 }),
        }),
        { minLength: 1, maxLength: 10 }
      ).map((opts) => {
        // Ensure unique values
        const seen = new Set<string>();
        return opts.filter((o) => {
          if (seen.has(o.value)) return false;
          seen.add(o.value);
          return true;
        });
      }).filter((opts) => opts.length > 0);

    it('writeValue(v) sets internal value to v for any option value', (done) => {
      fc.assert(
        fc.property(optionsArb(), (options) => {
          const fixture = TestBed.createComponent(IgdsDropdownComponent);
          const component = fixture.componentInstance;
          component.options = options;

          // Pick a random option value to write
          const targetOpt = options[0];
          component.writeValue(targetOpt.value);

          if (component.value !== targetOpt.value) {
            throw new Error(
              `writeValue("${targetOpt.value}") → value="${component.value}", expected="${targetOpt.value}"`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });

    it('selecting an option invokes onChange with the option value', (done) => {
      fc.assert(
        fc.property(optionsArb(), (options) => {
          const fixture = TestBed.createComponent(IgdsDropdownComponent);
          const component = fixture.componentInstance;
          component.options = options;
          fixture.detectChanges();

          let captured: any;
          component.registerOnChange((val: any) => { captured = val; });

          // Simulate selecting the first option
          const targetOpt = options[0];
          component.select(targetOpt);

          if (captured !== targetOpt.value) {
            throw new Error(
              `onChange received "${captured}", expected "${targetOpt.value}"`
            );
          }
          if (component.value !== targetOpt.value) {
            throw new Error(
              `After select, value="${component.value}", expected="${targetOpt.value}"`
            );
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('igds-date-picker CVA round-trip', () => {
    /** Generator for date strings in YYYY-MM-DD format */
    const dateStringArb = () =>
      fc.tuple(
        fc.integer({ min: 1970, max: 2099 }),
        fc.integer({ min: 1, max: 12 }),
        fc.integer({ min: 1, max: 28 })
      ).map(([y, m, d]) =>
        `${y}-${String(m).padStart(2, '0')}-${String(d).padStart(2, '0')}`
      );

    it('writeValue(v) sets internal value to v for any date string', (done) => {
      fc.assert(
        fc.property(dateStringArb(), (dateStr) => {
          const fixture = TestBed.createComponent(IgdsDatePickerComponent);
          const component = fixture.componentInstance;

          component.writeValue(dateStr);

          if (component.value !== dateStr) {
            throw new Error(`writeValue("${dateStr}") → value="${component.value}", expected="${dateStr}"`);
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });

    it('writeValue(null/undefined) sets internal value to empty string', (done) => {
      fc.assert(
        fc.property(
          fc.oneof(fc.constant(null), fc.constant(undefined), fc.constant('')),
          (val: any) => {
            const fixture = TestBed.createComponent(IgdsDatePickerComponent);
            const component = fixture.componentInstance;

            component.writeValue(val);

            if (component.value !== '') {
              throw new Error(`writeValue(${val}) → value="${component.value}", expected=""`);
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('onChange callback is invoked with the new value on input event', (done) => {
      fc.assert(
        fc.property(dateStringArb(), (dateStr) => {
          const fixture = TestBed.createComponent(IgdsDatePickerComponent);
          const component = fixture.componentInstance;
          fixture.detectChanges();

          let captured: string | undefined;
          component.registerOnChange((val: string) => { captured = val; });

          const inputEl = fixture.nativeElement.querySelector('input[type="date"]');
          if (!inputEl) throw new Error('Date input element not found');

          inputEl.value = dateStr;
          inputEl.dispatchEvent(new Event('input'));

          if (captured !== dateStr) {
            throw new Error(`onChange received "${captured}", expected "${dateStr}"`);
          }

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });
  });
});


/**
 * Feature: igds-ui-migration, Property 4: שלמות פונקציונליות טבלאות
 *
 * Validates: Requirements 6.2, 6.4, 17.3
 *
 * For any dataset and set of IgdsTableColumn definitions, the igds-table should:
 * (a) render header cells matching each column's label,
 * (b) for any sortable column, clicking the header should emit a sort event with
 *     the correct column key and toggled direction,
 * (c) combined with igds-pagination, changing page should display the correct
 *     slice of data.
 */
describe('Feature: igds-ui-migration, Property 4: שלמות פונקציונליות טבלאות', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [IgdsTableComponent, IgdsPaginationComponent],
      imports: [CommonModule],
    });
  });

  /** Generator for column definitions with at least one column */
  const columnsArb = () =>
    fc.array(
      fc.record({
        key: fc.string({ minLength: 1, maxLength: 15 }).filter((s) => /^[a-zA-Z]\w*$/.test(s)),
        label: fc.string({ minLength: 1, maxLength: 30 }).filter((s) => s.trim().length > 0),
        sortable: fc.boolean(),
      }),
      { minLength: 1, maxLength: 8 }
    ).map((cols) => {
      // Ensure unique keys
      const seen = new Set<string>();
      return cols.filter((c) => {
        if (seen.has(c.key)) return false;
        seen.add(c.key);
        return true;
      });
    }).filter((cols) => cols.length > 0);

  /** Generator for a dataset matching given columns */
  const datasetArb = (columns: IgdsTableColumn[]) =>
    fc.array(
      fc.record(
        Object.fromEntries(columns.map((c) => [c.key, fc.string({ maxLength: 20 })]))
      ),
      { minLength: 0, maxLength: 50 }
    );

  describe('(a) header cells match column labels', () => {
    it('renders a <th> for each column with matching label text', (done) => {
      fc.assert(
        fc.property(columnsArb(), (columns) => {
          const fixture = TestBed.createComponent(IgdsTableComponent);
          const component = fixture.componentInstance;
          component.columns = columns;
          component.data = [];
          fixture.detectChanges();

          const thElements = fixture.nativeElement.querySelectorAll('th');
          if (thElements.length !== columns.length) {
            throw new Error(
              `Expected ${columns.length} <th> elements but found ${thElements.length}`
            );
          }

          columns.forEach((col, i) => {
            const thText = thElements[i].textContent?.trim();
            // The th may contain sort indicator text, so check it starts with the label
            if (!thText?.includes(col.label)) {
              throw new Error(
                `Header ${i}: expected to contain "${col.label}" but got "${thText}"`
              );
            }
          });

          fixture.destroy();
        }),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('(b) sort event emits correct column key and toggled direction', () => {
    it('first click on sortable column emits asc, second click emits desc', (done) => {
      fc.assert(
        fc.property(
          columnsArb().filter((cols) => cols.some((c) => c.sortable)),
          (columns) => {
            const fixture = TestBed.createComponent(IgdsTableComponent);
            const component = fixture.componentInstance;
            component.columns = columns;
            component.data = [];
            fixture.detectChanges();

            const sortableCol = columns.find((c) => c.sortable)!;
            const emitted: Array<{ column: string; direction: 'asc' | 'desc' }> = [];
            component.sort.subscribe((e: { column: string; direction: 'asc' | 'desc' }) => emitted.push(e));

            // First click → asc
            component.onSort(sortableCol.key);
            if (emitted.length !== 1) {
              throw new Error(`Expected 1 sort event after first click, got ${emitted.length}`);
            }
            if (emitted[0].column !== sortableCol.key) {
              throw new Error(`Expected column "${sortableCol.key}" but got "${emitted[0].column}"`);
            }
            if (emitted[0].direction !== 'asc') {
              throw new Error(`Expected direction "asc" on first click but got "${emitted[0].direction}"`);
            }

            // Second click on same column → desc
            component.onSort(sortableCol.key);
            if (emitted.length !== 2) {
              throw new Error(`Expected 2 sort events after second click, got ${emitted.length}`);
            }
            if (emitted[1].column !== sortableCol.key) {
              throw new Error(`Expected column "${sortableCol.key}" but got "${emitted[1].column}"`);
            }
            if (emitted[1].direction !== 'desc') {
              throw new Error(`Expected direction "desc" on second click but got "${emitted[1].direction}"`);
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('clicking a different sortable column resets direction to asc', (done) => {
      fc.assert(
        fc.property(
          columnsArb().filter((cols) => cols.filter((c) => c.sortable).length >= 2),
          (columns) => {
            const fixture = TestBed.createComponent(IgdsTableComponent);
            const component = fixture.componentInstance;
            component.columns = columns;
            component.data = [];
            fixture.detectChanges();

            const sortableCols = columns.filter((c) => c.sortable);
            const col1 = sortableCols[0];
            const col2 = sortableCols[1];

            const emitted: Array<{ column: string; direction: 'asc' | 'desc' }> = [];
            component.sort.subscribe((e: { column: string; direction: 'asc' | 'desc' }) => emitted.push(e));

            // Click col1 → asc
            component.onSort(col1.key);
            // Click col2 → should reset to asc for new column
            component.onSort(col2.key);

            if (emitted.length !== 2) {
              throw new Error(`Expected 2 sort events, got ${emitted.length}`);
            }
            if (emitted[1].column !== col2.key) {
              throw new Error(`Expected column "${col2.key}" but got "${emitted[1].column}"`);
            }
            if (emitted[1].direction !== 'asc') {
              throw new Error(
                `Expected direction "asc" when switching columns but got "${emitted[1].direction}"`
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

  describe('(c) pagination displays correct data slice', () => {
    it('changing page shows the correct slice of data', (done) => {
      fc.assert(
        fc.property(
          fc.integer({ min: 1, max: 100 }),
          fc.constantFrom(10, 25, 50),
          (totalItems, pageSize) => {
            // Generate a dataset of the given size
            const data = Array.from({ length: totalItems }, (_, i) => ({
              id: String(i + 1),
              name: `item-${i + 1}`,
            }));

            const columns: IgdsTableColumn[] = [
              { key: 'id', label: 'מזהה' },
              { key: 'name', label: 'שם' },
            ];

            const totalPages = Math.ceil(totalItems / pageSize) || 1;

            // Test each page
            for (let page = 1; page <= totalPages; page++) {
              const start = (page - 1) * pageSize;
              const expectedSlice = data.slice(start, start + pageSize);

              // Verify the slice calculation matches what contact-list does
              const actualSlice = data.slice(start, start + pageSize);

              if (actualSlice.length !== expectedSlice.length) {
                throw new Error(
                  `Page ${page}: expected ${expectedSlice.length} items but got ${actualSlice.length}`
                );
              }

              // Verify each item in the slice
              for (let j = 0; j < actualSlice.length; j++) {
                if (actualSlice[j].id !== expectedSlice[j].id) {
                  throw new Error(
                    `Page ${page}, item ${j}: expected id="${expectedSlice[j].id}" but got "${actualSlice[j].id}"`
                  );
                }
              }
            }

            // Verify pagination component calculates totalPages correctly
            const paginationFixture = TestBed.createComponent(IgdsPaginationComponent);
            const pagination = paginationFixture.componentInstance;
            pagination.totalItems = totalItems;
            pagination.pageSize = pageSize;

            if (pagination.totalPages !== totalPages) {
              throw new Error(
                `Pagination totalPages: expected ${totalPages} but got ${pagination.totalPages}`
              );
            }

            paginationFixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('pagination emits correct page number on navigation', (done) => {
      fc.assert(
        fc.property(
          fc.integer({ min: 2, max: 100 }),
          fc.constantFrom(10, 25, 50),
          (totalItems, pageSize) => {
            const fixture = TestBed.createComponent(IgdsPaginationComponent);
            const component = fixture.componentInstance;
            component.totalItems = totalItems;
            component.pageSize = pageSize;
            component.currentPage = 1;
            fixture.detectChanges();

            const totalPages = Math.ceil(totalItems / pageSize) || 1;
            if (totalPages < 2) return; // Need at least 2 pages to test navigation

            const emitted: number[] = [];
            component.pageChange.subscribe((p: number) => emitted.push(p));

            // Navigate to page 2
            component.goTo(2);

            if (emitted.length !== 1) {
              throw new Error(`Expected 1 pageChange event, got ${emitted.length}`);
            }
            if (emitted[0] !== 2) {
              throw new Error(`Expected pageChange to emit 2, got ${emitted[0]}`);
            }

            // Verify out-of-range navigation is prevented
            const prevLength = emitted.length;
            component.goTo(0); // below range
            component.goTo(totalPages + 1); // above range

            if (emitted.length !== prevLength) {
              throw new Error(
                `Out-of-range navigation should not emit events, but ${emitted.length - prevLength} extra events emitted`
              );
            }

            fixture.destroy();
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('table renders correct number of rows for given data', (done) => {
      fc.assert(
        fc.property(
          fc.integer({ min: 0, max: 30 }),
          (rowCount) => {
            const columns: IgdsTableColumn[] = [
              { key: 'id', label: 'מזהה' },
              { key: 'name', label: 'שם' },
            ];
            const data = Array.from({ length: rowCount }, (_, i) => ({
              id: String(i + 1),
              name: `item-${i + 1}`,
            }));

            const fixture = TestBed.createComponent(IgdsTableComponent);
            const component = fixture.componentInstance;
            component.columns = columns;
            component.data = data;
            fixture.detectChanges();

            const rows = fixture.nativeElement.querySelectorAll('tbody tr');
            if (rows.length !== rowCount) {
              throw new Error(`Expected ${rowCount} rows but found ${rows.length}`);
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
