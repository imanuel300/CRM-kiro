import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Exam, ExamQueryParams } from '../../models/exam.models';
import { ExamService } from '../../services/exam.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-exam-list',
  template: `
    <div class="page-header">
      <h1>ניהול מבחנים</h1>
      <igds-button variant="primary" [routerLink]="['new']">
        מבחן חדש
      </igds-button>
    </div>

    <igds-card>
      <div igds-card-body>
        <div class="filters-row">
          <igds-input-field
            label="יחידה ארגונית"
            type="number"
            [ngModel]="filters.orgUnitId"
            (ngModelChange)="filters.orgUnitId = $event; onFilterChange()"
            placeholder="מזהה יחידה">
          </igds-input-field>

          <igds-input-field
            label="קול קורא"
            type="number"
            [ngModel]="filters.callForCandidatesId"
            (ngModelChange)="filters.callForCandidatesId = $event; onFilterChange()"
            placeholder="מזהה קול קורא">
          </igds-input-field>
        </div>

        <app-loading-spinner [loading]="loading"></app-loading-spinner>

        <igds-table
          *ngIf="!loading"
          [data]="filteredData"
          [columns]="columns"
          [sortColumn]="sortColumn"
          [sortDirection]="sortDirection"
          (sort)="onSort($event)"
          emptyMessage="לא נמצאו מבחנים">
          <ng-template igdsTableCell="examDate" let-row>
            {{ row.examDate | hebrewDate }}
          </ng-template>
          <ng-template igdsTableCell="location" let-row>
            {{ row.location || '—' }}
          </ng-template>
          <ng-template igdsTableCell="passingScore" let-row>
            {{ row.passingScore ?? '—' }}
          </ng-template>
          <ng-template igdsTableCell="actions" let-row>
            <igds-button variant="secondary" [iconOnly]="true" igdsTooltip="ציונים"
                         (onClick)="goToScores(row)">📊</igds-button>
            <igds-button variant="secondary" [iconOnly]="true" igdsTooltip="ערעורים"
                         (onClick)="goToAppeals(row)">⚖️</igds-button>
            <igds-button variant="secondary" [iconOnly]="true" igdsTooltip="מחיקה"
                         (onClick)="onDelete(row)">🗑️</igds-button>
          </ng-template>
        </igds-table>

        <igds-pagination
          [totalItems]="totalItems"
          [pageSize]="pageSize"
          [currentPage]="currentPage"
          (pageChange)="onPageChange($event)">
        </igds-pagination>
      </div>
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
      color: var(--igds-text-primary);
      margin: 0;
    }
    .filters-row {
      display: flex;
      gap: var(--igds-space-12);
      flex-wrap: wrap;
      margin-block-end: var(--igds-space-8);
    }
  `],
})
export class ExamListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'id', label: 'מזהה', sortable: true },
    { key: 'name', label: 'שם מבחן', sortable: true },
    { key: 'examDate', label: 'תאריך', sortable: true },
    { key: 'location', label: 'מיקום', sortable: true },
    { key: 'maxScore', label: 'ציון מקסימלי', sortable: true },
    { key: 'passingScore', label: 'ציון סף', sortable: true },
    { key: 'actions', label: 'פעולות' },
  ];
  allData: Exam[] = [];
  filteredData: Exam[] = [];
  loading = false;
  filters: ExamQueryParams = {};
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private examService: ExamService,
    private router: Router,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  onFilterChange(): void {
    this.loadData();
  }

  onSort(event: { column: string; direction: 'asc' | 'desc' }): void {
    this.sortColumn = event.column;
    this.sortDirection = event.direction;
    this.applyClientSide();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyClientSide();
  }

  loadData(): void {
    this.loading = true;
    this.examService.list(this.filters).subscribe({
      next: (data: Exam[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.currentPage = 1;
        this.applyClientSide();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת מבחנים');
        this.loading = false;
      },
    });
  }

  private applyClientSide(): void {
    let data = [...this.allData];
    if (this.sortColumn) {
      data.sort((a: any, b: any) => {
        const valA = a[this.sortColumn];
        const valB = b[this.sortColumn];
        const cmp = valA > valB ? 1 : valA < valB ? -1 : 0;
        return this.sortDirection === 'asc' ? cmp : -cmp;
      });
    }
    const start = (this.currentPage - 1) * this.pageSize;
    this.filteredData = data.slice(start, start + this.pageSize);
  }

  goToScores(exam: Exam): void {
    this.router.navigate(['/exams', exam.id, 'scores']);
  }

  goToAppeals(exam: Exam): void {
    this.router.navigate(['/exams', exam.id, 'appeals']);
  }

  onDelete(exam: Exam): void {
    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת מבחן',
      data: {
        message: `האם למחוק את המבחן "${exam.name}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.examService.delete(exam.id).subscribe({
          next: () => {
            this.notification.success('המבחן נמחק בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת המבחן'),
        });
      }
    });
  }
}
