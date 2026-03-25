import { Component, OnInit } from '@angular/core';
import {
  NotificationLog,
  NotificationLogQueryParams,
} from '../../models/notification.models';
import { NotificationApiService } from '../../services/notification-api.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTableColumn } from '@igds/angular';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-notification-log',
  template: `
    <div class="page-header">
      <h1>היסטוריית שליחות</h1>
    </div>

    <igds-card>
      <div class="filters-row">
        <igds-dropdown
          label="ערוץ"
          [options]="channelOptions"
          [(ngModel)]="filters.channel"
          (ngModelChange)="onFilterChange()"
          placeholder="הכל">
        </igds-dropdown>

        <igds-dropdown
          label="סטטוס"
          [options]="statusOptions"
          [(ngModel)]="filters.status"
          (ngModelChange)="onFilterChange()"
          placeholder="הכל">
        </igds-dropdown>

        <igds-date-picker
          label="מתאריך"
          [(ngModel)]="filters.fromDate"
          (ngModelChange)="onFilterChange()">
        </igds-date-picker>

        <igds-date-picker
          label="עד תאריך"
          [(ngModel)]="filters.toDate"
          (ngModelChange)="onFilterChange()">
        </igds-date-picker>
      </div>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו רשומות
      </div>

      <igds-table
        *ngIf="!loading && displayData.length > 0"
        [columns]="columns"
        [data]="displayData"
        [sortColumn]="sortColumn"
        [sortDirection]="sortDirection"
        (sort)="onSort($event)">
      </igds-table>

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
    .filters-row > * { flex: 1; min-width: 150px; max-width: 220px; }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
  `],
})
export class NotificationLogComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'sentAt', label: 'תאריך שליחה', sortable: true },
    { key: 'recipient', label: 'נמען' },
    { key: 'subject', label: 'נושא' },
    { key: 'channel', label: 'ערוץ', sortable: true },
    { key: 'status', label: 'סטטוס', sortable: true },
    { key: 'errorMessage', label: 'שגיאה' },
  ];

  channelOptions: IgdsDropdownOption[] = [
    { value: '', label: 'הכל' },
    { value: 'Email', label: 'דוא"ל' },
    { value: 'Sms', label: 'SMS' },
  ];

  statusOptions: IgdsDropdownOption[] = [
    { value: '', label: 'הכל' },
    { value: 'Sent', label: 'נשלח' },
    { value: 'Failed', label: 'נכשל' },
    { value: 'Pending', label: 'ממתין' },
  ];

  allData: NotificationLog[] = [];
  displayData: any[] = [];
  loading = false;
  filters: NotificationLogQueryParams = {};

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private notificationApi: NotificationApiService,
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
    this.notificationApi.getLogs(this.filters).subscribe({
      next: (data: NotificationLog[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת היסטוריית שליחות');
        this.loading = false;
      },
    });
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Sent': return 'נשלח';
      case 'Failed': return 'נכשל';
      case 'Pending': return 'ממתין';
      default: return status;
    }
  }

  getChannelLabel(channel: string): string {
    return channel === 'Email' ? 'דוא"ל' : 'SMS';
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
      sentAt: item.sentAt,
      recipient: item.recipient,
      subject: item.subject,
      channel: this.getChannelLabel(item.channel),
      status: this.getStatusLabel(item.status),
      errorMessage: item.errorMessage || '—',
    }));
  }
}
