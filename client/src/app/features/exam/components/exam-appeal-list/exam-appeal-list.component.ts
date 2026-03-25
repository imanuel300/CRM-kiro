import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Exam, ExamScore, SubmitAppealCommand } from '../../models/exam.models';
import { ExamService } from '../../services/exam.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-exam-appeal-list',
  template: `
    <div class="page-header">
      <h1>ערעורים — {{ exam?.name }}</h1>
      <igds-button variant="secondary" [routerLink]="['/exams']">
        חזרה לרשימה
      </igds-button>
    </div>

    <igds-card>
      <h2 igds-card-header class="card-title">הגשת ערעור</h2>
      <form [formGroup]="appealForm" (ngSubmit)="onSubmitAppeal()" class="appeal-form">
        <igds-input-field
          label="מזהה מועמדות"
          type="number"
          formControlName="candidacyId"
          [required]="true"
          [error]="appealForm.get('candidacyId')?.touched && appealForm.get('candidacyId')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-input-field>

        <igds-input-field
          label="ציון ערעור"
          type="number"
          formControlName="appealScore"
          [required]="true"
          [error]="appealForm.get('appealScore')?.touched && appealForm.get('appealScore')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-input-field>

        <igds-input-field
          label="סיבת ערעור"
          formControlName="reason"
          class="reason-field">
        </igds-input-field>

        <igds-button variant="primary" type="submit"
                     [disabled]="appealForm.invalid || submitting">
          הגשת ערעור
        </igds-button>
      </form>
    </igds-card>

    <igds-card class="appeals-table-card">
      <h2 igds-card-header class="card-title">ציונים עם ערעורים</h2>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו ציונים
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
    .card-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-lg);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .appeal-form {
      display: flex;
      gap: var(--igds-space-12);
      flex-wrap: wrap;
      align-items: flex-end;
    }
    .appeal-form > * { flex: 1; min-width: 150px; }
    .reason-field { flex: 2 !important; min-width: 250px; }
    .appeals-table-card { margin-block-start: var(--igds-space-16); }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
  `],
})
export class ExamAppealListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'candidacyId', label: 'מועמדות', sortable: true },
    { key: 'finalScore', label: 'ציון סופי', sortable: true },
    { key: 'isAppealedLabel', label: 'ערעור', sortable: true },
    { key: 'appealScore', label: 'ציון ערעור', sortable: true },
    { key: 'passedThresholdLabel', label: 'עבר סף', sortable: true },
  ];

  allData: ExamScore[] = [];
  displayData: any[] = [];
  exam: Exam | null = null;
  loading = false;
  submitting = false;
  appealForm!: FormGroup;

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private examService: ExamService,
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.appealForm = this.fb.group({
      candidacyId: [null, Validators.required],
      appealScore: [null, Validators.required],
      reason: [''],
    });

    const examId = Number(this.route.snapshot.paramMap.get('id'));
    if (examId) {
      this.loadExam(examId);
      this.loadScores(examId);
    }
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

  loadExam(id: number): void {
    this.examService.getById(id).subscribe({
      next: (exam: Exam) => (this.exam = exam),
      error: () => this.notification.error('שגיאה בטעינת פרטי המבחן'),
    });
  }

  loadScores(examId: number): void {
    this.loading = true;
    this.examService.getScoresByExam(examId).subscribe({
      next: (scores: ExamScore[]) => {
        this.allData = scores;
        this.totalItems = scores.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת ציונים');
        this.loading = false;
      },
    });
  }

  onSubmitAppeal(): void {
    if (this.appealForm.invalid || !this.exam) return;

    this.submitting = true;
    const command: SubmitAppealCommand = {
      examId: this.exam.id,
      candidacyId: this.appealForm.value.candidacyId,
      appealScore: this.appealForm.value.appealScore,
      reason: this.appealForm.value.reason || undefined,
    };

    this.examService.submitAppeal(command).subscribe({
      next: () => {
        this.notification.success('הערעור הוגש בהצלחה');
        this.appealForm.reset();
        this.loadScores(this.exam!.id);
        this.submitting = false;
      },
      error: () => {
        this.notification.error('שגיאה בהגשת הערעור');
        this.submitting = false;
      },
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
      finalScore: item.finalScore ?? '—',
      isAppealedLabel: item.isAppealed ? '⚖ כן' : '—',
      appealScore: item.appealScore ?? '—',
      passedThresholdLabel: item.passedThreshold === true ? '✓ עבר' : item.passedThreshold === false ? '✗ לא עבר' : '—',
    }));
  }
}
