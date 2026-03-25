import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  Interview,
  InterviewFeedback,
  SubmitFeedbackCommand,
} from '../../models/interview.models';
import { InterviewService } from '../../services/interview.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-interview-feedback-form',
  template: `
    <div class="page-header">
      <h1>משוב ראיון #{{ interview?.id }}</h1>
      <igds-button variant="secondary" [routerLink]="['/interviews']">
        חזרה לרשימה
      </igds-button>
    </div>

    <igds-card *ngIf="interview">
      <div igds-card-header>
        <h2 class="card-title">פרטי ראיון</h2>
      </div>
      <div igds-card-body>
        <div class="info-row">
          <span>תאריך: {{ interview.scheduledDate | hebrewDate }}</span>
          <span>שעה: {{ formatTime(interview.startTime) }} - {{ formatTime(interview.endTime) }}</span>
          <span>מיקום: {{ interview.location || '—' }}</span>
          <span>מועמדות: {{ interview.candidacyId }}</span>
        </div>
      </div>
    </igds-card>

    <igds-card>
      <div igds-card-header>
        <h2 class="card-title">הזנת משוב</h2>
      </div>
      <div igds-card-body>
        <form [formGroup]="feedbackForm" (ngSubmit)="onSubmitFeedback()" class="feedback-form">
          <igds-input-field
            label="מזהה מראיין"
            type="number"
            formControlName="interviewerId"
            [required]="true"
            [error]="feedbackForm.get('interviewerId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="דירוג (0-10)"
            type="number"
            formControlName="rating"
            [required]="true"
            [error]="getRatingError()">
          </igds-input-field>

          <igds-input-field
            label="הערות"
            formControlName="comments">
          </igds-input-field>

          <igds-button variant="primary" type="submit"
                       [disabled]="feedbackForm.invalid || submitting">
            שליחת משוב
          </igds-button>
        </form>
      </div>
    </igds-card>

    <igds-card>
      <div igds-card-header>
        <h2 class="card-title">משובים שהוזנו</h2>
      </div>
      <div igds-card-body>
        <app-loading-spinner [loading]="loading"></app-loading-spinner>

        <igds-table
          *ngIf="!loading"
          [data]="paginatedData"
          [columns]="feedbackColumns"
          emptyMessage="לא הוזנו משובים עדיין">
          <ng-template igdsTableCell="comments" let-row>
            {{ row.comments || '—' }}
          </ng-template>
          <ng-template igdsTableCell="submittedAt" let-row>
            {{ row.submittedAt | hebrewDate }}
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
    .info-row {
      display: flex;
      gap: var(--igds-space-24);
      flex-wrap: wrap;
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    .feedback-form {
      display: flex;
      gap: var(--igds-space-12);
      flex-wrap: wrap;
      align-items: flex-start;
    }
    igds-card + igds-card { margin-block-start: var(--igds-space-16); }
  `],
})
export class InterviewFeedbackFormComponent implements OnInit {
  feedbackColumns: IgdsTableColumn[] = [
    { key: 'interviewerId', label: 'מראיין' },
    { key: 'rating', label: 'דירוג' },
    { key: 'comments', label: 'הערות' },
    { key: 'submittedAt', label: 'תאריך הגשה' },
  ];
  allFeedback: InterviewFeedback[] = [];
  paginatedData: InterviewFeedback[] = [];
  interview: Interview | null = null;
  loading = false;
  submitting = false;
  feedbackForm!: FormGroup;
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;

  constructor(
    private interviewService: InterviewService,
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.feedbackForm = this.fb.group({
      interviewerId: [null, Validators.required],
      rating: [null, [Validators.required, Validators.min(0), Validators.max(10)]],
      comments: [''],
    });

    const interviewId = Number(this.route.snapshot.paramMap.get('id'));
    if (interviewId) {
      this.loadInterview(interviewId);
      this.loadFeedback(interviewId);
    }
  }

  getRatingError(): string {
    const ctrl = this.feedbackForm.get('rating');
    if (ctrl?.hasError('required')) return 'שדה חובה';
    if (ctrl?.hasError('min')) return 'מינימום 0';
    if (ctrl?.hasError('max')) return 'מקסימום 10';
    return '';
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyPagination();
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    this.paginatedData = this.allFeedback.slice(start, start + this.pageSize);
  }

  formatTime(time: string): string {
    if (!time) return '—';
    const parts = time.split(':');
    return parts.length >= 2 ? `${parts[0]}:${parts[1]}` : time;
  }

  loadInterview(id: number): void {
    this.interviewService.getById(id).subscribe({
      next: (interview) => (this.interview = interview),
      error: () => this.notification.error('שגיאה בטעינת פרטי הראיון'),
    });
  }

  loadFeedback(interviewId: number): void {
    this.loading = true;
    this.interviewService.getFeedback(interviewId).subscribe({
      next: (feedback) => {
        this.allFeedback = feedback;
        this.totalItems = feedback.length;
        this.currentPage = 1;
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת משובים');
        this.loading = false;
      },
    });
  }

  onSubmitFeedback(): void {
    if (this.feedbackForm.invalid || !this.interview) return;

    this.submitting = true;
    const command: SubmitFeedbackCommand = {
      interviewId: this.interview.id,
      interviewerId: this.feedbackForm.value.interviewerId,
      rating: this.feedbackForm.value.rating,
      comments: this.feedbackForm.value.comments || undefined,
    };

    this.interviewService.submitFeedback(command).subscribe({
      next: () => {
        this.notification.success('המשוב נשלח בהצלחה');
        this.feedbackForm.reset();
        this.loadFeedback(this.interview!.id);
        this.submitting = false;
      },
      error: () => {
        this.notification.error('שגיאה בשליחת המשוב');
        this.submitting = false;
      },
    });
  }
}
