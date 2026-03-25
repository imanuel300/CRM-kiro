import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { Quota } from '../../models/quota.models';
import { QuotaApiService } from '../../services/quota.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-quota-list',
  template: `
    <div class="page-header">
      <h1>ניהול מכסות</h1>
    </div>

    <igds-card>
      <div class="filter-row">
        <igds-input-field
          label="מזהה יחידה ארגונית"
          type="number"
          [formControl]="orgUnitId">
        </igds-input-field>

        <igds-button variant="secondary" (onClick)="onFilter()">
          סנן
        </igds-button>
      </div>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו מכסות
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
    .filter-row {
      display: flex;
      gap: var(--igds-space-16);
      align-items: center;
      margin-block-end: var(--igds-space-16);
    }
    .filter-row > *:first-child { min-width: 160px; max-width: 250px; }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .actions-column { display: none; }
  `],
})
export class QuotaListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'categoryName', label: 'קטגוריה', sortable: true },
    { key: 'targetCount', label: 'יעד', sortable: true },
    { key: 'currentCount', label: 'מילוי נוכחי', sortable: true },
    { key: 'statusDisplay', label: 'סטטוס', sortable: true },
  ];

  allData: Quota[] = [];
  displayData: any[] = [];
  loading = false;

  orgUnitId = new FormControl<number | null>(1);

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private quotaApi: QuotaApiService,
    private router: Router,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
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

  loadData(): void {
    this.loading = true;
    const id = this.orgUnitId.value ?? 1;

    this.quotaApi.getByOrgUnit(id).subscribe({
      next: (data: Quota[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת מכסות');
        this.loading = false;
      },
    });
  }

  onFilter(): void {
    this.currentPage = 1;
    this.loadData();
  }

  onDelete(row: any): void {
    const quota = this.allData.find(q => q.id === row.id);
    if (!quota) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת מכסה',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת מכסה',
        message: `האם למחוק את המכסה "${quota.categoryName}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.quotaApi.delete(quota.id).subscribe({
          next: () => {
            this.notification.success('המכסה נמחקה בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת המכסה'),
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
      statusDisplay: item.isActive ? 'פעילה' : 'לא פעילה',
    }));
  }
}
