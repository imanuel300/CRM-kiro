import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { StatusHistory } from '../../models/candidacy.models';
import { CandidacyService } from '../../services/candidacy.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsStep } from '@igds/angular';

@Component({
  selector: 'app-status-timeline',
  template: `
    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div class="timeline" *ngIf="!loading">
      <div *ngIf="history.length === 0" class="no-data">
        אין היסטוריית שינויי סטטוס
      </div>

      <igds-step-indicator
        *ngIf="history.length > 0"
        [steps]="timelineSteps"
        [activeStep]="history.length - 1">
      </igds-step-indicator>

      <div class="timeline-entries" *ngIf="history.length > 0">
        <igds-card *ngFor="let entry of history; let i = index"
          class="timeline-entry"
          [class.timeline-entry--active]="i === history.length - 1">
          <div igds-card-header class="timeline-header">
            <span class="timeline-date">{{ entry.changedAt | hebrewDate }}</span>
            <span class="timeline-user">משתמש: {{ entry.changedByUserId }}</span>
          </div>
          <div class="timeline-body">
            <div class="status-change">
              <igds-status-badge
                *ngIf="entry.fromStatusId"
                variant="neutral"
                [text]="'סטטוס ' + entry.fromStatusId">
              </igds-status-badge>
              <span *ngIf="entry.fromStatusId" class="arrow-icon" aria-hidden="true">←</span>
              <igds-status-badge
                variant="info"
                [text]="'סטטוס ' + entry.toStatusId">
              </igds-status-badge>
            </div>
            <div class="timeline-reason" *ngIf="entry.reason">
              <strong>סיבה:</strong> {{ entry.reason }}
            </div>
          </div>
        </igds-card>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .timeline {
      position: relative;
      padding-inline-end: var(--igds-space-24);
      font-family: var(--igds-font-family);
    }
    .timeline-entries {
      display: flex;
      flex-direction: column;
      gap: var(--igds-space-12);
      margin-block-start: var(--igds-space-24);
    }
    .timeline-entry {
      transition: box-shadow var(--igds-transition-fast);
    }
    .timeline-entry--active {
      box-shadow: var(--igds-shadow-md);
    }
    .timeline-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: var(--igds-font-size-xs);
      color: var(--igds-text-secondary);
    }
    .timeline-body {
      display: flex;
      flex-direction: column;
      gap: var(--igds-space-8);
    }
    .status-change {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
    }
    .arrow-icon {
      font-size: var(--igds-font-size-lg);
      color: var(--igds-text-secondary);
    }
    .timeline-reason {
      font-size: var(--igds-font-size-sm);
      color: var(--igds-text-secondary);
    }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
    }
  `],
})
export class StatusTimelineComponent implements OnChanges {
  @Input() candidacyId!: number;

  history: StatusHistory[] = [];
  timelineSteps: IgdsStep[] = [];
  loading = false;

  constructor(
    private candidacyService: CandidacyService,
    private notification: NotificationService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['candidacyId'] && this.candidacyId) {
      this.loadHistory();
    }
  }

  private loadHistory(): void {
    this.loading = true;
    this.candidacyService.getStatusHistory(this.candidacyId).subscribe({
      next: (data: StatusHistory[]) => {
        this.history = data;
        this.timelineSteps = this.buildSteps(data);
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת היסטוריית סטטוסים');
        this.loading = false;
      },
    });
  }

  private buildSteps(history: StatusHistory[]): IgdsStep[] {
    return history.map((entry, index) => ({
      label: `סטטוס ${entry.toStatusId}`,
      completed: index < history.length - 1,
    }));
  }
}
