import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConflictService } from '../../services/conflict.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-manual-review-list',
  template: `
    <div class="page-header">
      <h1>מועמדויות לבדיקה ידנית</h1>
    </div>

    <igds-card>
      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        אין מועמדויות הדורשות בדיקה ידנית
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
          <igds-button variant="primary" [iconBefore]="true"
                      (onClick)="onViewDeclarations(row.candidacyId)">
            <span igds-icon-before>👁️</span>
            צפייה בהצהרות
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
    .actions-list { display: none; }
  `],
})
export class ManualReviewListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'candidacyId', label: 'מזהה מועמדות', sortable: true },
  ];

  allData: number[] = [];
  displayData: { candidacyId: number }[] = [];
  loading = false;

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private conflictService: ConflictService,
    private router: Router,
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
    this.conflictService.getCandidacyIdsRequiringManualReview().subscribe({
      next: (ids: number[]) => {
        this.allData = ids;
        this.totalItems = ids.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת רשימת מועמדויות לבדיקה');
        this.loading = false;
      },
    });
  }

  onViewDeclarations(candidacyId: number): void {
    this.router.navigate(['/conflicts/candidacy', candidacyId]);
  }

  private applySort(): void {
    if (!this.sortColumn) return;
    const dir = this.sortDirection === 'asc' ? 1 : -1;
    this.allData = [...this.allData].sort((a, b) => (a - b) * dir);
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const paged = this.allData.slice(start, start + this.pageSize);
    this.displayData = paged.map(id => ({ candidacyId: id }));
  }
}
