import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Candidacy, CandidacyQueryParams } from '../../models/candidacy.models';
import { CandidacyService } from '../../services/candidacy.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-candidacy-list',
  template: `
    <div class="page-header">
      <h1>ניהול מועמדויות</h1>
      <igds-button variant="primary" (onClick)="onCreate()">
        מועמדות חדשה
      </igds-button>
    </div>

    <igds-card>
      <div class="filters-row">
        <igds-input-field
          label="יחידה ארגונית"
          type="number"
          placeholder="מזהה יחידה"
          [(ngModel)]="orgUnitFilter"
          (ngModelChange)="onFilterChange()">
        </igds-input-field>

        <igds-input-field
          label="איש קשר"
          type="number"
          placeholder="מזהה איש קשר"
          [(ngModel)]="contactFilter"
          (ngModelChange)="onFilterChange()">
        </igds-input-field>

        <igds-input-field
          label="קול קורא"
          type="number"
          placeholder="מזהה קול קורא"
          [(ngModel)]="callFilter"
          (ngModelChange)="onFilterChange()">
        </igds-input-field>

        <igds-dropdown
          label="סטטוס"
          [options]="statusOptions"
          [(ngModel)]="activeFilter"
          (ngModelChange)="onFilterChange()">
        </igds-dropdown>
      </div>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו מועמדויות
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
export class CandidacyListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'id', label: 'מזהה', sortable: true },
    { key: 'contactId', label: 'איש קשר', sortable: true },
    { key: 'orgUnitId', label: 'יחידה ארגונית', sortable: true },
    { key: 'callForCandidatesId', label: 'קול קורא', sortable: true },
    { key: 'isActive', label: 'פעילה', sortable: true },
    { key: 'submittedAt', label: 'תאריך הגשה', sortable: true },
  ];

  statusOptions: IgdsDropdownOption[] = [
    { value: 'all', label: 'הכל' },
    { value: 'active', label: 'פעילות' },
    { value: 'inactive', label: 'לא פעילות' },
  ];

  allData: Candidacy[] = [];
  displayData: any[] = [];
  loading = false;
  activeFilter = 'all';
  orgUnitFilter = '';
  contactFilter = '';
  callFilter = '';

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private candidacyService: CandidacyService,
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
    const params: CandidacyQueryParams = {};
    if (this.orgUnitFilter) params.orgUnitId = Number(this.orgUnitFilter);
    if (this.contactFilter) params.contactId = Number(this.contactFilter);
    if (this.callFilter) params.callForCandidatesId = Number(this.callFilter);
    if (this.activeFilter === 'active') params.isActive = true;
    else if (this.activeFilter === 'inactive') params.isActive = false;

    this.candidacyService.list(params).subscribe({
      next: (data: Candidacy[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת מועמדויות');
        this.loading = false;
      },
    });
  }

  onCreate(): void {
    this.router.navigate(['/candidacies/new']);
  }

  onView(row: any): void {
    this.router.navigate(['/candidacies', row.id]);
  }

  onDelete(row: any): void {
    const candidacy = this.allData.find(c => c.id === row.id);
    if (!candidacy) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת מועמדות',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת מועמדות',
        message: `האם למחוק את מועמדות מספר ${candidacy.id}?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.candidacyService.delete(candidacy.id).subscribe({
          next: () => {
            this.notification.success('המועמדות נמחקה בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת המועמדות'),
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
      isActive: item.isActive ? '✓' : '✗',
      submittedAt: item.submittedAt || '—',
    }));
  }
}
