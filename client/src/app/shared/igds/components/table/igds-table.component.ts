import { Component, Input, Output, EventEmitter } from '@angular/core';

export interface IgdsTableColumn {
  key: string;
  label: string;
  sortable?: boolean;
}

@Component({
  selector: 'igds-table',
  template: `
    <div class="igds-table-wrapper">
      <table class="igds-table" role="grid">
        <thead>
          <tr>
            <th *ngFor="let col of columns" class="igds-table__th"
              [class.igds-table__th--sortable]="col.sortable"
              [attr.aria-sort]="col.key === sortColumn ? (sortDirection === 'asc' ? 'ascending' : 'descending') : null"
              (click)="col.sortable ? onSort(col.key) : null"
              (keydown.enter)="col.sortable ? onSort(col.key) : null"
              [tabindex]="col.sortable ? 0 : -1">
              {{col.label}}
              <span *ngIf="col.sortable" class="igds-table__sort" aria-hidden="true">
                {{col.key === sortColumn ? (sortDirection === 'asc' ? '▲' : '▼') : '⇅'}}
              </span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let row of data" class="igds-table__row">
            <td *ngFor="let col of columns" class="igds-table__td">{{row[col.key]}}</td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-table-wrapper { overflow-x: auto; border: 1px solid var(--igds-border-divider); border-radius: var(--igds-radius-md); }
    .igds-table {
      width: 100%; border-collapse: collapse; font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-sm);
    }
    .igds-table__th {
      padding: var(--igds-space-12) var(--igds-space-16); text-align: inherit;
      font-weight: var(--igds-font-weight-medium); color: var(--igds-text-primary);
      background: var(--igds-bg-neutral-secondlevel); border-bottom: 2px solid var(--igds-border-divider);
      white-space: nowrap;
    }
    .igds-table__th--sortable { cursor: pointer; user-select: none; }
    .igds-table__th--sortable:hover { background: var(--igds-bg-neutral-hover); }
    .igds-table__th:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: -2px; }
    .igds-table__sort { margin-inline-start: var(--igds-space-4); font-size: var(--igds-font-size-xs); }
    .igds-table__row { transition: background var(--igds-transition-fast); }
    .igds-table__row:hover { background: var(--igds-bg-neutral-hover); }
    .igds-table__td {
      padding: var(--igds-space-12) var(--igds-space-16); color: var(--igds-text-primary);
      border-bottom: 1px solid var(--igds-border-divider);
    }
  `]
})
export class IgdsTableComponent {
  @Input() columns: IgdsTableColumn[] = [];
  @Input() data: any[] = [];
  @Input() sortColumn = '';
  @Input() sortDirection: 'asc' | 'desc' = 'asc';
  @Output() sort = new EventEmitter<{ column: string; direction: 'asc' | 'desc' }>();

  onSort(key: string) {
    if (this.sortColumn === key) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = key;
      this.sortDirection = 'asc';
    }
    this.sort.emit({ column: this.sortColumn, direction: this.sortDirection });
  }
}
