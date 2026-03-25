import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Exam, ExamScore, SubmitScoreCommand } from '../../models/exam.models';
import { ExamService } from '../../services/exam.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-exam-score-form',
  template: `
    <div class="page-header">
      <h1>הזנת ציונים — {{ exam?.name }}</h1>
      <igds-button variant="secondary" [routerLink]="['/exams']">
        חזרה לרשימה
      </igds-button>
    </div>

    <igds-card>
      <div igds-card-header>
        <h2 class="card-title">הזנת ציון למועמדות</h2>
      </div>
      <div igds-card-body>
        <form [formGroup]="scoreForm" (ngSubmit)="onSubmitScore()" class="score-form">
          <igds-input-field
            label="מזהה מועמדות"
            type="number"
            formControlName="candidacyId"
            [required]="true"
            [error]="scoreForm.get('candidacyId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="ציון בודק ראשון"
            type="number"
            formControlName="firstExaminerScore">
          </igds-input-field>

          <igds-input-field
            label="ציון בודק שני"
            type="number"
            formControlName="secondExaminerScore">
          </igds-input-field>

          <igds-button variant="primary" type="submit"
                       [disabled]="scoreForm.invalid || submitting">
            שמירת ציון
          </igds-button>
        </form>
      </div>
    </igds-card>

    <igds-card>
      <div igds-card-header>
        <h2 class="card-title">ציונים שהוזנו</h2>
      </div>
      <div igds-card-body>
        <app-loading-spinner [loading]="loading"></app-loading-spinner>

        <igds-table
          *ngIf="!loading"
          [data]="paginatedData"
          [columns]="columns"
          emptyMessage="לא הוזנו ציונים עדיין">
          <ng-template igdsTableCell="firstExaminerScore" let-row>
            {{ row.firstExaminerScore ?? '—' }}
          </ng-template>
          <ng-template igdsTableCell="secondExaminerScore" let-row>
            {{ row.secondExaminerScore ?? '—' }}
          </ng-template>
          <ng-template igdsTableCell="finalScore" let-row>
            {{ row.finalScore ?? '—' }}
          </ng-template>
          <ng-template igdsTableCell="passedThreshold" let-row>
            <igds-status-badge *ngIf="row.passedThreshold === true" variant="success" label="עבר"></igds-status-badge>
            <igds-status-badge *ngIf="row.passedThreshold === false" variant="failure" label="לא עבר"></igds-status-badge>
            <span *ngIf="row.passedThreshold == null">—</span>
          </ng-template>
          <ng-template igdsTableCell="scoredAt" let-row>
            {{ row.scoredAt | hebrewDate }}
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
    .card-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-lg);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .score-form {
      display: flex;
      gap: var(--igds-space-12);
      flex-wrap: wrap;
      align-items: flex-start;
    }
    igds-card + igds-card { margin-block-start: var(--igds-space-16); }
  `],
})
export class ExamScoreFormComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'candidacyId', label: 'מועמדות' },
    { key: 'firstExaminerScore', label: 'בודק ראשון' },
    { key: 'secondExaminerScore', label: 'בודק שני' },
    { key: 'finalScore', label: 'ציון סופי' },
    { key: 'passedThreshold', label: 'עבר סף' },
    { key: 'scoredAt', label: 'תאריך' },
  ];
  allScores: ExamScore[] = [];
  paginatedData: ExamScore[] = [];
  exam: Exam | null = null;
  loading = false;
  submitting = false;
  scoreForm!: FormGroup;
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;

  constructor(
    private examService: ExamService,
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.scoreForm = this.fb.group({
      candidacyId: [null, Validators.required],
      firstExaminerScore: [null],
      secondExaminerScore: [null],
    });

    const examId = Number(this.route.snapshot.paramMap.get('id'));
    if (examId) {
      this.loadExam(examId);
      this.loadScores(examId);
    }
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyPagination();
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    this.paginatedData = this.allScores.slice(start, start + this.pageSize);
  }

  loadExam(id: number): void {
    this.examService.getById(id).subscribe({
      next: (exam) => (this.exam = exam),
      error: () => this.notification.error('שגיאה בטעינת פרטי המבחן'),
    });
  }

  loadScores(examId: number): void {
    this.loading = true;
    this.examService.getScoresByExam(examId).subscribe({
      next: (scores) => {
        this.allScores = scores;
        this.totalItems = scores.length;
        this.currentPage = 1;
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת ציונים');
        this.loading = false;
      },
    });
  }

  onSubmitScore(): void {
    if (this.scoreForm.invalid || !this.exam) return;

    this.submitting = true;
    const command: SubmitScoreCommand = {
      examId: this.exam.id,
      candidacyId: this.scoreForm.value.candidacyId,
      firstExaminerScore: this.scoreForm.value.firstExaminerScore,
      secondExaminerScore: this.scoreForm.value.secondExaminerScore,
    };

    this.examService.submitScore(command).subscribe({
      next: () => {
        this.notification.success('הציון נשמר בהצלחה');
        this.scoreForm.reset();
        this.loadScores(this.exam!.id);
        this.submitting = false;
      },
      error: () => {
        this.notification.error('שגיאה בשמירת הציון');
        this.submitting = false;
      },
    });
  }
}
