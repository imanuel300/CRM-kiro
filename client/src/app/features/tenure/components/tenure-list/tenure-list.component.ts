import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { Tenure, TENURE_END_REASON_LABELS } from '../../models/tenure.models';
import { TenureApiService } from '../../services/tenure.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-tenure-list',
  template: `
    <div class="page-header">
      <h1>ניהול כהונות</h1>
      <igds-button variant="primary" (onClick)="onCreate()">
        כהונה חדשה
      </igds-button>
    </div>

    <igds-card>
      <div class="filter-row">
        <igds-dropdown
          label="סינון לפי"
          [options]="filterTypeOptions"
          [formControl]="filterType">
        </igds-dropdown>

        <igds-input-field
          label="מזהה"
          type="number"
          [formControl]="filterId">
        </igds-input-field>

        <igds-button variant="secondary" (onClick)="onFilter()">
          סנן
        </igds-button>
      </div>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו כהונות
      </div>

      <igds-table
        *ngIf="!loading && displayData.length > 0"
        [columns]="columns"
        [data]="displayData"
        [sortColumn]="sortColumn"
        [sortDirection]="sortDirection"
        (sort)="onSort($event)">
      </igds-table>

      <div *ngIf="!loading && displayData.length > 0" class="actions-list">
        <div *ngFor="let row of displayData" class="actions-row">
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="עריכה"
                       igdsTooltip="עריכה" (onClick)="onEdit(row)">
            <span igds-icon>✏️</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="סיום כהונה"
                       igdsTooltip="סיום כהונה" (onClick)="onEnd(row)" *ngIf="row.isActive">
            <span igds-icon>📅</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="מחיקה"
                       igdsTooltip="מחיקה" (onClick)="onDelete(row)">
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
    .filter-row {
      display: flex;
      gap: var(--igds-space-16);
      align-items: flex-end;
      margin-block-end: var(--igds-space-16);
      flex-wrap: wrap;
    }
    .filter-row > * { flex: 1; min-width: 150px; max-width: 250px; }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .actions-list { display: none; }
  `],
})
export class TenureListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'position', label: 'תפקיד', sortable: true },
    { key: 'startDate', label: 'תאריך התחלה', sortable: true },
    { key: 'expectedEndDate', label: 'תאריך סיום צפוי', sortable: true },
    { key: 'statusLabel', label: 'סטטוס', sortable: true },
  ];

  filterTypeOptions: IgdsDropdownOption[] = [
    { value: 'orgUnit', label: 'יחידה ארגונית' },
    { value: 'contact', label: 'איש קשר' },
  ];

  allData: Tenure[] = [];
  displayData: any[] = [];
  loading = false;

  filterType = new FormControl('orgUnit');
  filterId = new FormControl<number | null>(1);

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private tenureApi: TenureApiService,
    private router: Router,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    const id = this.filterId.value ?? 1;
    const obs = this.filterType.value === 'contact'
      ? this.tenureApi.getByContact(id)
      : this.tenureApi.getByOrgUnit(id);

    obs.subscribe({
      next: (data: Tenure[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת כהונות');
        this.loading = false;
      },
    });
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadData();
  }

  onSort(event: { column: string; direction: 'asc' | 'desc' }): void {
    this.sortColumn = event.column;
    this.sortDirection = event.direction;
    this.applySort();
    this.applyPagination();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyPagination();
  }

  onCreate(): void {
    this.router.navigate(['/tenures/new']);
  }

  onEdit(row: any): void {
    this.router.navigate(['/tenures', row.id]);
  }

  onEnd(row: any): void {
    this.router.navigate(['/tenures', row.id, 'end']);
  }

  onDelete(row: any): void {
    const tenure = this.allData.find(t => t.id === row.id);
    if (!tenure) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת כהונה',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת כהונה',
        message: `האם למחוק את הכהונה "${tenure.position}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.tenureApi.delete(tenure.id).subscribe({
          next: () => {
            this.notification.success('הכהונה נמחקה בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת הכהונה'),
        });
      }
    });
  }

  private applySort(): void {
    if (!this.sortColumn) return;
    const dir = this.sortDirection === 'asc' ? 1 : -1;
    this.allData = [...this.allData].sort((a: any, b: any) => {
      const valA = (a[this.sortColumn] ?? '').toString().toLowerCase();
      const valB = (b[this.sortColumn] ?? '').toString().toLowerCase();
      return valA.localeCompare(valB, 'he') * dir;
    });
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const paged = this.allData.slice(start, start + this.pageSize);
    this.displayData = paged.map(item => ({
      ...item,
      startDate: this.formatDate(item.startDate),
      expectedEndDate: this.formatDate(item.expectedEndDate),
      statusLabel: item.isActive ? 'פעילה' : 'הסתיימה',
    }));
  }

  private formatDate(dateStr: string): string {
    if (!dateStr) return '';
    try {
      return new Date(dateStr).toLocaleDateString('he-IL');
    } catch {
      return dateStr;
    }
  }
}
