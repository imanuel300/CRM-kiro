import { Component, OnInit } from '@angular/core';
import { ExpiringTenure } from '../../models/tenure.models';
import { TenureApiService } from '../../services/tenure.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-tenure-alerts',
  template: `
    <div class="page-header">
      <h1>התראות כהונות</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div *ngIf="!loading && expiringTenures.length === 0" class="no-alerts">
      <igds-status-badge variant="success" label="✓"></igds-status-badge>
      <p>אין כהונות שמתקרבות לסיום</p>
    </div>

    <div class="alerts-grid" *ngIf="!loading">
      <igds-card *ngFor="let tenure of expiringTenures"
                 [ngClass]="getAlertClass(tenure.daysUntilExpiry)">
        <div igds-card-header>
          <div class="alert-header">
            <igds-status-badge
              [variant]="getBadgeVariant(tenure.daysUntilExpiry)"
              [label]="getAlertIcon(tenure.daysUntilExpiry)">
            </igds-status-badge>
            <div>
              <h3 class="tenure-title">{{ tenure.position }}</h3>
              <p class="tenure-subtitle">איש קשר: {{ tenure.contactId }} | יחידה: {{ tenure.orgUnitId }}</p>
            </div>
          </div>
        </div>
        <div igds-card-body>
          <p>תאריך סיום צפוי: {{ tenure.expectedEndDate | hebrewDate }}</p>
          <p class="days-label" [ngClass]="getDaysClass(tenure.daysUntilExpiry)">
            {{ tenure.daysUntilExpiry }} ימים לסיום
          </p>
        </div>
      </igds-card>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      margin-block-end: var(--igds-space-16);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .no-alerts {
      text-align: center;
      padding: var(--igds-space-48);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .alerts-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: var(--igds-space-16);
    }
    .alert-header {
      display: flex;
      align-items: center;
      gap: var(--igds-space-12);
    }
    .tenure-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .tenure-subtitle {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-sm);
      color: var(--igds-text-secondary);
      margin: var(--igds-space-4) 0 0;
    }
    .alert-critical { border-inline-end: 4px solid var(--igds-border-error); }
    .alert-warning { border-inline-end: 4px solid var(--igds-border-warning); }
    .alert-info { border-inline-end: 4px solid var(--igds-border-info); }
    .days-critical { color: var(--igds-text-error); font-weight: var(--igds-font-weight-bold); }
    .days-warning { color: var(--igds-text-warning); font-weight: var(--igds-font-weight-bold); }
    .days-info { color: var(--igds-text-info); }
    .days-label {
      font-size: var(--igds-font-size-md);
      margin-block-start: var(--igds-space-8);
      font-family: var(--igds-font-family);
    }
  `],
})
export class TenureAlertsComponent implements OnInit {
  expiringTenures: ExpiringTenure[] = [];
  loading = false;

  constructor(
    private tenureApi: TenureApiService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadExpiring();
  }

  loadExpiring(): void {
    this.loading = true;
    this.tenureApi.getExpiring(60).subscribe({
      next: (data: ExpiringTenure[]) => {
        this.expiringTenures = data.sort((a, b) => a.daysUntilExpiry - b.daysUntilExpiry);
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת התראות כהונות');
        this.loading = false;
      },
    });
  }

  getAlertClass(days: number): string {
    if (days <= 7) return 'alert-critical';
    if (days <= 30) return 'alert-warning';
    return 'alert-info';
  }

  getBadgeVariant(days: number): string {
    if (days <= 7) return 'failure';
    if (days <= 30) return 'warning';
    return 'info';
  }

  getAlertIcon(days: number): string {
    if (days <= 7) return '⚠️';
    if (days <= 30) return '⚡';
    return 'ℹ️';
  }

  getDaysClass(days: number): string {
    if (days <= 7) return 'days-critical';
    if (days <= 30) return 'days-warning';
    return 'days-info';
  }
}
