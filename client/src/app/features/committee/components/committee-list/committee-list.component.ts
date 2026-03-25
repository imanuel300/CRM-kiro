import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Committee, CommitteeQueryParams } from '../../models/committee.models';
import { CommitteeService } from '../../services/committee.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-committee-list',
  template: `
    <div class="page-header">
      <h1>ניהול ועדות</h1>
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
      </div>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו ועדות
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
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="ישיבות" (onClick)="goToMeetings(row)">
            <span igds-icon>📅</span>
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
    .filters-row > * { flex: 1; min-width: 150px; max-width: 250px; }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .actions-column { display: none; }
  `],
})
export class CommitteeListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'id', label: 'מזהה', sortable: true },
    { key: 'name', label: 'שם הוועדה', sortable: true },
    { key: 'description', label: 'תיאור' },
    { key: 'membersCount', label: 'חברים' },
    { key: 'createdAt', label: 'תאריך יצירה', sortable: true },
  ];

  allData: Committee[] = [];
  displayData: any[] = [];
  loading = false;
  filters: CommitteeQueryParams = {};

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private committeeService: CommitteeService,
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
    this.committeeService.list(this.filters).subscribe({
      next: (data: Committee[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת ועדות');
        this.loading = false;
      },
    });
  }

  goToMeetings(row: any): void {
    const committee = this.allData.find(c => c.id === row.id);
    if (committee) {
      this.router.navigate(['/committees', committee.id, 'meetings']);
    }
  }

  onDelete(row: any): void {
    const committee = this.allData.find(c => c.id === row.id);
    if (!committee) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת ועדה',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת ועדה',
        message: `האם למחוק את הוועדה "${committee.name}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.committeeService.delete(committee.id).subscribe({
          next: () => {
            this.notification.success('הוועדה נמחקה בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת הוועדה'),
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
      description: item.description || '—',
      membersCount: item.members?.length || 0,
    }));
  }
}
