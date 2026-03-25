import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  Interview,
  InterviewQueryParams,
  InterviewStatus,
  InterviewType,
} from '../../models/interview.models';
import { InterviewService } from '../../services/interview.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-interview-list',
  template: `
    <div class="page-header">
      <h1>ניהול ראיונות</h1>
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

        <igds-input-field
          label="קול קורא"
          type="number"
          placeholder="מזהה קול קורא"
          [(ngModel)]="filters.callForCandidatesId"
          (ngModelChange)="onFilterChange()">
        </igds-input-field>

        <igds-input-field
          label="מועמדות"
          type="number"
          placeholder="מזהה מועמדות"
          [(ngModel)]="filters.candidacyId"
          (ngModelChange)="onFilterChange()">
        </igds-input-field>
      </div>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו ראיונות
      </div>

      <igds-table
        *ngIf="!loading && displayData.length > 0"
        [columns]="columns"
        [data]="displayData"
        [sortColumn]="sortColumn"
        [sortDirection]="sortDirection"
        (sort)="onSort($event)">
      </igds-table>

      <div *ngIf="!loading && allData.length > 0" class="status-actions-overlay">
        <div *ngFor="let row of displayData; let i = index" class="row-extras">
          <div class="row-status">
            <igds-status-badge
              [variant]="getStatusVariant(row._status)"
              [text]="row.statusLabel">
            </igds-status-badge>
          </div>
          <div class="row-actions">
            <igds-button variant="secondary" [iconOnly]="true"
              ariaLabel="משוב" (onClick)="goToFeedback(row)"
              [igdsTooltip]="'משוב'">
              <span igds-icon>📝</span>
            </igds-button>
            <igds-button variant="secondary" [iconOnly]="true"
              ariaLabel="ראיון שני" (onClick)="goToSchedule(row)"
              *ngIf="row._interviewType === InterviewType.First && row._status === InterviewStatus.Completed"
              [igdsTooltip]="'ראיון שני'">
              <span igds-icon>🔄</span>
            </igds-button>
            <igds-button variant="secondary" [iconOnly]="true"
              ariaLabel="מחיקה" (onClick)="onDelete(row)"
              [igdsTooltip]="'מחיקה'">
              <span igds-icon>🗑</span>
            </igds-button>
          </div>
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
    .status-actions-overlay { display: none; }
  `],
})
export class InterviewListComponent implements OnInit {
  readonly InterviewType = InterviewType;
  readonly InterviewStatus = InterviewStatus;

  columns: IgdsTableColumn[] = [
    { key: 'id', label: 'מזהה', sortable: true },
    { key: 'candidacyId', label: 'מועמדות', sortable: true },
    { key: 'scheduledDate', label: 'תאריך', sortable: true },
    { key: 'time', label: 'שעה' },
    { key: 'location', label: 'מיקום', sortable: true },
    { key: 'interviewType', label: 'סוג', sortable: true },
    { key: 'statusLabel', label: 'סטטוס', sortable: true },
  ];

  allData: Interview[] = [];
  displayData: any[] = [];
  loading = false;
  filters: InterviewQueryParams = {};

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private interviewService: InterviewService,
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
    this.interviewService.list(this.filters).subscribe({
      next: (data: Interview[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת ראיונות');
        this.loading = false;
      },
    });
  }

  formatTime(time: string): string {
    if (!time) return '—';
    const parts = time.split(':');
    return parts.length >= 2 ? `${parts[0]}:${parts[1]}` : time;
  }

  getTypeName(type: InterviewType): string {
    return type === InterviewType.Second ? 'ראיון שני' : 'ראיון ראשון';
  }

  getStatusName(status: InterviewStatus): string {
    switch (status) {
      case InterviewStatus.Scheduled: return 'מתוזמן';
      case InterviewStatus.Completed: return 'הושלם';
      case InterviewStatus.Cancelled: return 'בוטל';
      default: return '';
    }
  }

  getStatusVariant(status: InterviewStatus): 'success' | 'warning' | 'failure' | 'info' | 'neutral' {
    switch (status) {
      case InterviewStatus.Scheduled: return 'info';
      case InterviewStatus.Completed: return 'success';
      case InterviewStatus.Cancelled: return 'failure';
      default: return 'neutral';
    }
  }

  goToFeedback(row: any): void {
    this.router.navigate(['/interviews', row.id, 'feedback']);
  }

  goToSchedule(row: any): void {
    this.router.navigate(['/interviews', row.id, 'second-interview']);
  }

  onDelete(row: any): void {
    const interview = this.allData.find(i => i.id === row.id);
    if (!interview) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת ראיון',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת ראיון',
        message: `האם למחוק את הראיון #${interview.id}?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.interviewService.delete(interview.id).subscribe({
          next: () => {
            this.notification.success('הראיון נמחק בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת הראיון'),
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
      _status: item.status,
      _interviewType: item.interviewType,
      time: `${this.formatTime(item.startTime)} - ${this.formatTime(item.endTime)}`,
      location: item.location || '—',
      interviewType: this.getTypeName(item.interviewType),
      statusLabel: this.getStatusName(item.status),
      scheduledDate: item.scheduledDate,
    }));
  }
}
