import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  Interview,
  InterviewQueryParams,
  InterviewStatus,
  InterviewType,
} from '../../models/interview.models';
import { InterviewService } from '../../services/interview.service';
import { NotificationService } from '@core/services/notification.service';

interface DayEntry {
  date: Date;
  label: string;
  interviews: Interview[];
  isToday: boolean;
  isCurrentMonth: boolean;
}

@Component({
  selector: 'app-interview-schedule',
  template: `
    <div class="page-header">
      <h1>לוח זמנים — ראיונות</h1>
      <igds-button variant="secondary" [routerLink]="['/interviews']">
        תצוגת רשימה
      </igds-button>
    </div>

    <igds-card>
      <div igds-card-body>
        <div class="filters-row">
          <igds-input-field
            label="יחידה ארגונית"
            type="number"
            [ngModel]="filters.orgUnitId"
            (ngModelChange)="filters.orgUnitId = $event; loadData()"
            placeholder="מזהה יחידה">
          </igds-input-field>

          <igds-input-field
            label="קול קורא"
            type="number"
            [ngModel]="filters.callForCandidatesId"
            (ngModelChange)="filters.callForCandidatesId = $event; loadData()"
            placeholder="מזהה קול קורא">
          </igds-input-field>
        </div>

        <div class="calendar-nav">
          <igds-button variant="secondary" [iconOnly]="true" (onClick)="prevMonth()">
            ▶
          </igds-button>
          <h2 class="month-label">{{ monthLabel }}</h2>
          <igds-button variant="secondary" [iconOnly]="true" (onClick)="nextMonth()">
            ◀
          </igds-button>
          <igds-button variant="secondary" (onClick)="goToToday()">היום</igds-button>
        </div>

        <app-loading-spinner [loading]="loading"></app-loading-spinner>

        <div class="calendar-grid" *ngIf="!loading">
          <div class="day-header" *ngFor="let d of dayHeaders">{{ d }}</div>
          <div
            *ngFor="let day of days"
            class="day-cell"
            [class.today]="day.isToday"
            [class.other-month]="!day.isCurrentMonth"
          >
            <span class="day-number">{{ day.label }}</span>
            <div
              *ngFor="let iv of day.interviews"
              class="interview-chip"
              [class.completed]="iv.status === InterviewStatus.Completed"
              [class.cancelled]="iv.status === InterviewStatus.Cancelled"
              (click)="goToFeedback(iv)"
              igdsTooltip="{{ getTypeName(iv.interviewType) }} | מועמדות {{ iv.candidacyId }}"
            >
              {{ formatTime(iv.startTime) }} #{{ iv.candidacyId }}
            </div>
          </div>
        </div>
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
    .calendar-nav {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      margin-block-end: var(--igds-space-12);
    }
    .month-label {
      margin: 0;
      min-width: 160px;
      text-align: center;
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    .calendar-grid {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: 1px;
      background: var(--igds-border-divider);
      border: 1px solid var(--igds-border-divider);
      direction: rtl;
    }
    .day-header {
      background: var(--igds-bg-neutral-secondlevel);
      padding: var(--igds-space-8);
      text-align: center;
      font-weight: var(--igds-font-weight-medium);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    .day-cell {
      background: var(--igds-bg-neutral);
      min-height: 80px;
      padding: var(--igds-space-4);
      position: relative;
    }
    .day-cell.today { background: var(--igds-bg-brand-light); }
    .day-cell.other-month { background: var(--igds-bg-neutral-secondlevel); }
    .day-cell.other-month .day-number { color: var(--igds-text-disabled); }
    .day-number {
      font-size: var(--igds-font-size-xs);
      font-weight: var(--igds-font-weight-medium);
      display: block;
      margin-block-end: 2px;
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    .interview-chip {
      font-size: var(--igds-font-size-xs);
      padding: 2px var(--igds-space-4);
      margin-block-end: 2px;
      border-radius: var(--igds-radius-sm);
      background: var(--igds-bg-info-light);
      cursor: pointer;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      font-family: var(--igds-font-family);
    }
    .interview-chip:hover { background: var(--igds-bg-info); }
    .interview-chip.completed { background: var(--igds-bg-success-light); }
    .interview-chip.cancelled { background: var(--igds-bg-error-light); text-decoration: line-through; }
  `],
})
export class InterviewScheduleComponent implements OnInit {
  readonly InterviewStatus = InterviewStatus;

  filters: InterviewQueryParams = {};
  loading = false;
  currentDate = new Date();
  monthLabel = '';
  dayHeaders = ['ראשון', 'שני', 'שלישי', 'רביעי', 'חמישי', 'שישי', 'שבת'];
  days: DayEntry[] = [];

  private interviews: Interview[] = [];

  constructor(
    private interviewService: InterviewService,
    private router: Router,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.buildCalendar();
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.interviewService.list(this.filters).subscribe({
      next: (data) => {
        this.interviews = data;
        this.buildCalendar();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת ראיונות');
        this.loading = false;
      },
    });
  }

  prevMonth(): void {
    this.currentDate = new Date(
      this.currentDate.getFullYear(),
      this.currentDate.getMonth() - 1,
      1
    );
    this.buildCalendar();
  }

  nextMonth(): void {
    this.currentDate = new Date(
      this.currentDate.getFullYear(),
      this.currentDate.getMonth() + 1,
      1
    );
    this.buildCalendar();
  }

  goToToday(): void {
    this.currentDate = new Date();
    this.buildCalendar();
  }

  goToFeedback(interview: Interview): void {
    this.router.navigate(['/interviews', interview.id, 'feedback']);
  }

  formatTime(time: string): string {
    if (!time) return '';
    const parts = time.split(':');
    return parts.length >= 2 ? `${parts[0]}:${parts[1]}` : time;
  }

  getTypeName(type: InterviewType): string {
    return type === InterviewType.Second ? 'ראיון שני' : 'ראיון ראשון';
  }

  private buildCalendar(): void {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth();
    this.monthLabel = new Date(year, month, 1).toLocaleDateString('he-IL', {
      year: 'numeric',
      month: 'long',
    });

    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const startDow = firstDay.getDay();

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const days: DayEntry[] = [];

    for (let i = startDow - 1; i >= 0; i--) {
      const d = new Date(year, month, -i);
      days.push(this.makeDayEntry(d, month, today));
    }

    for (let d = 1; d <= lastDay.getDate(); d++) {
      days.push(this.makeDayEntry(new Date(year, month, d), month, today));
    }

    while (days.length % 7 !== 0) {
      const d = new Date(year, month + 1, days.length - lastDay.getDate() - startDow + 1);
      days.push(this.makeDayEntry(d, month, today));
    }

    this.days = days;
  }

  private makeDayEntry(date: Date, currentMonth: number, today: Date): DayEntry {
    const dateStr = this.toDateString(date);
    return {
      date,
      label: date.getDate().toString(),
      isToday: date.getTime() === today.getTime(),
      isCurrentMonth: date.getMonth() === currentMonth,
      interviews: this.interviews.filter((iv) => {
        const ivDate = iv.scheduledDate?.substring(0, 10);
        return ivDate === dateStr;
      }),
    };
  }

  private toDateString(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}
