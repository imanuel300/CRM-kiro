import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  Committee,
  CommitteeMeeting,
  CommitteeDecision,
  CommitteeDecisionType,
  MeetingStatus,
  RecordDecisionCommand,
  SubmitCommitteeAppealCommand,
} from '../../models/committee.models';
import { CommitteeService } from '../../services/committee.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTableColumn } from '@igds/angular';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-committee-meeting',
  template: `
    <div class="page-header">
      <h1>ישיבת ועדה — {{ committee?.name }}</h1>
      <igds-button variant="secondary" (onClick)="goBack()">
        חזרה לרשימה
      </igds-button>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <ng-container *ngIf="!loading && meeting">
      <!-- Meeting Info -->
      <igds-card class="section-card">
        <h2 igds-card-header>פרטי ישיבה</h2>
        <div class="info-grid">
          <div class="info-item">
            <span class="label">תאריך:</span>
            <span>{{ meeting.scheduledDate | hebrewDate }}</span>
          </div>
          <div class="info-item">
            <span class="label">מיקום:</span>
            <span>{{ meeting.location || '—' }}</span>
          </div>
          <div class="info-item">
            <span class="label">סטטוס:</span>
            <igds-tag
              [label]="getMeetingStatusName(meeting.status)"
              [variant]="getMeetingStatusVariant(meeting.status)">
            </igds-tag>
          </div>
          <div class="info-item">
            <span class="label">מועמדויות לדיון:</span>
            <span>{{ meeting.candidacyIds.length || 0 }}</span>
          </div>
        </div>
      </igds-card>

      <!-- Candidacy IDs list -->
      <igds-card class="section-card">
        <h2 igds-card-header>מועמדויות לדיון</h2>
        <div *ngIf="meeting.candidacyIds?.length; else noCandidacies" class="candidacy-tags">
          <igds-tag *ngFor="let cid of meeting.candidacyIds"
            [label]="'מועמדות #' + cid">
          </igds-tag>
        </div>
        <ng-template #noCandidacies>
          <p class="empty-message">אין מועמדויות משויכות לישיבה זו</p>
        </ng-template>
      </igds-card>

      <!-- Decision Form -->
      <igds-card class="section-card">
        <h2 igds-card-header>רישום החלטה</h2>
        <form [formGroup]="decisionForm" (ngSubmit)="onSubmitDecision()" class="decision-form">
          <igds-dropdown
            label="מועמדות"
            placeholder="בחר מועמדות"
            formControlName="candidacyId"
            [options]="candidacyOptions"
            [required]="true"
            [error]="decisionForm.get('candidacyId')?.touched && decisionForm.get('candidacyId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-dropdown>

          <igds-dropdown
            label="החלטה"
            placeholder="בחר החלטה"
            formControlName="decision"
            [options]="decisionTypeOptions"
            [required]="true"
            [error]="decisionForm.get('decision')?.touched && decisionForm.get('decision')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-dropdown>

          <igds-input-field
            label="מחליט (מזהה)"
            type="number"
            formControlName="decidedBy"
            [required]="true"
            [error]="decisionForm.get('decidedBy')?.touched && decisionForm.get('decidedBy')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="המלצה"
            formControlName="recommendation"
            class="recommendation-field">
          </igds-input-field>

          <igds-button variant="primary" type="submit"
                       [disabled]="decisionForm.invalid || submittingDecision">
            רישום החלטה
          </igds-button>
        </form>
      </igds-card>

      <!-- Decisions Table -->
      <igds-card class="section-card">
        <h2 igds-card-header>החלטות</h2>
        <igds-table
          [columns]="decisionTableColumns"
          [data]="pagedDecisions"
          [sortColumn]="sortColumn"
          [sortDirection]="sortDirection"
          (sort)="onSort($event)">
        </igds-table>

        <p *ngIf="allDecisions.length === 0" class="empty-message">לא נרשמו החלטות</p>

        <igds-pagination
          *ngIf="allDecisions.length > 0"
          [totalItems]="allDecisions.length"
          [pageSize]="pageSize"
          [currentPage]="currentPage"
          (pageChange)="onPageChange($event)">
        </igds-pagination>
      </igds-card>

      <!-- Appeal Form -->
      <igds-card class="section-card">
        <h2 igds-card-header>הגשת ערעור</h2>
        <form [formGroup]="appealForm" (ngSubmit)="onSubmitAppeal()" class="appeal-form">
          <igds-input-field
            label="מועמדות"
            type="number"
            formControlName="candidacyId"
            [required]="true"
            [error]="appealForm.get('candidacyId')?.touched && appealForm.get('candidacyId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="סיבת ערעור"
            formControlName="reason"
            [required]="true"
            class="reason-field"
            [error]="appealForm.get('reason')?.touched && appealForm.get('reason')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-button variant="secondary" type="submit"
                       [disabled]="appealForm.invalid || submittingAppeal">
            הגשת ערעור
          </igds-button>
        </form>
      </igds-card>
    </ng-container>
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
    .section-card {
      margin-block-end: var(--igds-space-16);
    }
    .info-grid {
      display: flex;
      gap: var(--igds-space-24);
      flex-wrap: wrap;
    }
    .info-item {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary);
    }
    .info-item .label {
      font-weight: var(--igds-font-weight-medium);
    }
    .candidacy-tags {
      display: flex;
      flex-wrap: wrap;
      gap: var(--igds-space-8);
    }
    .empty-message {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      color: var(--igds-text-secondary);
      text-align: center;
      padding: var(--igds-space-24);
    }
    .decision-form, .appeal-form {
      display: flex;
      gap: var(--igds-space-12);
      flex-wrap: wrap;
      align-items: flex-start;
    }
    .decision-form igds-dropdown,
    .decision-form igds-input-field,
    .appeal-form igds-input-field { flex: 1; min-width: 150px; }
    .recommendation-field, .reason-field { flex: 2 !important; min-width: 250px; }
    .decision-form igds-button,
    .appeal-form igds-button {
      align-self: flex-end;
      margin-block-start: var(--igds-space-8);
    }
  `],
})
export class CommitteeMeetingComponent implements OnInit {
  readonly CommitteeDecisionType = CommitteeDecisionType;
  readonly MeetingStatus = MeetingStatus;

  committee: Committee | null = null;
  meeting: CommitteeMeeting | null = null;
  loading = false;
  submittingDecision = false;
  submittingAppeal = false;

  decisionForm!: FormGroup;
  appealForm!: FormGroup;

  // Table data management (replaces MatTableDataSource)
  allDecisions: CommitteeDecision[] = [];
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  currentPage = 1;
  pageSize = 10;

  decisionTableColumns: IgdsTableColumn[] = [
    { key: 'candidacyId', label: 'מועמדות', sortable: true },
    { key: 'decisionLabel', label: 'החלטה', sortable: true },
    { key: 'recommendation', label: 'המלצה' },
    { key: 'decidedBy', label: 'מחליט', sortable: true },
    { key: 'decidedAtFormatted', label: 'תאריך החלטה', sortable: true },
  ];

  candidacyOptions: IgdsDropdownOption[] = [];
  decisionTypeOptions: IgdsDropdownOption[] = [
    { value: CommitteeDecisionType.Accepted, label: 'קבלה' },
    { value: CommitteeDecisionType.Rejected, label: 'דחייה' },
    { value: CommitteeDecisionType.Deferred, label: 'דחייה לדיון נוסף' },
  ];

  private committeeId!: number;
  private meetingId!: number;

  constructor(
    private committeeService: CommitteeService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.decisionForm = this.fb.group({
      candidacyId: [null, Validators.required],
      decision: [null, Validators.required],
      decidedBy: [null, Validators.required],
      recommendation: [''],
    });

    this.appealForm = this.fb.group({
      candidacyId: [null, Validators.required],
      reason: ['', Validators.required],
    });

    this.committeeId = Number(this.route.snapshot.paramMap.get('committeeId'));
    this.meetingId = Number(this.route.snapshot.paramMap.get('meetingId'));

    if (this.committeeId && this.meetingId) {
      this.loadData();
    }
  }

  get pagedDecisions(): any[] {
    const mapped = this.getSortedDecisions().map(d => ({
      ...d,
      decisionLabel: this.getDecisionName(d.decision),
      decidedAtFormatted: d.decidedAt ? new Date(d.decidedAt).toLocaleDateString('he-IL') : '—',
      recommendation: d.recommendation || '—',
    }));
    const start = (this.currentPage - 1) * this.pageSize;
    return mapped.slice(start, start + this.pageSize);
  }

  loadData(): void {
    this.loading = true;
    this.committeeService.getById(this.committeeId).subscribe({
      next: (c: Committee) => {
        this.committee = c;
      },
      error: () => this.notification.error('שגיאה בטעינת פרטי הוועדה'),
    });

    this.committeeService.getMeeting(this.committeeId, this.meetingId).subscribe({
      next: (m: CommitteeMeeting) => {
        this.meeting = m;
        this.candidacyOptions = (m.candidacyIds || []).map(cid => ({
          value: cid,
          label: 'מועמדות #' + cid,
        }));
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת פרטי הישיבה');
        this.loading = false;
      },
    });

    this.loadDecisions();
  }

  loadDecisions(): void {
    this.committeeService.getDecisions(this.committeeId, this.meetingId).subscribe({
      next: (decisions: CommitteeDecision[]) => {
        this.allDecisions = decisions;
        this.currentPage = 1;
      },
      error: () => this.notification.error('שגיאה בטעינת החלטות'),
    });
  }

  onSort(event: { column: string; direction: 'asc' | 'desc' }): void {
    this.sortColumn = event.column;
    this.sortDirection = event.direction;
  }

  onPageChange(page: number): void {
    this.currentPage = page;
  }

  onSubmitDecision(): void {
    if (this.decisionForm.invalid || !this.meeting) return;

    this.submittingDecision = true;
    const command: RecordDecisionCommand = {
      meetingId: this.meetingId,
      candidacyId: this.decisionForm.value.candidacyId,
      decision: this.decisionForm.value.decision,
      decidedBy: this.decisionForm.value.decidedBy,
      recommendation: this.decisionForm.value.recommendation || undefined,
    };

    this.committeeService.recordDecision(this.committeeId, this.meetingId, command).subscribe({
      next: () => {
        this.notification.success('ההחלטה נרשמה בהצלחה');
        this.decisionForm.reset();
        this.loadDecisions();
        this.submittingDecision = false;
      },
      error: () => {
        this.notification.error('שגיאה ברישום ההחלטה');
        this.submittingDecision = false;
      },
    });
  }

  onSubmitAppeal(): void {
    if (this.appealForm.invalid || !this.meeting) return;

    this.submittingAppeal = true;
    const command: SubmitCommitteeAppealCommand = {
      meetingId: this.meetingId,
      candidacyId: this.appealForm.value.candidacyId,
      reason: this.appealForm.value.reason,
    };

    this.committeeService.submitAppeal(this.committeeId, this.meetingId, command).subscribe({
      next: () => {
        this.notification.success('הערעור הוגש בהצלחה');
        this.appealForm.reset();
        this.submittingAppeal = false;
      },
      error: () => {
        this.notification.error('שגיאה בהגשת הערעור');
        this.submittingAppeal = false;
      },
    });
  }

  getMeetingStatusName(status: MeetingStatus): string {
    switch (status) {
      case MeetingStatus.Scheduled: return 'מתוזמנת';
      case MeetingStatus.InProgress: return 'בתהליך';
      case MeetingStatus.Completed: return 'הושלמה';
      default: return '';
    }
  }

  getMeetingStatusVariant(status: MeetingStatus): 'default' | 'success' | 'warning' | 'failure' {
    switch (status) {
      case MeetingStatus.Scheduled: return 'warning';
      case MeetingStatus.InProgress: return 'default';
      case MeetingStatus.Completed: return 'success';
      default: return 'default';
    }
  }

  getDecisionName(decision: CommitteeDecisionType): string {
    switch (decision) {
      case CommitteeDecisionType.Accepted: return 'התקבל';
      case CommitteeDecisionType.Rejected: return 'נדחה';
      case CommitteeDecisionType.Deferred: return 'נדחה לדיון נוסף';
      default: return '';
    }
  }

  goBack(): void {
    this.router.navigate(['/committees']);
  }

  private getSortedDecisions(): CommitteeDecision[] {
    if (!this.sortColumn) return [...this.allDecisions];
    return [...this.allDecisions].sort((a, b) => {
      const valA = (a as any)[this.sortColumn];
      const valB = (b as any)[this.sortColumn];
      if (valA == null && valB == null) return 0;
      if (valA == null) return 1;
      if (valB == null) return -1;
      const cmp = valA < valB ? -1 : valA > valB ? 1 : 0;
      return this.sortDirection === 'asc' ? cmp : -cmp;
    });
  }
}
