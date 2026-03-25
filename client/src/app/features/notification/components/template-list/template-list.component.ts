import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationTemplate, TRIGGER_EVENTS } from '../../models/notification.models';
import { NotificationApiService } from '../../services/notification-api.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-template-list',
  template: `
    <div class="page-header">
      <h1>ניהול תבניות דיוור</h1>
      <igds-button variant="primary" (onClick)="onCreate()">
        תבנית חדשה
      </igds-button>
    </div>

    <igds-card>
      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו תבניות דיוור
      </div>

      <igds-table
        *ngIf="!loading && displayData.length > 0"
        [columns]="columns"
        [data]="displayData"
        [sortColumn]="sortColumn"
        [sortDirection]="sortDirection"
        (sort)="onSort($event)">
      </igds-table>

      <div *ngIf="!loading && allData.length > 0" class="row-actions-overlay">
        <div *ngFor="let row of displayData" class="row-extras">
          <div class="row-channel">
            <span *ngIf="row._channel === 'Email'">📧</span>
            <span *ngIf="row._channel === 'Sms'">💬</span>
          </div>
          <div class="row-active">
            <span [class]="row._isActive ? 'status-active' : 'status-inactive'">
              {{ row._isActive ? '✓' : '✗' }}
            </span>
          </div>
          <div class="row-actions">
            <igds-button variant="secondary" [iconOnly]="true"
              ariaLabel="עריכה" (onClick)="onEdit(row)"
              [igdsTooltip]="'עריכה'">
              <span igds-icon>✎</span>
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
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .row-actions-overlay { display: none; }
    .status-active { color: var(--igds-text-success); }
    .status-inactive { color: var(--igds-text-failure); }
  `],
})
export class TemplateListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'name', label: 'שם התבנית', sortable: true },
    { key: 'subject', label: 'נושא' },
    { key: 'channel', label: 'ערוץ', sortable: true },
    { key: 'triggerEvent', label: 'אירוע מפעיל', sortable: true },
    { key: 'isActive', label: 'פעיל' },
    { key: 'actions', label: 'פעולות' },
  ];

  allData: NotificationTemplate[] = [];
  displayData: any[] = [];
  loading = false;

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private notificationApi: NotificationApiService,
    private router: Router,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.notificationApi.listTemplates().subscribe({
      next: (data: NotificationTemplate[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת תבניות');
        this.loading = false;
      },
    });
  }

  getTriggerLabel(event: string): string {
    return TRIGGER_EVENTS.find(e => e.value === event)?.label ?? event;
  }

  onCreate(): void {
    this.router.navigate(['/notifications/templates/new']);
  }

  onEdit(row: any): void {
    const template = this.allData.find(t => t.id === row.id);
    if (template) {
      this.router.navigate(['/notifications/templates', template.id]);
    }
  }

  onDelete(row: any): void {
    const template = this.allData.find(t => t.id === row.id);
    if (!template) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת תבנית',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת תבנית',
        message: `האם למחוק את התבנית "${template.name}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.notificationApi.deleteTemplate(template.id).subscribe({
          next: () => {
            this.notification.success('התבנית נמחקה בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת התבנית'),
        });
      }
    });
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
      id: item.id,
      name: item.name,
      subject: item.subject,
      channel: item.channel === 'Email' ? 'דוא"ל' : 'SMS',
      triggerEvent: this.getTriggerLabel(item.triggerEvent),
      isActive: item.isActive ? 'פעיל' : 'לא פעיל',
      actions: '',
      _channel: item.channel,
      _isActive: item.isActive,
    }));
  }
}
