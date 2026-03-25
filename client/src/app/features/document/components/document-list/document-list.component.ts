import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Document, DocumentQueryParams } from '../../models/document.models';
import { DocumentService } from '../../services/document.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-document-list',
  template: `
    <div class="page-header">
      <h1>ניהול מסמכים</h1>
      <igds-button variant="primary" (onClick)="navigateToUpload()">
        העלאת מסמך
      </igds-button>
    </div>

    <igds-card>
      <div class="filters-row">
        <igds-input-field
          label="מועמדות"
          type="number"
          placeholder="מזהה מועמדות"
          [(ngModel)]="filters.candidacyId"
          (ngModelChange)="onFilterChange()">
        </igds-input-field>

        <igds-input-field
          label="סוג מסמך"
          placeholder="סוג מסמך"
          [(ngModel)]="filters.documentType"
          (ngModelChange)="onFilterChange()">
        </igds-input-field>

        <igds-dropdown
          label="סטטוס"
          [options]="statusOptions"
          [(ngModel)]="filters.status"
          (ngModelChange)="onFilterChange()"
          placeholder="הכל">
        </igds-dropdown>
      </div>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו מסמכים
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
              [text]="row.status">
            </igds-status-badge>
          </div>
          <div class="row-actions">
            <igds-button variant="secondary" [iconOnly]="true"
              ariaLabel="אישור" (onClick)="onReview(row, 'Approved')"
              *ngIf="row._status === 'Uploaded'"
              [igdsTooltip]="'אישור'">
              <span igds-icon>✓</span>
            </igds-button>
            <igds-button variant="secondary" [iconOnly]="true"
              ariaLabel="דחייה" (onClick)="onReview(row, 'Rejected')"
              *ngIf="row._status === 'Uploaded'"
              [igdsTooltip]="'דחייה'">
              <span igds-icon>✗</span>
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
export class DocumentListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'id', label: 'מזהה', sortable: true },
    { key: 'candidacyId', label: 'מועמדות', sortable: true },
    { key: 'documentType', label: 'סוג מסמך', sortable: true },
    { key: 'fileName', label: 'שם קובץ', sortable: true },
    { key: 'status', label: 'סטטוס', sortable: true },
    { key: 'uploadedAt', label: 'תאריך העלאה', sortable: true },
    { key: 'version', label: 'גרסה', sortable: true },
  ];

  statusOptions: IgdsDropdownOption[] = [
    { value: '', label: 'הכל' },
    { value: 'Uploaded', label: 'הועלה' },
    { value: 'Approved', label: 'אושר' },
    { value: 'Rejected', label: 'נדחה' },
  ];

  allData: Document[] = [];
  displayData: any[] = [];
  loading = false;
  filters: DocumentQueryParams = {};

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private documentService: DocumentService,
    private route: ActivatedRoute,
    private router: Router,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    const candidacyId = this.route.snapshot.queryParamMap.get('candidacyId');
    if (candidacyId) {
      this.filters.candidacyId = +candidacyId;
    }
    this.loadData();
  }

  navigateToUpload(): void {
    this.router.navigate(['upload'], { relativeTo: this.route });
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
    this.documentService.list(this.filters).subscribe({
      next: (data: Document[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת מסמכים');
        this.loading = false;
      },
    });
  }

  onReview(row: any, status: 'Approved' | 'Rejected'): void {
    const doc = this.allData.find(d => d.id === row.id);
    if (!doc) return;

    const label = status === 'Approved' ? 'אישור' : 'דחייה';
    const modalRef = this.modalService.open<boolean>({
      title: `${label} מסמך`,
      component: ConfirmDialogComponent,
      data: {
        title: `${label} מסמך`,
        message: `האם לבצע ${label} למסמך "${doc.fileName}"?`,
        confirmText: label,
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.documentService.review(doc.id, {
          documentId: doc.id,
          status,
          reviewedByUserId: 0,
        }).subscribe({
          next: () => {
            this.notification.success(`המסמך ${status === 'Approved' ? 'אושר' : 'נדחה'} בהצלחה`);
            this.loadData();
          },
          error: () => this.notification.error('שגיאה בעדכון סטטוס המסמך'),
        });
      }
    });
  }

  onDelete(row: any): void {
    const doc = this.allData.find(d => d.id === row.id);
    if (!doc) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת מסמך',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת מסמך',
        message: `האם למחוק את המסמך "${doc.fileName}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.documentService.delete(doc.id).subscribe({
          next: () => {
            this.notification.success('המסמך נמחק בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת המסמך'),
        });
      }
    });
  }

  getStatusVariant(status: string): 'success' | 'warning' | 'failure' | 'info' | 'neutral' {
    switch (status) {
      case 'Uploaded': return 'warning';
      case 'Approved': return 'success';
      case 'Rejected': return 'failure';
      default: return 'neutral';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Uploaded': return 'הועלה';
      case 'Approved': return 'אושר';
      case 'Rejected': return 'נדחה';
      default: return 'חסר';
    }
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
      status: this.getStatusLabel(item.status),
    }));
  }
}
