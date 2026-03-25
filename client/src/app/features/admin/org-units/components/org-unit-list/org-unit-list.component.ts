import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OrgUnit } from '../../models/org-unit.models';
import { OrgUnitService } from '../../services/org-unit.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-org-unit-list',
  template: `
    <div class="page-header">
      <h1>ניהול יחידות ארגוניות</h1>
      <igds-button variant="primary" (onClick)="onCreate()">
        יחידה חדשה
      </igds-button>
    </div>

    <igds-card>
      <igds-search-field
        placeholder="חיפוש לפי שם..."
        [value]="filterValue"
        (search)="applyFilter($event)">
      </igds-search-field>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו יחידות ארגוניות
      </div>

      <igds-table
        *ngIf="!loading && displayData.length > 0"
        [columns]="columns"
        [data]="displayData"
        [sortColumn]="sortColumn"
        [sortDirection]="sortDirection"
        (sort)="onSort($event)">
      </igds-table>

      <div *ngIf="!loading && displayData.length > 0" class="actions-column">
        <div *ngFor="let row of displayData" class="actions-row">
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="עריכה" (onClick)="onEdit(row)">
            <span igds-icon>✏️</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="תהליך מיון" (onClick)="onWorkflow(row)">
            <span igds-icon>🔀</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="סטטוסים" (onClick)="onStatuses(row)">
            <span igds-icon>🏷️</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="מחיקה" (onClick)="onDelete(row)">
            <span igds-icon>🗑</span>
          </igds-button>
        </div>
      </div>

      <igds-pagination
        *ngIf="!loading && totalItems > 0"
        [totalItems]="totalItems"
        [pageSize]="pageSize"
        [currentPage]="currentPage"
        (pageChange)="onPageChange($event)">
      </igds-pagination>
    </igds-card>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-block-end: var(--igds-space-16);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .actions-column { display: none; }
  `],
})
export class OrgUnitListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'name', label: 'שם', sortable: true },
    { key: 'description', label: 'תיאור', sortable: true },
    { key: 'contactEmail', label: 'דוא"ל' },
    { key: 'isActiveLabel', label: 'סטטוס', sortable: true },
  ];

  allData: OrgUnit[] = [];
  displayData: any[] = [];
  loading = false;
  filterValue = '';

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private orgUnitService: OrgUnitService,
    private router: Router,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.orgUnitService.getAll().subscribe({
      next: (data: OrgUnit[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applyFilter(this.filterValue);
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת יחידות ארגוניות');
        this.loading = false;
      },
    });
  }

  applyFilter(value: string): void {
    this.filterValue = value;
    const term = value.trim().toLowerCase();
    const filtered = term
      ? this.allData.filter(u => u.name.toLowerCase().includes(term))
      : [...this.allData];
    this.totalItems = filtered.length;
    this.currentPage = 1;
    this.applySort(filtered);
  }

  onSort(event: { column: string; direction: 'asc' | 'desc' }): void {
    this.sortColumn = event.column;
    this.sortDirection = event.direction;
    this.applySort();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyPagination();
  }

  onCreate(): void {
    this.router.navigate(['/admin/org-units/new']);
  }

  onEdit(row: any): void {
    this.router.navigate(['/admin/org-units', row.id, 'edit']);
  }

  onWorkflow(row: any): void {
    this.router.navigate(['/admin/org-units', row.id, 'workflow']);
  }

  onStatuses(row: any): void {
    this.router.navigate(['/admin/org-units', row.id, 'statuses']);
  }

  onDelete(row: any): void {
    const unit = this.allData.find(u => u.id === row.id);
    if (!unit) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת יחידה ארגונית',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת יחידה ארגונית',
        message: `האם למחוק את היחידה "${unit.name}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.orgUnitService.delete(unit.id).subscribe({
          next: () => {
            this.notification.success('היחידה נמחקה בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת היחידה'),
        });
      }
    });
  }

  private applySort(data?: OrgUnit[]): void {
    const source = data || this.getFilteredData();
    if (this.sortColumn) {
      const dir = this.sortDirection === 'asc' ? 1 : -1;
      source.sort((a: any, b: any) => {
        const valA = (a[this.sortColumn] ?? '').toString().toLowerCase();
        const valB = (b[this.sortColumn] ?? '').toString().toLowerCase();
        return valA.localeCompare(valB, 'he') * dir;
      });
    }
    this.allFilteredData = source;
    this.applyPagination();
  }

  private allFilteredData: OrgUnit[] = [];

  private getFilteredData(): OrgUnit[] {
    const term = this.filterValue.trim().toLowerCase();
    return term
      ? this.allData.filter(u => u.name.toLowerCase().includes(term))
      : [...this.allData];
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const paged = this.allFilteredData.slice(start, start + this.pageSize);
    this.displayData = paged.map(item => ({
      ...item,
      description: item.description || '—',
      contactEmail: item.contactEmail || '—',
      isActiveLabel: item.isActive ? 'פעיל' : 'לא פעיל',
    }));
  }
}
