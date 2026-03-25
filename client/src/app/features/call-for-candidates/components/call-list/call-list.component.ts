import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CallForCandidates, CallForCandidatesQueryParams } from '../../models/call-for-candidates.models';
import { CallForCandidatesService } from '../../services/call-for-candidates.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-call-list',
  template: `
    <div class="page-header">
      <h1>ניהול קולות קוראים</h1>
      <igds-button variant="primary" (onClick)="onCreate()">
        קול קורא חדש
      </igds-button>
    </div>

    <igds-card>
      <div class="filters-row">
        <igds-input-field
          label="יחידה ארגונית"
          type="number"
          placeholder="מזהה יחידה"
          [(ngModel)]="filters.orgUnitId"
          (ngModelChange)="onFilterChange()">
        </igds-input-field>

        <igds-dropdown
          label="סטטוס"
          [options]="activeOptions"
          [(ngModel)]="activeFilter"
          (ngModelChange)="onFilterChange()">
        </igds-dropdown>

        <igds-dropdown
          label="סוג"
          [options]="tenderOptions"
          [(ngModel)]="tenderFilter"
          (ngModelChange)="onFilterChange()">
        </igds-dropdown>
      </div>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו קולות קוראים
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
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="צפייה" (onClick)="onView(row)">
            <span igds-icon>👁</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="עריכה" (onClick)="onEdit(row)">
            <span igds-icon>✏</span>
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
    .filters-row {
      display: flex;
      gap: var(--igds-space-12);
      flex-wrap: wrap;
      margin-block-end: var(--igds-space-16);
    }
    .filters-row > * { flex: 1; min-width: 150px; }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .actions-column { display: none; }
  `],
})
export class CallListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'id', label: 'מזהה', sortable: true },
    { key: 'title', label: 'כותרת', sortable: true },
    { key: 'orgUnitId', label: 'יחידה ארגונית', sortable: true },
    { key: 'isTender', label: 'מכרז', sortable: true },
    { key: 'openDate', label: 'תאריך פתיחה', sortable: true },
    { key: 'closeDate', label: 'תאריך סגירה', sortable: true },
    { key: 'isActive', label: 'פעיל', sortable: true },
  ];

  activeOptions: IgdsDropdownOption[] = [
    { value: 'all', label: 'הכל' },
    { value: 'active', label: 'פעילים' },
    { value: 'inactive', label: 'לא פעילים' },
  ];

  tenderOptions: IgdsDropdownOption[] = [
    { value: 'all', label: 'הכל' },
    { value: 'tender', label: 'מכרזים' },
    { value: 'regular', label: 'רגילים' },
  ];

  allData: CallForCandidates[] = [];
  displayData: any[] = [];
  loading = false;
  activeFilter = 'all';
  tenderFilter = 'all';
  filters: CallForCandidatesQueryParams = {};

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private callService: CallForCandidatesService,
    private router: Router,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  onFilterChange(): void {
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

  loadData(): void {
    this.loading = true;
    const params: CallForCandidatesQueryParams = { ...this.filters };
    if (this.activeFilter === 'active') params.isActive = true;
    else if (this.activeFilter === 'inactive') params.isActive = false;
    if (this.tenderFilter === 'tender') params.isTender = true;
    else if (this.tenderFilter === 'regular') params.isTender = false;

    this.callService.list(params).subscribe({
      next: (data: CallForCandidates[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת קולות קוראים');
        this.loading = false;
      },
    });
  }

  onCreate(): void {
    this.router.navigate(['/calls/new']);
  }

  onView(row: any): void {
    this.router.navigate(['/calls', row.id]);
  }

  onEdit(row: any): void {
    this.router.navigate(['/calls', row.id, 'edit']);
  }

  onDelete(row: any): void {
    const call = this.allData.find(c => c.id === row.id);
    if (!call) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת קול קורא',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת קול קורא',
        message: `האם למחוק את הקול קורא "${call.title}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.callService.delete(call.id).subscribe({
          next: () => {
            this.notification.success('הקול קורא נמחק בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת הקול קורא'),
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
      isTender: item.isTender ? '⚖' : '—',
      openDate: item.openDate || '—',
      closeDate: item.closeDate || '—',
      isActive: item.isActive ? '✓' : '✗',
    }));
  }
}
